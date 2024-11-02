using OSK.MessageBus.Abstractions;
using OSK.MessageBus.Events.Abstractions;
using OSK.MessageBus.Ports;
using OSK.MessageBus.RabbitMQ.Internal.Services;
using System;
using System.Threading.Tasks;

namespace OSK.MessageBus.RabbitMQ
{
    public static class MessageEventReceiverManagerExtensions
    {
        public static IMessageEventReceiverManager AddRabbitReceiver<TMessage>(this IMessageEventReceiverManager manager,
            string subscriberId, string topicFilter, Func<IMessageEventContext<TMessage>, Task> handler)
            where TMessage: IMessageEvent
            => manager.AddRabbitReceiver(subscriberId, topicFilter, null, handler);

        public static IMessageEventReceiverManager AddRabbitReceiver<TMessage>(this IMessageEventReceiverManager manager,
            string subscriberId, string topicFilter, Action<IMessageEventReceiverBuilder>? builderConfigurator, Func<IMessageEventContext<TMessage>, Task> handler)
            where TMessage : IMessageEvent
        {
            return manager.AddEventReceiver(subscriberId, (serviceProvider, configurators) =>
            {
                var rabbitReceiverBuilder = new RabbitMQReceiverBuilder<TMessage>(serviceProvider);
                rabbitReceiverBuilder.Configure(configuration =>
                {
                    configuration.WithTopic(topicFilter);
                });

                foreach (var configurator in configurators)
                {
                    configurator(rabbitReceiverBuilder);
                }

                builderConfigurator?.Invoke(rabbitReceiverBuilder);
                rabbitReceiverBuilder.UseHandler(handler);

                return rabbitReceiverBuilder;
            });
        }

        public static IMessageEventReceiverManager AddRabbitReceiver<TMessage, THandler>(this IMessageEventReceiverManager manager,
            string subscriberId, string topicFilter)
            where TMessage : IMessageEvent
            where THandler : IMessageEventHandler<TMessage>
            => manager.AddRabbitReceiver<TMessage, THandler>(subscriberId, topicFilter, null);

        public static IMessageEventReceiverManager AddRabbitReceiver<TMessage, THandler>(this IMessageEventReceiverManager manager,
            string subscriberId, string topicFilter, Action<IMessageEventReceiverBuilder>? builderConfigurator)
            where TMessage : IMessageEvent
            where THandler : IMessageEventHandler<TMessage>
        {
            return manager.AddEventReceiver(subscriberId, (serviceProvider, configurators) =>
            {
                var rabbitReceiverBuilder = new RabbitMQReceiverBuilder<TMessage>(serviceProvider);
                rabbitReceiverBuilder.Configure(configuration =>
                {
                    configuration.WithTopic(topicFilter);
                });

                foreach (var configurator in configurators)
                {
                    configurator(rabbitReceiverBuilder);
                }

                builderConfigurator?.Invoke(rabbitReceiverBuilder);
                rabbitReceiverBuilder.UseHandler<TMessage, THandler>();

                return rabbitReceiverBuilder;
            });
        }
    }
}
