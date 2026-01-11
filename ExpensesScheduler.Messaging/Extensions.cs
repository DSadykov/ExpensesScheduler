using System;
using System.Collections.Generic;
using System.Text;

using ExpensesScheduler.Messaging.Consumer;
using ExpensesScheduler.Messaging.Producer;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExpensesScheduler.Messaging;

public static class Extensions
{
    public static IServiceCollection AddProducer<Message>
        (this IServiceCollection services, IConfigurationManager configurationManager, string kafkaConfig)
    {
        services.Configure<MessageProducerConfig>(typeof(Message).ToString(), configurationManager.GetSection(kafkaConfig));
        services.Configure<BootstrapServerConfig>(configurationManager.GetSection("Kafka"));
        services.AddSingleton<IMessageProducer<Message>, MessageProducer<Message>>();
        return services;
    }

    public static IServiceCollection AddConsumer<Message, Handler>
        (this IServiceCollection services, IConfigurationManager configurationManager, string kafkaConfig)
        where Message : class
        where Handler : class, IMessageHandler<Message>
    {
        services.Configure<MessageConsumerConfig>
            (typeof(Message).ToString(), configurationManager.GetSection(kafkaConfig));
        services.Configure<BootstrapServerConfig>(configurationManager.GetSection("Kafka"));

        services.AddHostedService<MessageConsumer<Message>>();

        services.AddSingleton<IMessageHandler<Message>, Handler>();

        return services;
    }
}
