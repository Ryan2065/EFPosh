Param(
    $Server,
    $Database,
    $IntegratedSecurity
)

if($null -eq ( Get-Module EFPosh )){
    Import-Module EFPosh -ErrorAction Stop
}

Class MSSQLInformationSchemaTables {
    [string]$TABLE_CATALOG
    [string]$TABLE_SCHEMA
    [string]$TABLE_NAME
    [string]$TABLE_TYPE
}

Class MSSQLInformationSchemaColumns {
    [string]$TABLE_CATALOG
    [string]$TABLE_SCHEMA
    [string]$TABLE_NAME
    [string]$COLUMN_NAME
    [int]$ORDINAL_POSITION
    [string]$IS_NULLABLE
    [string]$DATA_TYPE
}

$Tables = @(
    ( New-EFPoshEntityDefinition -Type 'MSSQLInformationSchemaTables' -TableName 'TABLES' -Schema 'INFORMATION_SCHEMA' -Keyless ),
    ( New-EFPoshEntityDefinition -Type 'MSSQLInformationSchemaColumns' -TableName 'COLUMNS' -Schema 'INFORMATION_SCHEMA' -Keyless )
)

return New-EFPoshContext -MSSQLServer $Server -MSSQLDatabase $Database -MSSQLIntegratedSecurity $IntegratedSecurity -Entities $Tables
