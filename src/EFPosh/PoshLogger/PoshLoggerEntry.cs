﻿using System;
using System.Collections.Generic;
using System.Text;

namespace PoshLogger
{
    /// <summary>
    /// Message to be queued for writing in PowerShell
    /// </summary>
    public class PoshLoggerEntry
    {
        public string Message { get; set; }
        public Exception Exception { get; set; }
        public PoshLogLevel Level { get; set; }
    }
}
