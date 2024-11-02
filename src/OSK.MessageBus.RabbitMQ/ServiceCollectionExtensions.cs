using EasyNetQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using OSK.MessageBus.Ports;
using OSK.MessageBus.RabbitMQ.Internal.Services;
using OSK.MessageBus.RabbitMQ.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OSK.MessageBus.RabbitMQ
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMQBus(this IServiceCollection services,
            Action<RabbitMQMessageBusOptions> optionsConfigurator)
        {
            services.Configure(optionsConfigurator);
            services.TryAddScoped<IMessageEventPublisher, RabbitMQEventPublisher>();
            services.TryAddSingleton(serviceProvider =>
            {
                var currentConfiguration = serviceProvider.GetRequiredService<IOptions<RabbitMQMessageBusOptions>>();

                return RabbitHutch.CreateBus(currentConfiguration.Value, serviceRegister =>
                {
                    serviceRegister.EnableDelayedExchangeScheduler();
                    serviceRegister.EnableMessageVersioning();
                });
            });

            return services;
        }

        public static IHealthChecksBuilder AddRabbitMQHealthCheck(this IHealthChecksBuilder builder, string name, HealthStatus? failureStatus = null,
            IEnumerable<string>? tags = null)
        {
            return builder.AddCheck<RabbitMQHealthCheck>(name, failureStatus, tags ?? Enumerable.Empty<string>(), TimeSpan.FromSeconds(10));
        }
    }
}
