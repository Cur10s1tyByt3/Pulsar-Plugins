using Pulsar.Common.Plugins;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ActionPlugins
{
    public sealed class ActionPlugin : IUniversalPlugin
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

        public string PluginId => "actionplugin";
        public string Version => "1.0.0";
        public string[] SupportedCommands => Array.Empty<string>();

        public void Initialize(byte[] initData)
        {
            var message = (initData != null && initData.Length > 0)
                ? Encoding.UTF8.GetString(initData)
                : "Action executed!";

            MessageBox(IntPtr.Zero, message, "Action Plugin", 0);
        }

        public PluginResult ExecuteCommand(string command, byte[] parameters)
        {
            return new PluginResult
            {
                Success = false,
                Message = "No commands supported",
                ShouldUnload = true
            };
        }

        public bool IsComplete => true;

        public void Cleanup()
        {
        }
    }
}