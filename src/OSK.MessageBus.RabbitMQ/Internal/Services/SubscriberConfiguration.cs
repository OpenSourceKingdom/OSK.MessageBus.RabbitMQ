using EasyNetQ;
using System;
using System.Collections.Generic;

namespace OSK.MessageBus.RabbitMQ.Internal.Services
{
    internal class SubscriberConfiguration: ISubscriptionConfiguration
    {
        #region Variables

        public IList<string> Topics { get; }
        public bool AutoDelete { get; private set; }
        public int Priority { get; private set; }
        public ushort PrefetchCount { get; private set; }
        public int? Expires { get; private set; }
        public bool IsExclusive { get; private set; }
        public byte? MaxPriority { get; private set; }
        public bool Durable { get; private set; }
        public string QueueName { get; private set; }
        public int? MaxLength { get; private set; }
        public int? MaxLengthBytes { get; private set; }
        public string QueueMode { get; private set; }

        #endregion

        #region Constructors

        public SubscriberConfiguration(ushort defaultPrefetchCount)
        {
            Topics = new List<string>();
            AutoDelete = false;
            Priority = 0;
            PrefetchCount = defaultPrefetchCount;
            IsExclusive = false;
            Durable = true;
        }

        #endregion

        #region ISubscriptionConfiguration

        public ISubscriptionConfiguration WithTopic(string topic)
        {
            Topics.Add(topic);
            return this;
        }

        public ISubscriptionConfiguration WithAutoDelete(bool autoDelete = true)
        {
            AutoDelete = autoDelete;
            return this;
        }

        public ISubscriptionConfiguration WithDurable(bool durable = true)
        {
            Durable = durable;
            return this;
        }

        public ISubscriptionConfiguration WithPriority(int priority)
        {
            Priority = priority;
            return this;
        }

        public ISubscriptionConfiguration WithPrefetchCount(ushort prefetchCount)
        {
            PrefetchCount = prefetchCount;
            return this;
        }

        public ISubscriptionConfiguration WithExpires(int expires)
        {
            Expires = expires;
            return this;
        }

        public ISubscriptionConfiguration AsExclusive(bool isExclusive = true)
        {
            IsExclusive = isExclusive;
            return this;
        }

        public ISubscriptionConfiguration WithMaxPriority(byte priority)
        {
            MaxPriority = priority;
            return this;
        }

        public ISubscriptionConfiguration WithQueueName(string queueName)
        {
            QueueName = queueName;
            return this;
        }

        public ISubscriptionConfiguration WithMaxLength(int maxLength)
        {
            MaxLength = maxLength;
            return this;
        }

        public ISubscriptionConfiguration WithMaxLengthBytes(int maxLengthBytes)
        {
            MaxLengthBytes = maxLengthBytes;
            return this;
        }

        public ISubscriptionConfiguration WithQueueMode(string queueMode)
        {
            QueueMode = queueMode;
            return this;
        }

        public ISubscriptionConfiguration WithQueueType(string queueType = "classic")
        {
            throw new NotImplementedException();
        }

        public ISubscriptionConfiguration WithExchangeType(string exchangeType)
        {
            throw new NotImplementedException();
        }

        public ISubscriptionConfiguration WithAlternateExchange(string alternateExchange)
        {
            throw new NotImplementedException();
        }

        public ISubscriptionConfiguration WithSingleActiveConsumer(bool singleActiveConsumer = true)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}