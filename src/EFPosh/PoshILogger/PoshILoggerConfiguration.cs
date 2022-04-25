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
    public class PoshILoggerConfiguration
    {
        public LogLevel Level { get; set; } = LogLevel.Information;
        public Dictionary<LogLevel, PoshLogStream> LogLevelStreamMappings { get; set; } = new Dictionary<LogLevel, PoshLogStream>();
    }

}
