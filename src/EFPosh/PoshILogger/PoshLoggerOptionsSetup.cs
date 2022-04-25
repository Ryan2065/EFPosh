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
    public class PoshLoggerOptionsSetup : ConfigureFromConfigurationOptions<PoshILoggerConfiguration>
    {
        public PoshLoggerOptionsSetup(ILoggerProviderConfiguration<PoshILoggerProvider>
                                      providerConfiguration)
            : base(providerConfiguration.Configuration)
        {
        }
    }
}
