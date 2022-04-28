using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoshLogger
{
    internal class PoshLoggerOptionsSetup : ConfigureFromConfigurationOptions<PoshLoggerConfiguration>
    {
        public PoshLoggerOptionsSetup(ILoggerProviderConfiguration<PoshLoggerProvider>
                                      providerConfiguration)
            : base(providerConfiguration.Configuration)
        {
        }
    }
}
