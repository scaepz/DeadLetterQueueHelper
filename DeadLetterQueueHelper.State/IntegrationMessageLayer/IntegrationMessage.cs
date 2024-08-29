using Azure.Messaging.ServiceBus;
using DeadLetterQueueHelper.State.Models;

namespace DeadLetterQueueHelper.State.IntegrationMessageLayer
{
    public record IntegrationMessage(Queue Queue, List<ServiceBusReceivedMessage> Attempts)
    {
        public string Id => Attempts.First().MessageId;
        public string Subject => Attempts.First().Subject;
        public string FirstEnqueuedTime => Attempts.First().EnqueuedTime.ToLocalTime().ToString("g");
        public string DeadLetterReason => Attempts.First().DeadLetterReason;
    }
}
