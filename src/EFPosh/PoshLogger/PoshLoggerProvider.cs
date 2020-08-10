using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace PoshLogger
{
    [Microsoft.Extensions.Logging.ProviderAlias("PowerShell")]
    public class PoshLoggerProvider : LoggerProvider
    {
        public override bool IsEnabled(LogLevel logLevel)
        {
            bool Result = logLevel != LogLevel.None
                && this.Settings.LogLevel != LogLevel.None
                && Convert.ToInt32(logLevel) >= Convert.ToInt32(this.Settings.LogLevel);

            return Result;
        }

        public override void WriteLog(LogEntry Info)
        {
            if(Info.Level == LogLevel.Debug || Info.Level == LogLevel.Trace)
            {
                // ToDo make this work and add info to streams
                //WriteDebug("test");
            }
        }
        internal PoshLoggerOptions Settings { get; private set; }
    }
}
