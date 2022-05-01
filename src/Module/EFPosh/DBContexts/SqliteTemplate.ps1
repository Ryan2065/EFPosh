using namespace System.ComponentModel.DataAnnotations.Schema;
using namespace System.ComponentModel.DataAnnotations;
using namespace System.Collections.Generic;
Param(
    [string]$SQLiteFile = '##SQLiteFile##',
    [string]$ConnectionString
)

if($null -eq ( Get-Module EFPosh )){
    Import-Module EFPosh -ErrorAction Stop
}


#region Class Definitions

##ClassDefinitions##

#endregion

$Tables = @()

#region TableArray

##TableArray##

#EndRegion

$NewContextParams = @{}

if([string]::IsNullOrEmpty($ConnectionString)){
    $NewContextParams = @{
        'SQLiteFile' = $SQLiteFile
    }
}
else{
    $NewContextParams = @{
        'ConnectionString' = $ConnectionString
        'DbType' = 'SQLite'
    }
}

return (New-EFPoshContext @NewContextParams -Entities $Tables)
