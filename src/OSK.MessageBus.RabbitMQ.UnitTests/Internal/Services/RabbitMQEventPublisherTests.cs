using EasyNetQ;
using Microsoft.Extensions.Logging;
using Moq;
using OSK.MessageBus.Abstractions;
using OSK.MessageBus.Exceptions;
using OSK.MessageBus.Ports;
using OSK.MessageBus.RabbitMQ.Internal.Services;
using OSK.MessageBus.RabbitMQ.UnitTests.Helpers;
using Xunit;

namespace OSK.MessageBus.RabbitMQ.UnitTests.Internal.Services
{
    public class RabbitMQEventPublisherTests
    {
        #region Variables

        private readonly Mock<IBus> _rabbitBus;

        private readonly IMessageEventPublisher _publisher;

        #endregion

        #region Constructors

        public RabbitMQEventPublisherTests() 
        {
            _rabbitBus = new Mock<IBus>();
            var mockLogger = new Mock<ILogger<RabbitMQEventPublisher>>();

            _publisher = new RabbitMQEventPublisher(_rabbitBus.Object, mockLogger.Object);
        }

        #endregion

        #region PublishAsync

        [Fact]
        public async Task PublishAsync_NullMessage_ThrowsArgumentNullException()
        {
            // Arrange/Act/Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _publisher.PublishAsync((TestEvent)null, new MessagePublishOptions()));
        }

        [Fact]
        public async Task PublishAsync_NullOptions_ThrowsArgumentNullException()
        {
            // Arrange/Act/Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _publisher.PublishAsync(new TestEvent(), null));
        }

        [Fact]
        public async Task PublishAsync_InvalidDelayTimeSpan_ThrowsArgumentException()
        {
            // Arrange/Act/Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _publisher.PublishAsync(new TestEvent(), new MessagePublishOptions()
            {
                DelayTimeSpan = TimeSpan.FromSeconds(-1)
            }));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public async Task PublishAsync_ThrowsBusException_CatchesAndThrowsMessageBusException(int publishDelayInSeconds)
        {
            // Arrange
            var delayTimeSpan = TimeSpan.FromSeconds(publishDelayInSeconds);

            var mockScheduler = new Mock<IScheduler>();
            mockScheduler.Setup(m => m.FuturePublishAsync(It.IsAny<TestEvent>(), It.IsAny<TimeSpan>(), It.IsAny<Action<IFuturePublishConfiguration>>(),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException());

            _rabbitBus.SetupGet(m => m.Scheduler)
                .Returns(mockScheduler.Object);

            var mockPubSub = new Mock<IPubSub>();
            mockPubSub.Setup(m => m.PublishAsync(It.IsAny<TestEvent>(), It.IsAny<Action<IPublishConfiguration>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException());

            _rabbitBus.SetupGet(m => m.PubSub)
                .Returns(mockPubSub.Object);

            // Act/Assert
            try
            {
                await _publisher.PublishAsync(new TestEvent(), new MessagePublishOptions()
                {
                    DelayTimeSpan = delayTimeSpan
                });
            }
            catch (MessageBusPublishException ex)
            {
                Assert.IsType<InvalidOperationException>(ex.InnerException);
            }
        }

        [Fact]
        public async Task PublishAsync_ValidDelayTimeSpan_UsesDelayScheduler()
        {
            // Arrange
            var mockScheduler = new Mock<IScheduler>();
            _rabbitBus.SetupGet(m => m.Scheduler)
                .Returns(mockScheduler.Object);

            var mockPubSub = new Mock<IPubSub>();
            _rabbitBus.SetupGet(m => m.PubSub)
                .Returns(mockPubSub.Object);

            // Act
            await _publisher.PublishAsync(new TestEvent(), new MessagePublishOptions()
            {
                DelayTimeSpan = TimeSpan.FromMicroseconds(1)
            });

            // Assert
            mockScheduler.Verify(m => m.FuturePublishAsync(It.IsAny<TestEvent>(), It.IsAny<TimeSpan>(),
                 It.IsAny<Action<IFuturePublishConfiguration>>(), It.IsAny<CancellationToken>()), Times.Once);

            mockPubSub.Verify(m => m.PublishAsync(It.IsAny<TestEvent>(), It.IsAny<Action<IPublishConfiguration>>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task PublishAsync_ValidNoDelayTimeSpan_UsesPubSub()
        {
            // Arrange
            var mockScheduler = new Mock<IScheduler>();
            _rabbitBus.SetupGet(m => m.Scheduler)
                .Returns(mockScheduler.Object);

            var mockPubSub = new Mock<IPubSub>();
            mockPubSub.Setup(m => m.PublishAsync(It.IsAny<TestEvent>(), It.IsAny<Action<IPublishConfiguration>>(), 
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _rabbitBus.SetupGet(m => m.PubSub)
                .Returns(mockPubSub.Object);

            // Act
            await _publisher.PublishAsync(new TestEvent(), new MessagePublishOptions());

            // Assert
            mockScheduler.Verify(m => m.FuturePublishAsync(It.IsAny<TestEvent>(), It.IsAny<TimeSpan>(),
                 It.IsAny<Action<IFuturePublishConfiguration>>(), It.IsAny<CancellationToken>()), 
                 Times.Never);

            mockPubSub.Verify(m => m.PublishAsync(It.IsAny<TestEvent>(), 
                It.IsAny<Action<IPublishConfiguration>>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #endregion
    }
}
