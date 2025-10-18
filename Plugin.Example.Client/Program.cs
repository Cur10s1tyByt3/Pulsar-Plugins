using Pulsar.Common.Plugins;
using System;
using System.Runtime.InteropServices;

public class ActionPlugin : IUniversalPlugin
{
    //messagebox
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    public string PluginId => "actionplugin";
    public string Version => "1.0";
    public string[] SupportedCommands => new string[0];

    private bool _isExecuting = false;

    public void Initialize(byte[] initData) 
    {
        var result = PerformAction();
    }

    public PluginResult ExecuteCommand(string command, byte[] parameters)
    {
        return new PluginResult { Success = false, Message = "Unknown command" };
    }

    public bool IsComplete => !_isExecuting;
    public void Cleanup() { _isExecuting = false; }

    private string PerformAction()
    {
        MessageBox(IntPtr.Zero, "Action executed!", "Action Plugin", 0);
        return "Action executed successfully.";
    }
}
