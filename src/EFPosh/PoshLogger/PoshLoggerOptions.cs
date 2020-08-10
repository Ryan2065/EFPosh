using Microsoft.Extensions.Logging;
using System;

namespace PoshLogger
{
    public class PoshLoggerOptions
    {
        public LogLevel LogLevel { get; set; } = Microsoft.Extensions.Logging.LogLevel.Information;
    }
}
