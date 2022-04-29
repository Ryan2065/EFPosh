Function Start-EFPoshModel {
    <#
    .SYNOPSIS
    Starts a modeling of an existing database
    
    .DESCRIPTION
    Starts the modeling of an existing database
    
    .PARAMETER MSSQLServer
    SQL Server instance name
    
    .PARAMETER MSSQLDatabase
    SQL Server database
    
    .PARAMETER MSSQLIntegratedSecurity
    Integrated security
    
    .PARAMETER FilePath
    Path to output the file
    
    .PARAMETER Overwrite
    Overwrite an existing file or add to it?
    
    .PARAMETER EntitesToMap
    List of entities to map. Optional, can be provided with Add-EFPoshModelEntity also
    
    .PARAMETER AllEntities
    Switch - Will model the entire database. Only recommended for small databases
    
    .NOTES
    .Author: Ryan Ephgrave
    #>
    Param(
        [Parameter(Mandatory = $true, ParameterSetName = "MSSQL")]
        [string]$MSSQLServer,
        [Parameter(Mandatory = $true, ParameterSetName = "MSSQL")]
        [string]$MSSQLDatabase,
        [Parameter(Mandatory = $false, ParameterSetName = "MSSQL")]
        [bool]$MSSQLIntegratedSecurity = $true,
        [Parameter(Mandatory = $true, ParameterSetName = "MSSQL")]
        [string]$FilePath,
        [Parameter(Mandatory = $false, ParameterSetName = "MSSQL")]
        [switch]$Overwrite,
        [Parameter(Mandatory = $false, ParameterSetName = "MSSQL")]
        [string[]]$EntitesToMap,
        [Parameter(Mandatory = $false, ParameterSetName = "MSSQL")]
        [switch]$AllEntities
    )
    Write-Verbose 'Creating new Model object'
    $ParentDirectory = ( Get-Item $PSScriptRoot ).Parent.FullName
    if($Overwrite){
        If(Test-Path $FilePath) { Remove-Item $FilePath -Force -ErrorAction Stop }
        If(Test-Path $FilePath) {
            throw "Could not remove existing file $FilePath with Remove-Item - Please remove it manually first"
            return
        }
    }
    If(-not (Test-Path $FilePath)) {
        $TemplateFile = [System.IO.Path]::Combine($ParentDirectory, 'DBContexts', 'MSSqlTemplate.ps1')
        $null = Copy-Item $TemplateFile -Destination $FilePath -Force
        $Content = Get-Content $FilePath -Raw
        $Content = $Content.Replace('##Server##', $MSSQLServer).Replace('##Database##', $MSSQLDatabase).Replace("'##IntegratedSecurity##'", "`$$MSSQLIntegratedSecurity")
        $Content | Out-File $FilePath -Force -Encoding utf8
    }
    $SchemaDbContext = [System.IO.Path]::Combine($ParentDirectory, 'DBContexts', 'MSSqlInformationSchemasDbContext.ps1')
    $Context = . $SchemaDbContext -Server $MSSQLServer -Database $MSSQLDatabase -IntegratedSecurity $MSSQLIntegratedSecurity
    $TableList = @{}
    $ViewList = @{}
    $SchemaList = New-Object System.Collections.Generic.List[string]
    
    $AllTableInformation = $Context.MSSQLInformationSchemaTables.ToList()
    
    $UniqueSchemas = $AllTableInformation.TABLE_SCHEMA | Select-Object -Unique
    foreach($schema in $UniqueSchemas) { $SchemaList.Add($schema) }
    foreach($schema in $UniqueSchemas) {
        $TableList[$schema] = New-Object System.Collections.Generic.List[string]
        $ViewList[$schema] = New-Object System.Collections.Generic.List[string]
        foreach($table in $AllTableInformation) {
            if($table.TABLE_SCHEMA -eq $schema -and $table.TABLE_TYPE -ne 'View'){
                $TableList[$schema].Add($table.TABLE_NAME)
            }
            elseif($table.TABLE_SCHEMA -eq $schema) {
                $ViewList[$schema].Add($table.TABLE_NAME)
            }
        }
    }
    
    $Script:ContextFileSettings = @{
        'Context' = $Context
        'FilePath' = $FilePath
        'EntitiesToMap' = @($EntitesToMap)
        'Schemas' = $SchemaList
        'Tables' = $TableList
        'Views' = $ViewList
    }
    if($AllEntities) {
        $EntitesToMap = $AllTableInformation.TABLE_NAME
    }
    if($EntitesToMap.Count -gt 0){
        Foreach($entity in $EntitesToMap){
            Foreach($tableInfo in $AllTableInformation){
                if($tableInfo.Table_Name -eq $entity){
                    $null = Add-EFPoshModelEntity -DBSchema $tableInfo.TABLE_SCHEMA -Name $tableInfo.Table_Name
                }
            }
        }
    }
}