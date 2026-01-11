using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

using Confluent.Kafka;

namespace ExpensesScheduler.Messaging.Consumer;

public class MessageDeserializer<Message> : IDeserializer<Message>
{

    public Message Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        if (isNull)

        {
            return default;
        }

        return JsonSerializer.Deserialize<Message>(data);
    }
}
