using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoshLogger
{
    public class PoshILogger : ILogger
    {
        private readonly PoshLoggerConfiguration _logConfig;
        public PoshILogger(Func<PoshLoggerConfiguration> config)
        {
            _logConfig = config();
            InitialSetup();
        }
        public PoshILogger()
        {
            _logConfig = new PoshLoggerConfiguration();
            InitialSetup();
        }

        public PoshILogger(LogLevel level)
        {
            _logConfig = new PoshLoggerConfiguration
            {
                Level = level
            };
            InitialSetup();
        }

        private void InitialSetup()
        {
            PoshLogger.PoshLoggerQueue.Enable();
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return default;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _logConfig.Level;
        }

        private PoshLogLevel PoshLogLevel(LogLevel logLevel)
        {
            if (_logConfig.LevelMappings.ContainsKey(logLevel))
            {
                return _logConfig.LevelMappings[logLevel];
            }
            switch (logLevel)
            {
                case LogLevel.Trace:
                    return PoshLogger.PoshLogLevel.Debug;
                case LogLevel.Information:
                    return PoshLogger.PoshLogLevel.Information;
                case LogLevel.Warning:
                    return PoshLogger.PoshLogLevel.Warning;
                case LogLevel.Error:
                case LogLevel.Critical:
                    return PoshLogger.PoshLogLevel.Error;
                case LogLevel.None:
                case LogLevel.Debug:
                default:
                    return PoshLogger.PoshLogLevel.Verbose;
            }
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }
            var newPoshItem = new PoshLoggerEntry
            {
                Level = PoshLogLevel(logLevel),
                Message = $"{formatter(state, exception)}",
                Exception = exception
            };
            PoshLogger.PoshLoggerQueue.Enqueue(newPoshItem);
        }
    }
}
