using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace EFPosh
{
    public class PoshILoggerConfiguration
    {
        public HashSet<LogLevel> Levels = new HashSet<LogLevel>() { LogLevel.Information };
        public Dictionary<LogLevel, PoshLogStream> LogLevelStreamMappings { get; set; } = new Dictionary<LogLevel, PoshLogStream>();
    }
    public class PoshILoggerProvider : ILoggerProvider
    {
        private PoshILoggerConfiguration _currentConfig;
        private readonly IDisposable _onChangeToken;
        private readonly ConcurrentDictionary<string, PoshILogger> _loggers = new ConcurrentDictionary<string, PoshILogger>();
        public PoshILoggerProvider(IOptionsMonitor<PoshILoggerConfiguration> config)
        {
            _currentConfig = config.CurrentValue;
            _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
        }
        public ILogger CreateLogger(string categoryName) 
        {
            return _loggers.GetOrAdd(categoryName, p => new PoshILogger(categoryName, GetCurrentConfig));
        }
        private PoshILoggerConfiguration GetCurrentConfig() => _currentConfig;

        public void Dispose()
        {
            _loggers.Clear();
            _onChangeToken.Dispose();
        }
    }
    public class PoshILogger : ILogger
    {
        private readonly Func<PoshILoggerConfiguration> _logConfig;
        private readonly string _name;
        private System.Management.Automation.PowerShell _powerShell;
        private bool _enabled;
        public PoshILogger(string name, Func<PoshILoggerConfiguration> config)
        {
            _name = name;
            _logConfig = config;
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
            if(_enabled == false) { return false; }
            return _logConfig().Levels.Contains(logLevel);
        }

        private System.Management.Automation.PowerShell GetPowerShellObject()
        {
            try
            {
                if(_powerShell == null)
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
            if(_enabled == false) { return; }
            var poshCommand = GetPowerShellObject();
            if(poshCommand == null) { return; }

            switch (logStream)
            {
                case PoshLogStream.Output:
                    poshCommand.Commands.AddScript("Param($message) Write-Output $message").AddParameter("message", message);
                    poshCommand.Invoke();
                    return;
                case PoshLogStream.Error:
                    if(exceptionRecord != null)
                    {
                        poshCommand.Commands.AddScript("Param($exc, $message) Write-Error -Exception $exc -Message $message").AddParameter("exc", exceptionRecord).AddParameter("message", message);
                    }
                    else
                    {
                        poshCommand.Commands.AddScript("Param($message) Write-Error -Message $message").AddParameter("message", message);
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
            if (_logConfig().LogLevelStreamMappings.ContainsKey(logLevel))
            {
                return _logConfig().LogLevelStreamMappings[logLevel];
            }
            switch (logLevel)
            {
                case LogLevel.Trace:
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
                    return PoshLogStream.Information;
                default:
                    return PoshLogStream.Information;
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
    internal class PoshLoggerOptionsSetup : ConfigureFromConfigurationOptions<PoshILoggerConfiguration>
    {
        public PoshLoggerOptionsSetup(ILoggerProviderConfiguration<PoshILoggerProvider>
                                      providerConfiguration)
            : base(providerConfiguration.Configuration)
        {
        }
    }
    public static class PoshILoggerExtensions
    {
        public static ILoggingBuilder AddPoshLogger(this ILoggingBuilder builder, PoshILoggerConfiguration config)
        {
            builder.AddConfiguration();
            return builder;
        }
        public static ILoggingBuilder AddPoshLogger(
            this ILoggingBuilder builder)
        {
            builder.AddConfiguration();

            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<ILoggerProvider, PoshILoggerProvider>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton
           <IConfigureOptions<PoshILoggerConfiguration>, PoshLoggerOptionsSetup>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton
           <IOptionsChangeTokenSource<PoshILoggerConfiguration>,
           LoggerProviderOptionsChangeTokenSource<PoshILoggerConfiguration, PoshILogger>>());

            return builder;
        }

        public static ILoggingBuilder AddPoshLogger(
            this ILoggingBuilder builder,
            Action<PoshILoggerConfiguration> configure)
        {
            builder.AddPoshLogger();
            builder.Services.Configure(configure);

            return builder;
        }
    }
    public enum PoshLogStream
    {
        Output,
        Error,
        Warning,
        Verbose,
        Debug,
        Information,
        Progress
    }
}
