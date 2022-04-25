using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PoshILogger
{
    public static class PoshILoggerExtensions
    {
        public static ILoggingBuilder AddPoshLogger(this ILoggingBuilder builder, PoshILoggerConfiguration config)
        {
            builder.AddConfiguration();
            return builder;
        }
        public static ILoggingBuilder AddPoshLogger(
            this ILoggingBuilder builder)
        {
            builder.AddConfiguration();

            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<ILoggerProvider, PoshILoggerProvider>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton
           <IConfigureOptions<PoshILoggerConfiguration>, PoshLoggerOptionsSetup>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton
           <IOptionsChangeTokenSource<PoshILoggerConfiguration>,
           LoggerProviderOptionsChangeTokenSource<PoshILoggerConfiguration, PoshLogger>>());

            return builder;
        }

        public static ILoggingBuilder AddPoshLogger(
            this ILoggingBuilder builder,
            Action<PoshILoggerConfiguration> configure)
        {
            builder.AddPoshLogger();
            builder.Services.Configure(configure);

            return builder;
        }
    }
}
