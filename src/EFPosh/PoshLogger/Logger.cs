using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoshLogger
{
    internal class Logger : ILogger
    {
        public Logger(LoggerProvider Provider, string Category)
        {
            this.Provider = Provider;
            this.Category = Category;
        }

        IDisposable ILogger.BeginScope<TState>(TState state)
        {
            return Provider.ScopeProvider.Push(state);
        }

        bool ILogger.IsEnabled(LogLevel logLevel)
        {
            return Provider.IsEnabled(logLevel);
        }

        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId,
            TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if ((this as ILogger).IsEnabled(logLevel))
            {
                LogEntry Info = new LogEntry();
                Info.Category = this.Category;
                Info.Level = logLevel;
                // well, the passed default formatter function 
                // does not take the exception into account
                // SEE: https://github.com/aspnet/Extensions/blob/master/src/Logging/Logging.Abstractions/src/LoggerExtensions.cs
                Info.Text = exception?.Message ?? state.ToString(); // formatter(state, exception)
                Info.Exception = exception;
                Info.EventId = eventId;
                Info.State = state;

                // well, you never know what it really is
                if (state is string)
                {
                    Info.StateText = state.ToString();
                }
                // in case we have to do with a message template, 
                // let's get the keys and values (for Structured Logging providers)
                // SEE: https://docs.microsoft.com/en-us/aspnet/core/
                // fundamentals/logging#log-message-template
                // SEE: https://softwareengineering.stackexchange.com/
                // questions/312197/benefits-of-structured-logging-vs-basic-logging
                else if (state is IEnumerable<KeyValuePair<string, object>> Properties)
                {
                    Info.StateProperties = new Dictionary<string, object>();

                    foreach (KeyValuePair<string, object> item in Properties)
                    {
                        Info.StateProperties[item.Key] = item.Value;
                    }
                }

                // gather info about scope(s), if any
                if (Provider.ScopeProvider != null)
                {
                    Provider.ScopeProvider.ForEachScope((value, loggingProps) =>
                    {
                        if (Info.Scopes == null)
                            Info.Scopes = new List<LogScopeInfo>();

                        LogScopeInfo Scope = new LogScopeInfo();
                        Info.Scopes.Add(Scope);

                        if (value is string)
                        {
                            Scope.Text = value.ToString();
                        }
                        else if (value is IEnumerable<KeyValuePair<string, object>> props)
                        {
                            if (Scope.Properties == null)
                                Scope.Properties = new Dictionary<string, object>();

                            foreach (var pair in props)
                            {
                                Scope.Properties[pair.Key] = pair.Value;
                            }
                        }
                    },
                    state);
                }

                Provider.WriteLog(Info);
            }
        }

        public LoggerProvider Provider { get; private set; }
        public string Category { get; private set; }
    }
}
