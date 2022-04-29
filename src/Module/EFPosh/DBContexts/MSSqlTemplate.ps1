Param(
    [string]$Server = '##Server##',
    [string]$Database = '##Database##',
    [bool]$IntegratedSecurity = '##IntegratedSecurity##'
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


return (New-EFPoshContext -MSSQLServer $Server -MSSQLDatabase $Database -MSSQLIntegratedSecurity $IntegratedSecurity -Entities $Tables)
