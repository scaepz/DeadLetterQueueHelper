using Azure.Messaging.ServiceBus;
using Blazored.LocalStorage;
using DeadLetterQueueHelper.State.Models;
using Stl.Fusion;

namespace DeadLetterQueueHelper.State.ServiceBusLayer
{
    public class QueueMonitor
    {
        private Timer? _timer;

        private readonly ServiceBusClientProvider _clientProvider;
        private readonly QueueErrors _queueErrors;
        private readonly ILocalStorageService _localStorage;

        private const string LocalStorageName = "queueMonitor";

        public delegate Task MessageEvent(object sender, MonitorEntry entry);
        public event MessageEvent? OnMessageDisappeared;

        public QueueMonitor(ServiceBusClientProvider clientProvider, QueueErrors queueErrors, ILocalStorageService localStorage)
        {
            _clientProvider = clientProvider;
            _queueErrors = queueErrors;
            _localStorage = localStorage;
        }

        public async Task Add(MonitorEntry message)
        {
            var messages = await GetMessagesToMonitor();
            messages.Add(message);
            await SetMessagesToMonitor(messages);

            StartMonitoring();
        }

        [ComputeMethod]
        protected virtual async Task<List<MonitorEntry>> GetMessagesToMonitor()
        {
            return await _localStorage.GetItemAsync<List<MonitorEntry>>(LocalStorageName) ?? [];
        }

        private async Task SetMessagesToMonitor(List<MonitorEntry> messages)
        {
            await _localStorage.SetItemAsync(LocalStorageName, messages);

            using (Computed.Invalidate())
            {
                _ = GetMessagesToMonitor();
            }
        }

        public void StartMonitoring()
        {
            if (_timer != null)
                return;

            _timer = new Timer(async _ => await Run(), null, dueTime: 0, period: 5000);
        }

        private async Task Run()
        {
            var messages = await GetMessagesToMonitor();
            Console.WriteLine(messages.Count);

            foreach (var messagesByQueue in messages.GroupBy(x => x.Queue))
            {
                var queue = messagesByQueue.Key;

                try
                {
                    var receiver = await _clientProvider.GetReceiver(queue, SubQueue.None);

                    if (receiver == null)
                        return;

                    var queuedMessages = await receiver.PeekMessagesAsync(1000, 0);

                    for (int i = messages.Count - 1; i >= 0; i--)
                    {
                        var message = messages[i];
                        if (queuedMessages.Any(x => x.MessageId == message.MessageId))
                            continue;

                        if (OnMessageDisappeared != null)
                        {
                            await OnMessageDisappeared.Invoke(this, message);
                        }
                        messages.Remove(message);
                    }
                }
                catch (Exception e)
                {
                    _queueErrors.SetError(queue, e.Message);
                }
            }
        }
    }

    public record MonitorEntry(Queue Queue, string MessageId, long PreviousSequenceNumber);
}
