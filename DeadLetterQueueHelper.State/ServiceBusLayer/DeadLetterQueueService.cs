﻿using Azure.Messaging.ServiceBus;
using DeadLetterQueueHelper.State.Models;
using Stl.DependencyInjection;
using Stl.Fusion;

namespace DeadLetterQueueHelper.State.ServiceBusLayer
{
    public class DeadLetterQueueService : IComputeService, IHasIsDisposed
    {
        public bool IsDisposed => false;

        private readonly ServiceBusClientProvider _clientProvider;
        private readonly QueueMonitor _queueMonitor;
        private readonly QueueErrors _queueErrors;

        public DeadLetterQueueService(ServiceBusClientProvider clientProvider, QueueMonitor queueMonitor, QueueErrors queueErrors)
        {
            _clientProvider = clientProvider;
            _queueMonitor = queueMonitor;
            _queueErrors = queueErrors;
        }

        [ComputeMethod(AutoInvalidationDelay = 30)]
        public async virtual Task<IReadOnlyList<ServiceBusReceivedMessage>> PeekAllDeadLetters(Queue queue)
        {
            try
            {
                Console.WriteLine("PeekAllDeadLetters");
                var receiver = await _clientProvider.GetReceiver(queue, SubQueue.DeadLetter);

                if (receiver == null)
                {
                    return [];
                }

                var messages = await receiver.PeekMessagesAsync(1000, fromSequenceNumber: 0);

                _queueErrors.ClearError(queue);

                return messages;
            }
            catch (Exception e)
            {
                _queueErrors.SetError(queue, e.Message);

                return [];
            }
        }

        public async virtual Task<IReadOnlyList<ServiceBusReceivedMessage>> ForcePeekAllDeadLetters(Queue queue)
        {
            using (Computed.Invalidate())
            {
                _ = PeekAllDeadLetters(queue);
            }
            return await PeekAllDeadLetters(queue);
        }

        public async Task DeleteDeadLetters(Queue queue, string messageId)
        {
            var deadLetterQueue = await _clientProvider.GetReceiver(queue, SubQueue.DeadLetter);
            if (deadLetterQueue == null)
            {
                throw new InvalidOperationException($"Couldn't get a DLQ receiver when I tried to delete {messageId}.");
            }

            var messages = await deadLetterQueue.ReceiveMessagesAsync(1000);

            foreach (var message in messages)
            {
                Console.WriteLine("message: " + message);
                if (message.MessageId == messageId)
                {

                    await deadLetterQueue.CompleteMessageAsync(message);
                }
                else
                {
                    await deadLetterQueue.AbandonMessageAsync(message);
                }
            }

            using (Computed.Invalidate())
            {
                _ = PeekAllDeadLetters(queue);
            }
        }


        public async Task Send(Queue queue, ServiceBusReceivedMessage originalMessage, EditableAttemptProperties? withValues = null)
        {
            var sender = await _clientProvider.GetSender(queue);
            if (sender == null)
            {
                throw new InvalidOperationException("Could not get a service bus sender");
            }

            var messageToSend = new ServiceBusMessage(originalMessage);
            if (withValues != null)
            {
                messageToSend.Body = new BinaryData(withValues.Body);
            }
            await sender.SendMessageAsync(messageToSend);
        }
    }
}
