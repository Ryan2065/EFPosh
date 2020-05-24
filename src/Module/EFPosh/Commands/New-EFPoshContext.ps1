Function New-EFPoshContext{
    <#
    .SYNOPSIS
    Creates a new EFPosh Context Interaction object and stores it in the Module space for reuse
    
    .DESCRIPTION
    Creates a new EFPosh Context Interaction object and stores it in the Module space for reuse
    
    .PARAMETER ConnectionString
    ConnectionString to the database - accepts SQLite or MSSql connection strings
    
    .PARAMETER DBType
    Is this a SQLite or MSSql database we are connecting to
    
    .PARAMETER Entities
    A list of Entities that will map to database tables. Use New-EFPoshEntityDefinition to create these.
    
    .PARAMETER EnsureCreated
    Will create the database if it's not created already. If the database exists but the tables aren't correct, this will do nothing as it's only checking if it exists
    
    .EXAMPLE
    New-EFPoshContext -ConnectionString 'Filename=.\MyDatabase.sqlite' -DBType 'SQLite' -Types @('Table1','Table2') -EnsureCreated
    This will check if the file MyDatabase.sqlite exists, and if it doesn't create it with the DB schema for the provided types
    
    .NOTES
    .Author: Ryan Ephgrave
    #>
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$true, ParameterSetName = "ConnectionString")]
        [string]$ConnectionString,
        [Parameter(Mandatory = $false, ParameterSetName = "ConnectionString")]
        [ValidateSet('SQLite', 'MSSQL')]
        [string]$DBType = 'MSSQL',
        [Parameter(Mandatory = $true, ParameterSetName = "SQLite")]
        [string]$SQLiteFile,
        [Parameter(Mandatory = $true, ParameterSetName = "MSSQL")]
        [string]$MSSQLServer,
        [Parameter(Mandatory = $true, ParameterSetName = "MSSQL")]
        [string]$MSSQLDatabase,
        [Parameter(Mandatory = $false, ParameterSetName = "MSSQL")]
        [bool]$MSSQLIntegratedSecurity = $false,
        [Parameter(Mandatory = $false, ParameterSetName = "ConnectionString")]
        [Parameter(Mandatory = $false, ParameterSetName = "SQLite")]
        [Parameter(Mandatory = $false, ParameterSetName = "MSSQL")]
        [EFPosh.PoshEntity[]]$Entities,
        [Parameter(Mandatory = $false, ParameterSetName = "ConnectionString")]
        [Parameter(Mandatory = $false, ParameterSetName = "SQLite")]
        [Parameter(Mandatory = $false, ParameterSetName = "MSSQL")]
        [switch]$EnsureCreated,
        [Parameter(Mandatory = $false, ParameterSetName = "ConnectionString")]
        [Parameter(Mandatory = $false, ParameterSetName = "SQLite")]
        [Parameter(Mandatory = $false, ParameterSetName = "MSSQL")]
        [switch]$ReadOnly,
        [Parameter(Mandatory = $false, ParameterSetName = "ConnectionString")]
        [Parameter(Mandatory = $false, ParameterSetName = "SQLite")]
        [Parameter(Mandatory = $false, ParameterSetName = "MSSQL")]
        [string]$AssemblyFile,
        [Parameter(Mandatory = $false, ParameterSetName = "ConnectionString")]
        [Parameter(Mandatory = $false, ParameterSetName = "SQLite")]
        [Parameter(Mandatory = $false, ParameterSetName = "MSSQL")]
        [string]$ClassName
    )
    if($Entities -and $AssemblyFile){
        throw 'Entities parameter can not be used with AssemblyFile - please use one or the other'
        return
    }
    if($AssemblyFile -and -not $ClassName){
        throw 'You must provide the ClassName to look for in the assembly'
        return
    }
    $Script:LatestDBContext = $null
    $Script:LatestDBContext = [EFPosh.PoshContextInteractions]::new()
    $boolEnsureCreated = $false
    if($EnsureCreated){ $boolEnsureCreated = $true }
    $boolReadOnly = $false
    if($ReadOnly) { $boolReadOnly = $true }
    if($PSCmdlet.ParameterSetName -eq 'SQLite'){
        $DBType = 'SQLite'
        $ConnectionString = "Filename=$($SQLiteFile)"
    }
    elseif($PSCmdlet.ParameterSetName -eq 'MSSQL'){
        $DBType = 'MSSQL'
        $ConnectionString = "Server=$($MSSQLServer);Database=$($MSSQLDatabase);Integrated Security=$($MSSQLIntegratedSecurity)"
    }
    if([string]::IsNullOrEmpty($AssemblyFile)){
        $null = $Script:LatestDBContext.NewPoshContext($ConnectionString, $DBType, $Entities, $boolEnsureCreated, $boolReadOnly)
    }
    else{
        $null = $Script:LatestDBContext.ExistingContext($ConnectionString, $DBType, $boolEnsureCreated, $boolReadOnly, $AssemblyFile, $ClassName)
    }
    return $Script:LatestDBContext
}