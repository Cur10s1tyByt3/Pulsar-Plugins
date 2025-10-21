using System;
using System.Windows.Forms;
using Pulsar.Server.Plugins;

namespace Pulsar.Plugin.MessageBox.Server
{
    public static class MessageInputDialog
    {
        public static string Show(IServerContext context)
        {
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

                context.ApplyTheme(f => f.BackColor = inputForm.BackColor);

                if (inputForm.ShowDialog(context.MainForm) == DialogResult.OK)
                {
                    string message = textBox.Text;
                    if (string.IsNullOrWhiteSpace(message))
                    {
                        MessageBoxDisplay.ShowEmptyMessageError();
                        return null;
                    }
                    return message;
                }
                return null;
            }
        }
    }
}