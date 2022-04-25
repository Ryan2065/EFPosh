using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace EFPosh
{
    [Cmdlet(VerbsCommon.New, "EFPoshContext")]
    public class NewEFPoshContext : PSCmdlet
    {
        [Parameter(Mandatory = true, ParameterSetName="ConnectionString")]
        public string ConnectionString { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "ConnectionString")]
        [ValidateSet("SQLite", "MSSQL")]
        public string DBType { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "SQLite")]
        public string SQLiteFile { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "MSSQL")]
        public string MSSQLServer { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "MSSQL")]
        public string MSSQLDatabase { get; set; }

        [Parameter(Mandatory = false, ParameterSetName = "MSSQL")]
        public bool MSSQLIntegratedSecurity { get; set; } = false;

        [Parameter(Mandatory = false, ParameterSetName = "ConnectionString")]
        [Parameter(Mandatory = false, ParameterSetName = "SQLite")]
        [Parameter(Mandatory = false, ParameterSetName = "MSSQL")]
        public EFPosh.PoshEntity[] Entities { get; set; }

        [Parameter(Mandatory = false, ParameterSetName = "ConnectionString")]
        [Parameter(Mandatory = false, ParameterSetName = "SQLite")]
        [Parameter(Mandatory = false, ParameterSetName = "MSSQL")]
        public SwitchParameter EnsureCreated { get; set; }

        [Parameter(Mandatory = false, ParameterSetName = "ConnectionString")]
        [Parameter(Mandatory = false, ParameterSetName = "SQLite")]
        [Parameter(Mandatory = false, ParameterSetName = "MSSQL")]
        public SwitchParameter RunMigrations { get; set; }

        [Parameter(Mandatory = false, ParameterSetName = "ConnectionString")]
        [Parameter(Mandatory = false, ParameterSetName = "SQLite")]
        [Parameter(Mandatory = false, ParameterSetName = "MSSQL")]
        public SwitchParameter ReadOnly { get; set; }

        [Parameter(Mandatory = false, ParameterSetName = "ConnectionString")]
        [Parameter(Mandatory = false, ParameterSetName = "SQLite")]
        [Parameter(Mandatory = false, ParameterSetName = "MSSQL")]
        public string AssemblyFile { get; set; }

        [Parameter(Mandatory = false, ParameterSetName = "ConnectionString")]
        [Parameter(Mandatory = false, ParameterSetName = "SQLite")]
        [Parameter(Mandatory = false, ParameterSetName = "MSSQL")]
        public string ClassName { get; set; }

        protected override void BeginProcessing()
        {
            if(Entities != null && !string.IsNullOrEmpty(AssemblyFile))
            {
                throw new ArgumentException("Entities parameter can not be used with AssemblyFile - please use one or the other");
            }
            else if(!string.IsNullOrEmpty(AssemblyFile) && string.IsNullOrEmpty(ClassName))
            {
                throw new ArgumentException("You must provide the ClassName to look for in the assembly");
            }

        }
        protected override void ProcessRecord()
        {
            EFPoshState.LatestDbContext = new EFPosh.DbContextInteractions();
            if(ParameterSetName == "SQLite")
            {
                DBType = "SQLite";
                ConnectionString = $"FileName={SQLiteFile}";
            }
            else if(ParameterSetName == "MSSQL")
            {
                DBType = "MSSQL";
                ConnectionString = $"Data Source={MSSQLServer};Initial Catalog={MSSQLDatabase};Integrated Security={MSSQLIntegratedSecurity}";
            }
        }
        protected override void EndProcessing()
        {
            if (string.IsNullOrEmpty(AssemblyFile))
            {
                EFPoshState.LatestDbContext.NewPoshContext(ConnectionString, DBType, Entities, EnsureCreated.ToBool(), RunMigrations.ToBool(), ReadOnly.ToBool());
            }
            else
            {
                EFPoshState.LatestDbContext.ExistingContext(ConnectionString, DBType, EnsureCreated.ToBool(), RunMigrations.ToBool(), ReadOnly.ToBool(), AssemblyFile, ClassName);
            }
            WriteObject(EFPoshState.LatestDbContext);
        }
    }
}
