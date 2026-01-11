using System;

using Confluent.Kafka;

using ExpensesScheduler.Messaging.Producer;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ExpensesScheduler.Messaging.Consumer;

public class MessageConsumer<Message> : BackgroundService, IDisposable
{
    private readonly ConsumerConfig _config;
    private readonly string _topic;
    private readonly IConsumer<string, Message> _consumer;
    private readonly IMessageHandler<Message> _messageHandler;

    public MessageConsumer(IOptionsMonitor<MessageConsumerConfig> options,
        IOptions<BootstrapServerConfig> bootstarpConfig,
        IMessageHandler<Message> messageHandler)
    {
        var currentConfig = options.Get(typeof(Message).ToString());
        _config = new ConsumerConfig()
        {
            BootstrapServers = bootstarpConfig.Value.BootstrapServers,
            GroupId = currentConfig.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        _topic = currentConfig.Topic;
        var deserializer = new MessageDeserializer<Message>();
        _consumer = new ConsumerBuilder<string, Message>(_config)
            .SetValueDeserializer(deserializer)
            .Build();
        _messageHandler = messageHandler;
    }

    public void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
    }

    public async Task HandleMessageAsync(Func<Message, Task> messageHandler, CancellationToken ct)
    {
        var message = _consumer.Consume(ct);
        await messageHandler(message.Message.Value);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(()=> ConsumeAsync(stoppingToken), stoppingToken);
    }

    private async Task ConsumeAsync(CancellationToken stoppingToken)
    {
        WaitForTopicCreation(stoppingToken);

        _consumer.Subscribe(_topic);

        try
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                var result = _consumer.Consume(stoppingToken);
                await _messageHandler.HandleAsync(result.Message.Value, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            var type = typeof(Message);
        }
    }

    private void WaitForTopicCreation(CancellationToken stoppingToken)
    {
        using var adminClient = new AdminClientBuilder(_config).Build();
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Fetch metadata for all topics
                var metadata = adminClient.GetMetadata(_topic, TimeSpan.FromSeconds(2));

                // Check if the specific topic exists in the metadata
                if (metadata.Topics.Exists(t => t.Topic == _topic))
                {
                    return;
                }
            }
            catch (KafkaException e)
            {
                Console.WriteLine($"Error fetching metadata: {e.Message}. Retrying...");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Timeout waiting for topic creation.");
                throw new TimeoutException($"Timed out waiting for topic '{_topic}' to be created.");
            }

            Thread.Sleep(TimeSpan.FromSeconds(2)); // Wait before the next attempt
        }
    }
}
