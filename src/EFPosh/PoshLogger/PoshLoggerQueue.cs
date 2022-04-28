using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PoshLogger
{
    public static class PoshLoggerQueue
    {
        public delegate void EnqueueHandler(PoshLoggerEntry handler);

        public static event EnqueueHandler OnEnqueue;
        
        private static readonly ConcurrentQueue<PoshLoggerEntry> _queue = new ConcurrentQueue<PoshLoggerEntry>();

        private static bool Enabled { get; set; } = false;
        
        public static void Enqueue(PoshLoggerEntry entry)
        {
            if (!Enabled)
            {
                return;
            }
            _queue.Enqueue(entry);
            if(OnEnqueue == null) { return; }
            try
            {
                OnEnqueue(entry);
            }
            catch
            {
                // sometimes this will fail. If this is running from a separate thread from the runspace, it'll fail
            }
        }
        
        public static IEnumerable<PoshLoggerEntry> Dequeue()
        {
            while(_queue.TryDequeue(out var queueItem))
            {
                yield return queueItem;
            }
        }

        public static void Enable()
        {
            if (Enabled) { return; }
            try
            {
                
                var powerShell = System.Management.Automation.PowerShell.Create(System.Management.Automation.RunspaceMode.CurrentRunspace);
                powerShell.Commands.Clear();
                powerShell.Commands.AddScript(@"
                    [PoshLogger.PoshLoggerQueue]::add_OnEnqueue({
                        Param($entry)
                        [PoshLogger.PoshLoggerQueue]::Dequeue() | foreach-object {
                            if($_.Level -eq 'Information') {
                                Write-Information $_.Message
                            }
                            elseif($_.Level -eq 'Verbose') {
                                Write-Verbose $_.Message
                            }
                            elseif($_.Level -eq 'Debug') {
                                Write-Debug $_.Message
                            }
                            elseif($_.Level -eq 'Progress') {
                                Write-Progress $_.Message
                            }
                            elseif($_.Level -eq 'Warning') {
                                Write-Warning $_.Message
                            }
                            elseif($_.Level -eq 'Error') {
                                if($null -ne $_.Exception){
                                    Write-Error -Exception $_.Exception -Message $_.Message -ErrorAction Continue
                                }
                                else{
                                    Write-Error -Message $_.Message -ErrorAction Continue
                                }
                            }
                        }

                    })
                "
                );
                powerShell.Invoke();
            }
            catch
            {
                throw;
                //Might not be running in PowerShell
                //return;
            }

            Enabled = true;
        }
    }


}
