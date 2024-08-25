using Stl.Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadLetterQueueHelper.State.IntegrationMessageLayer
{
    public record IntegrationMessage(string Id, List<IntegrationMessageAttempt> Attempts)
    {

    }
}
