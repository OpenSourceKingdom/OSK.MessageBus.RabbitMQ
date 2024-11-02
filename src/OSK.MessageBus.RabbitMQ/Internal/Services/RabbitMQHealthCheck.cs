using EasyNetQ;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace OSK.MessageBus.RabbitMQ.Internal.Services
{
    internal class RabbitMQHealthCheck(IBus bus) : IHealthCheck
    {
        #region Variables

        private static readonly TimeSpan SuccessCacheTimeSpan = TimeSpan.FromMinutes(2);
        private static readonly SemaphoreSlim _healthCheckSemaphore = new SemaphoreSlim(1);
        private static DateTime _lastHeartbeat = DateTime.MinValue;
        private static readonly string ApplicationName;

        #endregion

        static RabbitMQHealthCheck()
        {
            var assemblyName = Assembly.GetEntryAssembly()?.GetName();
            ApplicationName = assemblyName?.Name ?? "Unknown Assembly";
        }

        #region IHealthCheck

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, 
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (DateTime.UtcNow - _lastHeartbeat < SuccessCacheTimeSpan)
            {
                return HealthCheckResult.Healthy();
            }

            await _healthCheckSemaphore.WaitAsync(cancellationToken);
            try
            {
                if (DateTime.UtcNow - _lastHeartbeat < SuccessCacheTimeSpan)
                {
                    return HealthCheckResult.Healthy();
                }

                await bus.PubSub.PublishAsync(new
                {
                    ApplicationName
                });

                _lastHeartbeat = DateTime.UtcNow;
                return HealthCheckResult.Healthy();
            }
            finally
            {
                _healthCheckSemaphore.Release();
            }
        }

        #endregion
    }
}
