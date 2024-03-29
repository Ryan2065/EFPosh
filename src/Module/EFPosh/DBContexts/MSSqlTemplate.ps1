using namespace System.ComponentModel.DataAnnotations.Schema;
using namespace System.ComponentModel.DataAnnotations;
using namespace System.Collections.Generic;
Param(
    [string]$Server = '##Server##',
    [string]$Database = '##Database##',
    [bool]$IntegratedSecurity = '##IntegratedSecurity##',
    [string]$ConnectionString
)

if($null -eq ( Get-Module EFPosh )){
    Import-Module EFPosh -ErrorAction Stop
}
S

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
