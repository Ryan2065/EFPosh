using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PoshLogger
{
    /// <summary>
    /// Queue class to write the logs in PowerShell
    /// </summary>
    public static class PoshLoggerQueue
    {
        /// <summary>
        /// Event handler when a new message is added
        /// </summary>
        /// <param name="handler"></param>
        public delegate void EnqueueHandler(PoshLoggerEntry handler);
        /// <summary>
        /// Event when a new message is added - is subscribed to inside of PowerShell
        /// </summary>
        public static event EnqueueHandler OnEnqueue;
        /// <summary>
        /// The queue of messages to write
        /// </summary>
        private static readonly ConcurrentQueue<PoshLoggerEntry> _queue = new ConcurrentQueue<PoshLoggerEntry>();
        /// <summary>
        /// Is this enabled? Is true if the event is set up in PowerShell
        /// </summary>
        private static bool Enabled { get; set; } = false;
        /// <summary>
        /// Queue up a log. Sometimes this will be called from another thread, and if it is OnEnqueue will fail
        /// That's ok, .net is loud and the next time it's called from the correct thread those messages will get picked up
        /// </summary>
        /// <param name="entry">Log entry to queue up</param>
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
        /// <summary>
        /// Method called from PowerShell to get all queued messages to write
        /// </summary>
        /// <returns>Queued messages</returns>
        public static IEnumerable<PoshLoggerEntry> Dequeue()
        {
            while(_queue.TryDequeue(out var queueItem))
            {
                yield return queueItem;
            }
        }
        /// <summary>
        /// Sets up an event in PowerShell to respond to the event raised when new messages are queued.
        /// Will perform the correct write action based on the queued messages
        /// </summary>
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
                //Might not be running in PowerShell - this just disables it
                return;
            }

            Enabled = true;
        }
    }


}
