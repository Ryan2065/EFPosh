using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace PoshLogger
{
    public class PoshLoggerProvider : ILoggerProvider
    {
        private PoshLoggerConfiguration _currentConfig;
        private readonly IDisposable _onChangeToken;
        private readonly ConcurrentDictionary<string, PoshILogger> _loggers = new ConcurrentDictionary<string, PoshILogger>();
        public PoshLoggerProvider(IOptionsMonitor<PoshLoggerConfiguration> config)
        {
            _currentConfig = config.CurrentValue;
            _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
        }
        public ILogger CreateLogger(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName)) { categoryName = "PoshLoggerDefault"; }
            return _loggers.GetOrAdd(categoryName, p => new PoshILogger(GetCurrentConfig));
        }
        private PoshLoggerConfiguration GetCurrentConfig() => _currentConfig;

        public void Dispose()
        {
            _loggers.Clear();
            _onChangeToken.Dispose();
        }
    }
}
