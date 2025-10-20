using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
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
        private Dictionary<string, string> _pendingMessages = new Dictionary<string, string>();

        public void Initialize(IServerContext context)
        {
            _context = context;

            UniversalPluginResponseHandler.ResponseReceived += OnPluginResponse;

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
                System.Windows.Forms.MessageBox.Show(
                    "Please select at least one client.",
                    "No Client Selected",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            using (var inputForm = new Form())
            {
                inputForm.Text = "Send Message Box";
                inputForm.Size = new System.Drawing.Size(400, 180);
                inputForm.StartPosition = FormStartPosition.CenterParent;
                inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                inputForm.MaximizeBox = false;
                inputForm.MinimizeBox = false;

                var label = new Label
                {
                    Text = "Enter the message to display on the client:",
                    Location = new System.Drawing.Point(10, 10),
                    Size = new System.Drawing.Size(370, 20)
                };

                var textBox = new TextBox
                {
                    Location = new System.Drawing.Point(10, 35),
                    Size = new System.Drawing.Size(360, 60),
                    Multiline = true,
                    Text = "This is a test message from the Pulsar server. Do you agree?"
                };

                var btnSend = new Button
                {
                    Text = "Send",
                    Location = new System.Drawing.Point(10, 105),
                    Size = new System.Drawing.Size(100, 30),
                    DialogResult = DialogResult.OK
                };

                var btnCancel = new Button
                {
                    Text = "Cancel",
                    Location = new System.Drawing.Point(120, 105),
                    Size = new System.Drawing.Size(100, 30),
                    DialogResult = DialogResult.Cancel
                };

                inputForm.Controls.AddRange(new Control[] { label, textBox, btnSend, btnCancel });
                inputForm.AcceptButton = btnSend;
                inputForm.CancelButton = btnCancel;

                _context.ApplyTheme(f => f.BackColor = inputForm.BackColor);

                if (inputForm.ShowDialog(_context.MainForm) == DialogResult.OK)
                {
                    string message = textBox.Text;
                    if (string.IsNullOrWhiteSpace(message))
                    {
                        System.Windows.Forms.MessageBox.Show("Please enter a message.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    foreach (var client in clients)
                    {
                        SendMessageBoxCommand(client, message);
                    }

                    _context.Log($"Sent message box command to {clients.Count} client(s)");
                }
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
                _pendingMessages[key] = message;

                _context.Log($"Sent message box to {client.EndPoint}");
            }
            catch (Exception ex)
            {
                _context.Log($"Error sending message box to {client.EndPoint}: {ex.Message}");
                System.Windows.Forms.MessageBox.Show(
                    $"Failed to send message box: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void OnPluginResponse(PluginResponse response)
        {
            if (response.PluginId != "messagebox-plugin")
                return;

            try
            {
                string clientKey = response.Client.EndPoint.ToString();

                if (response.Success && response.Data != null)
                {
                    string userChoice = Encoding.UTF8.GetString(response.Data);

                    _pendingMessages.TryGetValue(clientKey, out string originalMessage);
                    _pendingMessages.Remove(clientKey);

                    _context.MainForm.Invoke(new Action(() =>
                    {
                        string displayMessage = $"Client: {response.Client.EndPoint}\n\n";

                        if (!string.IsNullOrEmpty(originalMessage))
                        {
                            displayMessage += $"Original Message:\n{originalMessage}\n\n";
                        }

                        displayMessage += $"User Response: {userChoice}";

                        System.Windows.Forms.MessageBox.Show(
                            displayMessage,
                            "Client Response - MessageBox Plugin",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                    }));

                    _context.Log($"Client {response.Client.EndPoint} clicked: {userChoice}");
                }
                else
                {
                    _pendingMessages.Remove(clientKey);
                    _context.Log($"Client {response.Client.EndPoint} error: {response.Message}");

                    _context.MainForm.Invoke(new Action(() =>
                    {
                        System.Windows.Forms.MessageBox.Show(
                            $"Error from client {response.Client.EndPoint}:\n{response.Message}",
                            "Client Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }));
                }
            }
            catch (Exception ex)
            {
                _context.Log($"Error processing response: {ex.Message}");
            }
        }
    }
}