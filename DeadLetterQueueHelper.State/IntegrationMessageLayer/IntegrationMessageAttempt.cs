using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadLetterQueueHelper.State.IntegrationMessageLayer
{
    public record IntegrationMessageAttempt(long SequenceNumber, string body)
    {
    }
}
