Function Get-EFPoshPowerShellType {
    Param(
        [string]$DBType,
        [string]$ColumnType,
        [switch]$Nullable
    )
    $strType = "["
    if($Nullable){
        $strType += "Nullable["
    }
    Switch($ColumnType){
        "bigint" { $strType += "long" }
        "binary" { $strType = "[byte[]" }
        "bit" { $strType += "bool" }
        "char" { $strType = "[string" }
        "date" { $strType += "DateTime" }
        "datetime" { $strType += "DateTime" }
        "datetime2" { $strType += "DateTime" }
        "datetimeoffset" { $strType += "DateTimeOffset" }
        "decimal" { $strType += "decimal" }
        "float" { $strType += "double" }
        "image" { $strType = "[byte[]" }
        "int" { $strType += "int" }
        "money" { $strType += "decimal" }
        "nchar" { $strType = "[string" }
        "ntext" { $strType = "[string" }
        "numeric" { $strType += "decimal" }
        "nvarchar" { $strType = "[string" }
        "real" { $strType += "double" }
        "smalldatetime" { $strType += "DateTime" }
        "smallint" { $strType += "short" }
        'int16' { $strType += "short" }
        "smallmoney" { $strType += "decimal" }
        "text" { $strType = "[string" }
        "time" { $strType += "TimeSpan" }
        "timestamp" { $strType += "long" }
        "tinyint" { $strType += "byte" }
        "uniqueidentifier" { $strType += "Guid" }
        "varbinary" { $strType = "[byte[]" }
        "varchar" { $strType = "[string" }
        "xml" { $strType = "[string" }
        'boolean' { $strType = "bool" }
        'int32' { $strType += "int" }
        'INTEGER' { $strType += "long" }
        'int64' { $strType += "long" }
        'currency' { $strType += "decimal" }
        'smalldate' { $strType += "DateTime" }
        'BLOB' { $strType = "[byte[]" }
        default { $strType = "[byte[]" }
    }
    $strType += "]"
    if($strType.contains('[Nullable')){
        $strType += "]"
    }
    return $strType
}
