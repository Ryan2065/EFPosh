using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoshLogger
{
    public class PoshLoggerConfiguration
    {
        public LogLevel Level { get; set; } = LogLevel.Information;
        public Dictionary<LogLevel, PoshLogLevel> LevelMappings { get; set; } = new Dictionary<LogLevel, PoshLogLevel>();
    }
}
