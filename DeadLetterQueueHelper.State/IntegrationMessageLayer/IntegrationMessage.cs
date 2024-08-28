using Azure.Messaging.ServiceBus;

namespace DeadLetterQueueHelper.State.IntegrationMessageLayer
{
    public record IntegrationMessage(List<ServiceBusReceivedMessage> Attempts)
    {
        public string Id => Attempts.First().MessageId;
        public string Subject => Attempts.First().Subject;
        public string FirstEnqueuedTime => Attempts.First().EnqueuedTime.ToLocalTime().ToString();
        public string DeadLetterReason => Attempts.First().DeadLetterReason;
    }
}
