using System;
using System.Text;
using System.Windows.Forms;
using Pulsar.Common.Plugins;

namespace Pulsar.Plugin.MessageBox.Client
{
    /// <summary>
    /// Client-side plugin that displays message boxes and returns user's response
    /// </summary>
    public class MessageBoxClientPlugin : IUniversalPlugin
    {
        public string PluginId => "messagebox-plugin";
        public string Version => "1.0.0";
        public string[] SupportedCommands => new[] { "show-message" };

        private bool _isComplete = false;
        public bool IsComplete => _isComplete;

        public void Initialize(Object initData)
        {
            // Plugin loaded and ready
            // initData could contain default configuration if needed
        }

        public PluginResult ExecuteCommand(string command, Object parameters)
        {
            try
            {
                if (command == "show-message")
                {
                    return ShowMessageBox(parameters);
                }

                return new PluginResult
                {
                    Success = false,
                    Message = $"Unknown command: {command}"
                };
            }
            catch (Exception ex)
            {
                return new PluginResult
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        private PluginResult ShowMessageBox(Object parameters)
        {
            try
            {
                byte[] bytes = parameters as byte[];

                string message = parameters != null && bytes.Length > 0
                    ? Encoding.UTF8.GetString(bytes)
                    : "Default message from Pulsar";

                DialogResult result = System.Windows.Forms.MessageBox.Show(
                    message,
                    "Message from Pulsar Server",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                string response = result == DialogResult.Yes ? "Yes" : "No";
                string responseMessage = $"User clicked: {response}";

                return new PluginResult
                {
                    Success = true,
                    Message = responseMessage,
                    Data = Encoding.UTF8.GetBytes(response),
                    ShouldUnload = false
                };
            }
            catch (Exception ex)
            {
                return new PluginResult
                {
                    Success = false,
                    Message = $"Error showing message box: {ex.Message}"
                };
            }
        }

        public void Cleanup()
        {
            _isComplete = true;
        }
    }
}