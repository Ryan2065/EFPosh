Function New-EFPoshEntityDefinition{
    <#
        .SYNOPSIS
        Creates an entity definition object to power creating the tables of the DBContext
        
        .DESCRIPTION
        Creates an entity definition object to power creating the tables of the DBContext
        
        .PARAMETER Type
        The type you want to map the entity to
        
        .PARAMETER PrimaryKeys
        The primary key(s) (unique column(s)) of the table. Required if you want write access to the table
        
        .PARAMETER Keyless
        If there is no primary key (or it's a view) select this option to make the entity read only.
        
        .PARAMETER TableName
        Table (or view name) of the object in the database. If not provided, the name of the Type will be used.

        .PARAMETER Schema
        DB schema type - only used if the database type is MSSQL as SQLite doesn't have a schema. Defaults to dbo

        .EXAMPLE
        New-EFPoshEntityDefinition -Type 'MyClassName' -PrimaryKeys @('UniqueColumn')
        
        .NOTES
        .Author: Ryan Ephgrave
    #>
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$true)]
        [string]$Type,
        [Parameter(Mandatory=$false)]
        [string[]]$PrimaryKeys,
        [Parameter(Mandatory=$false)]
        [switch]$Keyless,
        [Parameter(Mandatory=$false)]
        [string]$TableName,
        [Parameter(Mandatory = $false)]
        [string]$Schema
    )
    Write-Verbose "Creating Entity Definition for type $($Type)"
    $TypeObject = Get-EFPoshType -TypeName $Type
    if($null -eq $TypeObject){
        throw 'Could not find provided type. Please make sure it is imported'
    }
    $newEntity = [EFPosh.PoshEntity]::new()
    $newEntity.Type = $TypeObject
    $newEntity.PrimaryKeys = $PrimaryKeys
    if($Keyless){
        $newEntity.Keyless = $true
    }
    $newEntity.Schema = $Schema
    $newEntity.TableName = $TableName
    return $newEntity
}