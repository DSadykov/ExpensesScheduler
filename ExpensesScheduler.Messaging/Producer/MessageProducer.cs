using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

using Confluent.Kafka;

using Microsoft.Extensions.Options;

namespace ExpensesScheduler.Messaging.Producer;

public class MessageProducer<Message> : IMessageProducer<Message>
{
    private readonly string _topic;
    private readonly IProducer<string, Message> _producer;

    public MessageProducer(IOptionsMonitor<MessageProducerConfig> options,
        IOptions<BootstrapServerConfig> bootstarpConfig)
    {
        var currentOptions = options.Get(typeof(Message).ToString());
        var config = new ProducerConfig()
        {
            BootstrapServers = bootstarpConfig.Value.BootstrapServers
        };
        _topic = currentOptions.Topic;
        _producer = new ProducerBuilder<string, Message>(config)
            .SetValueSerializer(new MessageSerializer<Message>())
            .SetKeySerializer(Serializers.Utf8)
            .Build();
    }

    public void Dispose() => _producer?.Dispose();

    public async Task ProduceAsync(Message messsage, CancellationToken ct)
    {
        await _producer.ProduceAsync(_topic, new()
        {
            Key = "Guid.NewGuid().ToString()",
            Value = messsage
        }, ct);
    }
}
