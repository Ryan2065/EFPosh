Function Add-EFPoshModelEntity {
    <#
    .SYNOPSIS
    Used to generate a DB schema - adds an entity to the model. Currently only works with MSSql
    
    .DESCRIPTION
    Used to generate a DB schema - adds an entity to the model
    
    .PARAMETER DBSchema
    Schema the entity is in - defaults to dbo
    
    .PARAMETER EntityType
    Is the entity a Table or View? Only used for narrowing auto-complete of Name.
    
    .PARAMETER Name
    Name of the View/Table
    
    .PARAMETER PropertyList
    List of properties you want modeled - defaults to all

    .PARAMETER FromSql
    An optional SQL query to use as a base for querying the entity
    
    .NOTES
    .Author: Ryan Ephgrave
    #>
    Param(
        [Parameter(Mandatory = $false)]
        [ArgumentCompleter({
            Param($CommandName, $ParameterName, $WordToComplete, $CommandAst, $FakeBoundParameters)
            $SchemaList = ( Get-Module EFPosh ).Invoke( { $Script:ContextFileSettings.Schemas } )
            foreach($instance in $SchemaList){
                if($instance -like "$($WordToComplete)*"){
                    New-Object System.Management.Automation.CompletionResult (
                        $instance,
                        $instance,
                        'ParameterValue',
                        $instance
                    )
                }
            }
        })]
        [string]$DBSchema = 'dbo',
        [Parameter(Mandatory=$false)]
        [ValidateSet('Table', 'View', 'Function')]
        [string]$EntityType,
        [Parameter(Mandatory = $true)]
        [ArgumentCompleter({
            Param($CommandName, $ParameterName, $WordToComplete, $CommandAst, $FakeBoundParameters)
            $TableHash = ( Get-Module EFPosh ).Invoke( { $Script:ContextFileSettings.Tables } )
            $ViewHash = ( Get-Module EFPosh ).Invoke( { $Script:ContextFileSettings.Views } )
            if($FakeBoundParams.Keys -Contains 'DBSchema'){
                $Schema = $FakeBoundParameters.DBSchema
            }
            else{
                $Schema = 'dbo'
            }
            if($FakeBoundParams.Keys -Contains 'EntityType'){
                $ET = $FakeBoundParameters.EntityType
            }
            else{
                $ET = $null
            }
            $TableList = $TableHash[$Schema]
            $ViewList = $ViewHash[$schema]
            if($null -eq $Schema) { $Schema = 'dbo' }
            if($ET -ne 'View'){
                foreach($instance in $TableList){
                    if($instance -like "*$($WordToComplete)*"){
                        New-Object System.Management.Automation.CompletionResult (
                            $instance,
                            $instance,
                            'ParameterValue',
                            $instance
                        )
                    }
                }
            }
            if($ET -ne 'Table'){
                foreach($instance in $ViewList){
                    if($instance -like "*$($WordToComplete)*"){
                        New-Object System.Management.Automation.CompletionResult (
                            $instance,
                            $instance,
                            'ParameterValue',
                            $instance
                        )
                    }
                }
            }
        })]
        [string]$Name,
        [Parameter(Mandatory = $false)]
        [string[]]$PropertyList,
        [Parameter(Mandatory=$false)]
        [string[]]$FromSql
    )
    if($null -eq $Script:ContextFileSettings) {
        throw 'Must first run Start-EFPoshModel to set the DB and file information'
        return
    }
    $Context = $Script:ContextFileSettings.Context
    if($EntityType -eq 'Function'){
        $SchemaInfo = Search-EFPosh -DbContext $Context -Entity 'MSSQLInformationSchemaColumns' -FromSql 'Select * FROM Information_Schema.Routine_Columns' -Expression { $_.TABLE_SCHEMA -eq $DBSchema -and $_.TABLE_NAME -eq $Name }
    }
    else{
        $SchemaInfo = Search-EFPosh -DbContext $Context -Entity 'MSSQLInformationSchemaColumns'  -Expression { $_.TABLE_SCHEMA -eq $DBSchema -and $_.TABLE_NAME -eq $Name }
    }
    
    if($null -eq $PropertyList) {
        $PropertyList = $SchemaInfo.COLUMN_NAME
    }

    $ClassBuilder = @("Class $($Name) {")

    foreach($prop in $SchemaInfo){
        if($PropertyList -contains $prop.COLUMN_NAME){
            $type = Get-EFPoshPowerShellType -ColumnType $prop.DATA_TYPE -Nullable:($prop.IS_NULLABLE -ne 'NO')
            $ClassBuilder += "    $type `$$($prop.COLUMN_NAME -replace "\W" )"
        }
    }
    $ClassBuilder += "}"
    $ClassBuilder += ""
    $ClassBuilder += "##ClassDefinitions##"
    $ClassString = $ClassBuilder -join [System.Environment]::NewLine
    $strPKs = '-Keyless'
    $foundPrimaryKeys = Search-EFPosh -DbContext $Context -Entity 'MSSQLInformationSchemaPrimaryKeys' -Expression { $_.TABLESCHEMA -eq $DBSchema -and $_.TABLENAME -eq $Name } -FromSql "
        SELECT 
            KU.table_name as TABLENAME
            ,column_name as PRIMARYKEYCOLUMN
            ,KU.TABLE_SCHEMA as TABLESCHEMA
            ,KU.ORDINAL_POSITION as POSITION
        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS TC
        INNER JOIN
            INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KU
                ON TC.CONSTRAINT_TYPE = 'PRIMARY KEY' AND
                    TC.CONSTRAINT_NAME = KU.CONSTRAINT_NAME 
    "
    if($foundPrimaryKeys.Count -gt 0){
        $foundPrimaryKeys = $foundPrimaryKeys | Sort-Object -Property POSITION
        
        $strPKs = "-PrimaryKeys @('"
        foreach($p in $foundPrimaryKeys){
            $strPKs += "$($p.PRIMARYKEYCOLUMN)','"
        }
        $strPks = $strPKs.TrimEnd(",'") + "')"
    }
    $TableArrayBuilder = @()
    $TableString = "`$Tables += New-EFPoshEntityDefinition -Type '$Name' -TableName '$Name' -Schema '$DBSchema' $strPks"
    if($FromSql){
        $TableString += " -FromSql '$($FromSql)'"
    }
    $TableArrayBuilder += $TableString
    $TableArrayBuilder += "##TableArray##"
    $TableArrayString = $TableArrayBuilder -join [System.Environment]::NewLine
    $Content = Get-Content $Script:ContextFileSettings.FilePath -Raw
    $Content = $Content.Replace('##ClassDefinitions##', $ClassString).Replace("##TableArray##", $TableArrayString).Trim()
    $Content | Out-File $Script:ContextFileSettings.FilePath -Force -Encoding utf8
}