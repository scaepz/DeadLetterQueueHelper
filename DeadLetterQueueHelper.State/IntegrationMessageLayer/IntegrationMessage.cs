using Azure.Messaging.ServiceBus;
using Stl.Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadLetterQueueHelper.State.IntegrationMessageLayer
{
    public record IntegrationMessage(List<ServiceBusReceivedMessage> Attempts)
    {
        public string Id => Attempts.First().MessageId;
        public string Subject => Attempts.First().Subject;
        public string FirstEnqueuedTime => Attempts.First().EnqueuedTime.ToLocalTime().ToString();
        public string DeadLetterReason => Attempts.First().DeadLetterReason;
        public string Body => Attempts.First().Body.ToString();
    }
}
