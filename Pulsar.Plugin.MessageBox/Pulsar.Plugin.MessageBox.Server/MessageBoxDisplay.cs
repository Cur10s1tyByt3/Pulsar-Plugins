using System;
using System.Net;
using System.Windows.Forms;

namespace Pulsar.Plugin.MessageBox.Server
{
    public static class MessageBoxDisplay
    {
        public static void ShowNoClientSelected()
        {
            System.Windows.Forms.MessageBox.Show(
                "Please select at least one client.",
                "No Client Selected",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        public static void ShowEmptyMessageError()
        {
            System.Windows.Forms.MessageBox.Show("Please enter a message.", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void ShowSendError(string errorMessage)
        {
            System.Windows.Forms.MessageBox.Show(
                $"Failed to send message box: {errorMessage}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }

        public static void ShowClientResponse(IPEndPoint endPoint, string originalMessage, string userChoice)
        {
            string displayMessage = $"Client: {endPoint}\n\n";

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
        }

        public static void ShowClientError(IPEndPoint endPoint, string errorMessage)
        {
            System.Windows.Forms.MessageBox.Show(
                $"Error from client {endPoint}:\n{errorMessage}",
                "Client Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }
}