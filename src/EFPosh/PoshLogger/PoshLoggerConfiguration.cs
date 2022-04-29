using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoshLogger
{
    public class PoshLoggerConfiguration
    {
        /// <summary>
        /// What log level are we configured for
        /// </summary>
        public LogLevel Level { get; set; } = LogLevel.Information;
        /// <summary>
        /// Override the default log level mappings to use custom ones
        /// </summary>
        public Dictionary<LogLevel, PoshLogLevel> LevelMappings { get; set; } = new Dictionary<LogLevel, PoshLogLevel>();
    }
}
