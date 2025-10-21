using System.Collections.Generic;

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
}