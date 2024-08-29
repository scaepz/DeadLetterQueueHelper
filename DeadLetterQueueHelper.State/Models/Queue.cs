namespace DeadLetterQueueHelper.State.Models
{
    public record Queue(string Namespace, string QueueName)
    {
        public Queue() : this("", "") { }
    }
}
