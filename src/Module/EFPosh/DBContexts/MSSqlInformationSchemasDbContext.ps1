Param(
    $Server,
    $Database,
    $IntegratedSecurity,
    $ConnectionString
)

if($null -eq ( Get-Module EFPosh )){
    Import-Module EFPosh -ErrorAction Stop
}

Class SchemaTables {
    [string]$TABLE_SCHEMA
    [string]$TABLE_NAME
    [string]$TABLE_TYPE
}

Class SchemaColumns {
    [string]$TABLE_SCHEMA
    [string]$TABLE_NAME
    [string]$COLUMN_NAME
    [string]$IS_NULLABLE
    [string]$DATA_TYPE
}

Class SchemaPrimaryKeys {
    [string]$TABLENAME
    [string]$PRIMARYKEYCOLUMN
    [string]$TABLESCHEMA
    [int]$POSITION
}

$Tables = @(
    ( New-EFPoshEntityDefinition -Type 'SchemaTables' -TableName 'TABLES' -Schema 'INFORMATION_SCHEMA' -Keyless ),
    ( New-EFPoshEntityDefinition -Type 'SchemaColumns' -TableName 'COLUMNS' -Schema 'INFORMATION_SCHEMA' -Keyless ),
    ( New-EFPoshEntityDefinition -Type 'SchemaPrimaryKeys' -Keyless -FromSQL "
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
    " )
)

$NewContextParams = @{}

if([string]::IsNullOrEmpty($ConnectionString)){
    $NewContextParams = @{
        'MSSQLServer' = $Server
        'MSSQLDatabase' = $Database
        'MSSQLIntegratedSecurity' = $IntegratedSecurity
    }
}
else{
    $NewContextParams = @{
        'ConnectionString' = $ConnectionString
        'DbType' = 'MSSQL'
    }
}

return (New-EFPoshContext @NewContextParams -Entities $Tables)

