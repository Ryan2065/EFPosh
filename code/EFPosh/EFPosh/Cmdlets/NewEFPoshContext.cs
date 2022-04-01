using EFPosh.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EFPosh.Cmdlets
{
    [Cmdlet(VerbsCommon.New, "EFPoshContext")]
    public class NewEFPoshContext : PSCmdlet
    {
        [Parameter(Mandatory = true, ParameterSetName = "ConnectionString")]
        public string ConnectionString { get; set; }
        [Parameter(Mandatory = false, ParameterSetName = "ConnectionString")]
        public EfDbType DbType { get; set; } = EfDbType.MSSQL;
        [Parameter(Mandatory = true, ParameterSetName = "SQLite")]
        public string SqliteFile { get; set; }
        [Parameter(Mandatory = true, ParameterSetName = "MSSQL")]
        public string MsSqlServer { get; set; }
        [Parameter(Mandatory = true, ParameterSetName = "MSSQL")]
        public string MsSqlDatabase { get; set; }
        [Parameter(Mandatory = false, ParameterSetName = "MSSQL")]
        public SwitchParameter MsSqlIntegratedSecurity { get; set; }
        [Parameter(Mandatory = false, ParameterSetName = "ConnectionString")]
        [Parameter(Mandatory = false, ParameterSetName = "SQLite")]
        [Parameter(Mandatory = false, ParameterSetName = "MSSQL")]
        public EfEntity[] Entities { get; set; }
        [Parameter(Mandatory = false, ParameterSetName = "ConnectionString")]
        [Parameter(Mandatory = false, ParameterSetName = "SQLite")]
        [Parameter(Mandatory = false, ParameterSetName = "MSSQL")]
        public SwitchParameter EnsureCreated { get; set; }
        [Parameter(Mandatory = false, ParameterSetName = "ConnectionString")]
        [Parameter(Mandatory = false, ParameterSetName = "SQLite")]
        [Parameter(Mandatory = false, ParameterSetName = "MSSQL")]
        public SwitchParameter ReadOnly { get; set; }
        [Parameter(Mandatory = false, ParameterSetName = "ConnectionString")]
        [Parameter(Mandatory = false, ParameterSetName = "SQLite")]
        [Parameter(Mandatory = false, ParameterSetName = "MSSQL")]
        public Type ExistingContext { get; set; }
    }
}
