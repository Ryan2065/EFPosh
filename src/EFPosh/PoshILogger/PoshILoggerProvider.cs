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
    public class PoshILoggerProvider : ILoggerProvider
    {
        private PoshILoggerConfiguration _currentConfig;
        private readonly IDisposable _onChangeToken;
        private readonly ConcurrentDictionary<string, PoshLogger> _loggers = new ConcurrentDictionary<string, PoshLogger>();
        public PoshILoggerProvider(IOptionsMonitor<PoshILoggerConfiguration> config)
        {
            _currentConfig = config.CurrentValue;
            _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
        }
        public ILogger CreateLogger(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName)) { categoryName = "Default"; }
            return _loggers.GetOrAdd(categoryName, p => new PoshLogger(categoryName, GetCurrentConfig));
        }
        private PoshILoggerConfiguration GetCurrentConfig() => _currentConfig;

        public void Dispose()
        {
            _loggers.Clear();
            _onChangeToken.Dispose();
        }
    }
}
