using EasyNetQ;
using System;
using OSK.MessageBus.Events.Abstractions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OSK.MessageBus.Models;
using OSK.MessageBus.RabbitMQ.Options;
using System.Linq;
using EasyNetQ.Topology;
using OSK.MessageBus.Exceptions;

namespace OSK.MessageBus.RabbitMQ.Internal.Services
{
    internal class RabbitMQEventReceiver<TMessage>(MessageEventDelegate eventDelegate, IBus messageBus, 
        ILogger<RabbitMQEventReceiver<TMessage>> logger, IServiceProvider serviceProvider,
        RabbitMQEventReceiverSettings settings) : MessageEventReceiverBase(eventDelegate, serviceProvider)
        where TMessage: IMessageEvent
    {
        #region Variables

        private IDisposable? _subscription;

        #endregion

        #region IMessageEventReceiver

        public override void Dispose()
        {
            _subscription?.Dispose();
        }

        public override void Start()
        {
            try
            {
                var subscriptionConfiguration = new SubscriberConfiguration(settings.MessageBusOptions.PrefetchCount);
                settings.SubscriptionConfigurator(subscriptionConfiguration);

                var queueName = messageBus.Advanced.Conventions.QueueNamingConvention(typeof(TMessage), settings.SubscriptionId);
                var exchangeName = messageBus.Advanced.Conventions.ExchangeNamingConvention(typeof(TMessage));

                var queue = messageBus.Advanced.QueueDeclare(queueName, configure =>
                {
                    configure.AsAutoDelete(subscriptionConfiguration.AutoDelete);
                    configure.AsDurable(subscriptionConfiguration.Durable);
                    
                    if (subscriptionConfiguration.Expires.HasValue)
                    {
                        configure.WithExpires(TimeSpan.FromMilliseconds(subscriptionConfiguration.Expires.Value));
                    }
                    if (subscriptionConfiguration.MaxPriority.HasValue)
                    {
                        configure.WithMaxPriority(subscriptionConfiguration.MaxPriority.Value);
                    }
                    if (subscriptionConfiguration.MaxLengthBytes.HasValue)
                    {
                        configure.WithMaxLengthBytes(subscriptionConfiguration.MaxLengthBytes.Value);
                    }
                    if (subscriptionConfiguration.MaxLength.HasValue)
                    {
                        configure.WithMaxLength(subscriptionConfiguration.MaxLength.Value);
                    }
                    if (!string.IsNullOrWhiteSpace(subscriptionConfiguration.QueueMode))
                    {
                        configure.WithQueueMode(subscriptionConfiguration.QueueMode);
                    }
                });

                var exchange = messageBus.Advanced.ExchangeDeclare(exchangeName, ExchangeType.Topic);

                foreach (var topic in subscriptionConfiguration.Topics.DefaultIfEmpty("#"))
                {
                    messageBus.Advanced.Bind(exchange, queue, topic);
                }

                _subscription = messageBus.Advanced.Consume<TMessage>(
                    queue,
                    HandleEvent,
                    x =>
                    {
                        x.WithPriority(subscriptionConfiguration.Priority)
                         .WithPrefetchCount(subscriptionConfiguration.PrefetchCount)
                         .WithExclusive(subscriptionConfiguration.IsExclusive);
                    });
            }
            catch (Exception ex)
            {
                var message = $"Error starting event subscriber for '{typeof(TMessage)}'";
                logger.LogError(ex, message);

                throw new MessageBusReceiverException(message, ex);
            }
        }

        #endregion

        #region Helpers

        internal Task HandleEvent<T>(IMessage<T> message, MessageReceivedInfo messageReceivedInfo)
            where T : IMessageEvent
            => HandleEventAsync(message.Body, message);

        #endregion
    }
}
