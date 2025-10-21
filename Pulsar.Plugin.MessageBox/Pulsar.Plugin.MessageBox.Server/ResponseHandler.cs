using System;
using System.Net;
using System.Text;
using Pulsar.Server.Messages;
using Pulsar.Server.Plugins;

namespace Pulsar.Plugin.MessageBox.Server
{
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
            if (response.PluginId != "messagebox-plugin")
                return;

            try
            {
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
            catch (Exception ex)
            {
                _context.Log($"Error processing response: {ex.Message}");
            }
        }

        private void HandleSuccessfulResponse(PluginResponse response, string clientKey)
        {
            string userChoice = Encoding.UTF8.GetString(response.Data);

            string originalMessage = _pendingManager.GetAndRemove(clientKey);

            _context.MainForm.Invoke(new Action(() =>
            {
                MessageBoxDisplay.ShowClientResponse(response.Client.EndPoint, originalMessage, userChoice);
            }));

            _context.Log($"Client {response.Client.EndPoint} clicked: {userChoice}");
        }

        private void HandleErrorResponse(PluginResponse response, string clientKey)
        {
            _pendingManager.Remove(clientKey);
            _context.Log($"Client {response.Client.EndPoint} error: {response.Message}");

            _context.MainForm.Invoke(new Action(() =>
            {
                MessageBoxDisplay.ShowClientError(response.Client.EndPoint, response.Message);
            }));
        }
    }
}