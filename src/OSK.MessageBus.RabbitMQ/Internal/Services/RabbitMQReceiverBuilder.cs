using EasyNetQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OSK.MessageBus.Events.Abstractions;
using OSK.MessageBus.Models;
using OSK.MessageBus.Ports;
using OSK.MessageBus.RabbitMQ.Options;
using System;

namespace OSK.MessageBus.RabbitMQ.Internal.Services
{
    internal class RabbitMQReceiverBuilder<TMessage>(IServiceProvider serviceProvider) : MessageEventReceiverBuilderBase
        where TMessage: IMessageEvent
    {
        #region Variables

        private Action<ISubscriptionConfiguration>? _subscriptionConfiguration;

        #endregion

        #region MessageEventReceiverBuilderBase

        public void Configure(Action<ISubscriptionConfiguration>? subscriptionConfiguration)
        {
            _subscriptionConfiguration = subscriptionConfiguration;
        }

        protected override IMessageEventReceiver BuildReceiver(string subscriptionId, MessageEventDelegate eventDelegate)
        {
            if (string.IsNullOrWhiteSpace(subscriptionId))
            {
                throw new ArgumentNullException("subscriptionId can not be empty", nameof(subscriptionId));
            }
            if (_subscriptionConfiguration == null)
            {
                _subscriptionConfiguration = _ => { };
            }

            return ActivatorUtilities.CreateInstance<RabbitMQEventReceiver<TMessage>>(serviceProvider,
                subscriptionId, eventDelegate, 
                new RabbitMQEventReceiverSettings(subscriptionId,
                 serviceProvider.GetRequiredService<IOptions<RabbitMQMessageBusOptions>>().Value,
                 _subscriptionConfiguration));
        }

        #endregion
    }
}
