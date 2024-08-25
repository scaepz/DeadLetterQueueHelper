using Azure.Messaging.ServiceBus;
using System.Linq;

namespace DeadLetterQueueHelper.State.ServiceBusLayer
{
    public class QueueMonitor
    {
        private Timer? _timer;
        private readonly List<MonitorEntry> _messagesToMonitor = new();
        private readonly ServiceBusClientProvider _clientProvider;

        public QueueMonitor(ServiceBusClientProvider clientProvider)
        {
            _clientProvider = clientProvider;
        }

        public void CallbackWhenMessageDisappeared(MonitorEntry message)
        {
            _messagesToMonitor.Add(message);
            StartMonitoring();
        }

        public void StartMonitoring()
        {
            if (_timer != null)
                return;

            _timer = new Timer(async _ => await Run(), null, dueTime: 0, period: 5000);
        }

        private async Task Run()
        {
            Console.WriteLine("Running monitor");
            if (_messagesToMonitor.Count == 0)
                return;

            var client = await _clientProvider.GetReceiver(SubQueue.None);

            if (client == null)
                return;

            var queuedMessages = await client.PeekMessagesAsync(1000, 0);

            var localCopyOfMessages = _messagesToMonitor.ToList();
            for (int i = localCopyOfMessages.Count - 1; i >= 0; i--)
            {
                var message = localCopyOfMessages[i];
                Console.WriteLine(message);

                if (queuedMessages.Any(x => x.MessageId == message.MessageId))
                    continue;

                await message.Callback(message);
                _messagesToMonitor.Remove(message);
            }
        }
    }

    public record MonitorEntry(string MessageId, long PreviousSequenceNumber, Func<MonitorEntry, Task> Callback);
}
