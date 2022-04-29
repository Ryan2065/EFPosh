using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoshLogger
{
    /// <summary>
    /// Extension class to add loggers to service collections
    /// </summary>
    public static class PoshILoggerExtensions
    {
        public static ILoggingBuilder AddPoshLogger(
            this ILoggingBuilder builder)
        {
            builder.AddConfiguration();

            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<ILoggerProvider, PoshLoggerProvider>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton
           <IConfigureOptions<PoshLoggerConfiguration>, PoshLoggerOptionsSetup>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton
           <IOptionsChangeTokenSource<PoshLoggerConfiguration>,
           LoggerProviderOptionsChangeTokenSource<PoshLoggerConfiguration, PoshILogger>>());

            return builder;
        }

        public static ILoggingBuilder AddPoshLogger(
            this ILoggingBuilder builder,
            Action<PoshLoggerConfiguration> configure)
        {
            builder.AddPoshLogger();
            builder.Services.Configure(configure);

            return builder;
        }
    }
}
