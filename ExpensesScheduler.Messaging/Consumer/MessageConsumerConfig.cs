namespace ExpensesScheduler.Messaging.Consumer;

public class MessageConsumerConfig
{
    public required string Topic { get; set; }
    public required string GroupId { get; set; }
}