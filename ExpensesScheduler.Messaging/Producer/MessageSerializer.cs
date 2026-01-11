using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

using Confluent.Kafka;

namespace ExpensesScheduler.Messaging.Producer;

public class MessageSerializer<Message> : ISerializer<Message>
{
    public byte[] Serialize(Message data, SerializationContext context)
    {
        var json = JsonSerializer.Serialize(data);
        return Encoding.UTF8.GetBytes(json);
    }
}
