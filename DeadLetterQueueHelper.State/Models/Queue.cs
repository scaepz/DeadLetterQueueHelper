namespace DeadLetterQueueHelper.State.Models
{
    public record Queue(string Namespace, string QueueName, bool IsSelected)
    {
        public Queue() : this("", "", false) { }
    }
}
