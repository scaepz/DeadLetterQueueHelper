using Azure.Messaging.ServiceBus;

public record EditableAttemptProperties(string Body)
{
    public EditableAttemptProperties(ServiceBusReceivedMessage message) : this(message.Body.ToString()) { }

}