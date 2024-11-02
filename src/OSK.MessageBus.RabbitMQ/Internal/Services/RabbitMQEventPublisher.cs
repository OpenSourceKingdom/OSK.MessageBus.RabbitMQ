using EasyNetQ;
using OSK.MessageBus.Abstractions;
using OSK.MessageBus.Ports;
using System;
using System.Threading;
using System.Threading.Tasks;
using OSK.MessageBus.Exceptions;
using OSK.MessageBus.Events.Abstractions;
using Microsoft.Extensions.Logging;

namespace OSK.MessageBus.RabbitMQ.Internal.Services
{
    internal class RabbitMQEventPublisher(IBus messageBus, ILogger<RabbitMQEventPublisher> logger) : IMessageEventPublisher
    {
        #region IMessageEventPublisher

        public async Task PublishAsync<TMessage>(TMessage message, MessagePublishOptions options, 
            CancellationToken cancellationToken = default)
            where TMessage : IMessageEvent
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.DelayTimeSpan < TimeSpan.Zero)
            {
                throw new ArgumentException("Publish delay must be greater than or equal to zero.", nameof(options.DelayTimeSpan));
            }

            try
            {
                if (options.DelayTimeSpan > TimeSpan.Zero)
                {
                    await messageBus.Scheduler.FuturePublishAsync(message, options.DelayTimeSpan, configuration => {
                        configuration.WithTopic(message.TopicId);
                    }, cancellationToken);
                }
                else
                {
                    await messageBus.PubSub.PublishAsync(message, configuration =>
                    {
                        configuration.WithTopic(message.TopicId);
                    }, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error publishing event '{message.GetType()}' to message bus";
                logger.LogError(ex, errorMessage);

                throw new MessageBusPublishException(errorMessage, ex);
            }
        }

        #endregion
    }
}
