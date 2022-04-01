using Microsoft.Extensions.Logging;
using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
#if net6_0
using System.Text.Json;
#endif
#if NETSTANDARD2_0
using Newtonsoft.Json;
#endif

namespace EFPosh
{
    public class PowerShellLogger : ILogger
    {
        //private readonly System.Management.Automation.Host.PSHost _host;
        private readonly PSCmdlet _cmdletObject;
        private string _loggingContext = "";

        public PowerShellLogger(PSCmdlet cmdletObject)
        {
            _cmdletObject = cmdletObject;
         }
        public IDisposable BeginScope<TState>(TState state)
        {
            var s = state as IDisposable;
#if net6_0
            _loggingContext = JsonSerializer.Serialize(s);
#endif
#if NETSTANDARD2_0
            _loggingContext = JsonConvert.SerializeObject(s);
#endif
            return s;
        }


        public bool IsEnabled(LogLevel logLevel)
        {
            if(_cmdletObject != null)
            {
                return true;
            }
            return false;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }
            
            string message = $"{_loggingContext}{formatter(state, exception)}  [EventId:{eventId.Id}]";
            
            switch (logLevel)
            {
                case LogLevel.Trace:
                    _cmdletObject.WriteDebug(message);
                    break;
                case LogLevel.Debug:
                    _cmdletObject.WriteDebug(message);
                    break;
                case LogLevel.Information:
                    _cmdletObject.WriteInformation(new InformationRecord(message, null));
                    break;
                case LogLevel.Warning:
                    _cmdletObject.WriteWarning(message);
                    break;
                case LogLevel.Error:
                    _cmdletObject.WriteDebug(message);
                    _cmdletObject.WriteError(new ErrorRecord(exception, "", ErrorCategory.NotSpecified, null));
                    break;
                case LogLevel.Critical:
                    _cmdletObject.WriteWarning(message);
                    break;
                case LogLevel.None:
                    _cmdletObject.WriteInformation(new InformationRecord(message, null));
                    break;
                default:
                    break;
            }
            
        }
    }
}