using EasyNetQ;
using System;

namespace OSK.MessageBus.RabbitMQ.Options
{
    public class RabbitMQEventReceiverSettings(string subscriptionId, RabbitMQMessageBusOptions messageBusOptions,
        Action<ISubscriptionConfiguration> subscriptionConfiguration)
    {
        public string SubscriptionId => subscriptionId;

        public RabbitMQMessageBusOptions MessageBusOptions => messageBusOptions;

        public Action<ISubscriptionConfiguration> SubscriptionConfigurator => subscriptionConfiguration;
    }
}
