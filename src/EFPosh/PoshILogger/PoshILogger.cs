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
    public class PoshLogger : ILogger
    {
        private readonly PoshILoggerConfiguration _logConfig;
        private readonly string _name;
        private System.Management.Automation.PowerShell _powerShell;
        private bool _enabled;
        public PoshLogger(string name, Func<PoshILoggerConfiguration> config)
        {
            _name = name;
            _logConfig = config();
            InitialSetup();
        }
        public PoshLogger()
        {
            _logConfig = new PoshILoggerConfiguration();
            InitialSetup();
        }

        public PoshLogger(LogLevel level)
        {
            _logConfig = new PoshILoggerConfiguration();
            _logConfig.Level = level;
            InitialSetup();
        }

        private void InitialSetup()
        {
            try
            {
                _powerShell = System.Management.Automation.PowerShell.Create(System.Management.Automation.RunspaceMode.CurrentRunspace);
                _enabled = true;
            }
            catch
            {
                // errors may happen if not run inside of Posh - just disable logging if that happens
                _enabled = false;
            }
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return default;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            if (_enabled == false) { return false; }
            return logLevel >= _logConfig.Level;
        }

        private System.Management.Automation.PowerShell GetPowerShellObject()
        {
            try
            {
                if (_powerShell == null)
                {
                    _powerShell = System.Management.Automation.PowerShell.Create(System.Management.Automation.RunspaceMode.CurrentRunspace);
                }
                _powerShell.Commands.Clear();
                return _powerShell;
            }
            catch
            {
                _enabled = false;
            }
            return null;
        }

        private void WritePoshLog(string message, PoshLogStream logStream, Exception exceptionRecord = null)
        {
            if (_enabled == false) { return; }
            var poshCommand = GetPowerShellObject();
            if (poshCommand == null) { return; }

            switch (logStream)
            {
                case PoshLogStream.Output:
                    poshCommand.Commands.AddScript("Param($message) Write-Output $message").AddParameter("message", message);
                    poshCommand.Invoke();
                    return;
                case PoshLogStream.Error:
                    if (exceptionRecord != null)
                    {
                        poshCommand.Commands.AddScript("Param($exc, $message) Write-Error -Exception $exc -Message $message -ErrorAction Continue").AddParameter("exc", exceptionRecord).AddParameter("message", message);
                    }
                    else
                    {
                        poshCommand.Commands.AddScript("Param($message) Write-Error -Message $message -ErrorAction Continue").AddParameter("message", message);
                    }
                    poshCommand.Invoke();
                    return;
                case PoshLogStream.Warning:
                    poshCommand.Commands.AddScript("Param($message) Write-Warning $message").AddParameter("message", message);
                    poshCommand.Invoke();
                    return;
                case PoshLogStream.Verbose:
                    poshCommand.Commands.AddScript("Param($message) Write-Verbose $message").AddParameter("message", message);
                    poshCommand.Invoke();
                    return;
                case PoshLogStream.Debug:
                    poshCommand.Commands.AddScript("Param($message) Write-Debug $message").AddParameter("message", message);
                    poshCommand.Invoke();
                    return;
                case PoshLogStream.Information:
                    poshCommand.Commands.AddScript("Param($message) Write-Information $message").AddParameter("message", message);
                    poshCommand.Invoke();
                    return;
                case PoshLogStream.Progress:
                    poshCommand.Commands.AddScript("Param($message) Write-Progress $message").AddParameter("message", message);
                    poshCommand.Invoke();
                    return;
            }
        }

        private PoshLogStream PoshLogLevel(LogLevel logLevel)
        {
            if (_logConfig.LogLevelStreamMappings.ContainsKey(logLevel))
            {
                return _logConfig.LogLevelStreamMappings[logLevel];
            }
            switch (logLevel)
            {
                case LogLevel.Debug:
                    return PoshLogStream.Debug;
                case LogLevel.Information:
                    return PoshLogStream.Information;
                case LogLevel.Warning:
                    return PoshLogStream.Warning;
                case LogLevel.Error:
                case LogLevel.Critical:
                    return PoshLogStream.Error;
                case LogLevel.None:
                case LogLevel.Trace:
                default:
                    return PoshLogStream.Verbose;
            }
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }
            var poshLogLevel = PoshLogLevel(logLevel);
            string message = $"{formatter(state, exception)}";
            WritePoshLog(message, poshLogLevel, exception);
        }

    }
}
