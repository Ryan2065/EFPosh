Param(
    $SqliteFileName,
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
    ( New-EFPoshEntityDefinition -Type 'SchemaTables' -Keyless -FromSql "
    SELECT
        'dbo' AS TABLE_SCHEMA,
        sm.type AS TABLE_TYPE,
        sm.Name AS TABLE_NAME
    FROM sqlite_master sm
    WHERE sm.Name != 'sqlite_sequence'
    AND sm.type IN ('table', 'view')
    " ),
    ( New-EFPoshEntityDefinition -Type 'SchemaColumns' -Keyless -FromSql "
    SELECT
        'dbo' AS 'TABLE_SCHEMA'
        ,m.Name AS 'TABLE_NAME'
        ,p.Name AS 'COLUMN_NAME'
        ,CASE p.[notnull]
            WHEN 0
                THEN 'YES'
            ELSE 'NO'
            END 'IS_NULLABLE'
        ,p.type AS 'DATA_TYPE'
    FROM sqlite_master m
    left outer join pragma_table_info((m.name)) p
        on m.name <> p.name
    WHERE m.Name != 'sqlite_sequence'
    AND m.type IN ('table', 'view')
    " ),
    ( New-EFPoshEntityDefinition -Type 'SchemaPrimaryKeys' -Keyless -FromSql "
    SELECT
        'dbo' AS 'TABLESCHEMA'
        ,m.Name AS 'TABLENAME'
        ,p.Name AS 'PRIMARYKEYCOLUMN'
        ,p.pk AS 'POSITION'
    FROM sqlite_master m
    left outer join pragma_table_info((m.name)) p
        on m.name <> p.name
    WHERE m.Name != 'sqlite_sequence'
    AND m.type IN ('table', 'view')
    AND p.pk > 0
    " )
)

$NewContextParams = @{}

if([string]::IsNullOrEmpty($ConnectionString)){
    $NewContextParams = @{
        'SQLiteFile' = $SqliteFileName
    }
}
else{
    $NewContextParams = @{
        'ConnectionString' = $ConnectionString
        'DbType' = 'SQLite'
    }
}

return (New-EFPoshContext @NewContextParams -Entities $Tables)

