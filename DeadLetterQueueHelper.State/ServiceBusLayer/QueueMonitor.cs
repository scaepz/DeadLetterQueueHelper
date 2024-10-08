﻿using Azure.Messaging.ServiceBus;
using DeadLetterQueueHelper.State.Models;

namespace DeadLetterQueueHelper.State.ServiceBusLayer
{
    public class QueueMonitor
    {
        private Timer? _timer;
        private readonly List<MonitorEntry> _messagesToMonitor = new();
        private readonly ServiceBusClientProvider _clientProvider;
        private readonly QueueErrors _queueErrors;

        public QueueMonitor(ServiceBusClientProvider clientProvider, QueueErrors queueErrors)
        {
            _clientProvider = clientProvider;
            _queueErrors = queueErrors;
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

            foreach (var messagesByQueue in _messagesToMonitor.GroupBy(x => x.Queue))
            {
                var queue = messagesByQueue.Key;

                try
                {
                    var receiver = await _clientProvider.GetReceiver(queue, SubQueue.None);

                    if (receiver == null)
                        return;

                    var queuedMessages = await receiver.PeekMessagesAsync(1000, 0);

                    var localCopyOfMessages = messagesByQueue.ToList();
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
                catch (Exception e)
                {
                    _queueErrors.SetError(queue, e.Message);
                }
            }
        }
    }

    public record MonitorEntry(Queue Queue, string MessageId, long PreviousSequenceNumber, Func<MonitorEntry, Task> Callback);
}
