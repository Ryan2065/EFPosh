using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace PoshLogger
{
    public abstract class LoggerProvider : IDisposable, ILoggerProvider, ISupportExternalScope
    {
        ConcurrentDictionary<string, Logger> loggers = new ConcurrentDictionary<string, Logger>();
        IExternalScopeProvider fScopeProvider;
        protected IDisposable SettingsChangeToken;

        void ISupportExternalScope.SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            fScopeProvider = scopeProvider;
        }

        ILogger ILoggerProvider.CreateLogger(string Category)
        {
            return loggers.GetOrAdd(Category,
            (category) => {
                return new Logger(this, category);
            });
        }

        void IDisposable.Dispose()
        {
            if (!this.IsDisposed)
            {
                try
                {
                    Dispose(true);
                }
                catch
                {
                }

                this.IsDisposed = true;
                GC.SuppressFinalize(this);  // instructs GC not bother to call the destructor   
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (SettingsChangeToken != null)
            {
                SettingsChangeToken.Dispose();
                SettingsChangeToken = null;
            }
        }

        public LoggerProvider()
        {
        }

        ~LoggerProvider()
        {
            if (!this.IsDisposed)
            {
                Dispose(false);
            }
        }

        public abstract bool IsEnabled(LogLevel logLevel);

        public abstract void WriteLog(LogEntry Info);

        internal IExternalScopeProvider ScopeProvider
        {
            get
            {
                if (fScopeProvider == null)
                    fScopeProvider = new LoggerExternalScopeProvider();
                return fScopeProvider;
            }
        }

        public bool IsDisposed { get; protected set; }
    }
}
