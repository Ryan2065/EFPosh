using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoshLogger
{
    /// <summary>
    /// ILogger that will log to the PowerShell session without a Cmdlet
    /// </summary>
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
        /// <summary>
        /// Run enable static method to build the PowerShell command
        /// </summary>
        private void InitialSetup()
        {
            PoshLogger.PoshLoggerQueue.Enable();
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return default;
        }
        /// <summary>
        /// Decide if we are going to log - just based on configured log level
        /// </summary>
        /// <param name="logLevel">log level of the message</param>
        /// <returns>true if we should log the message</returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _logConfig.Level;
        }
        /// <summary>
        /// Figures out what the PowerShell log level should be compared to .net log level
        /// </summary>
        /// <param name="logLevel">Message log level</param>
        /// <returns>PowerShell log level</returns>
        private PoshLogLevel PoshLogLevel(LogLevel logLevel)
        {
            if (_logConfig.LevelMappings.ContainsKey(logLevel))
            {
                return _logConfig.LevelMappings[logLevel];
            }
            switch (logLevel)
            {
                case LogLevel.Trace:
                    return PoshLogger.PoshLogLevel.Progress;
                case LogLevel.Debug:
                    return PoshLogger.PoshLogLevel.Debug;
                case LogLevel.Information:
                    return PoshLogger.PoshLogLevel.Verbose;
                case LogLevel.Warning:
                    return PoshLogger.PoshLogLevel.Warning;
                case LogLevel.Error:
                case LogLevel.Critical:
                    return PoshLogger.PoshLogLevel.Error;
                case LogLevel.None:
                default:
                    return PoshLogger.PoshLogLevel.Verbose;
            }
        }
        /// <summary>
        /// Checks if we are enabled for logging, if so, will queue the message
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="logLevel"></param>
        /// <param name="eventId"></param>
        /// <param name="state"></param>
        /// <param name="exception"></param>
        /// <param name="formatter"></param>
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
