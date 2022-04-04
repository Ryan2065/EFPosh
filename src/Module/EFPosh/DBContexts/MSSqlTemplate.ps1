Param(
    [string]$Server = '##Server##',
    [string]$Database = '##Database##',
    [bool]$IntegratedSecurity = '##IntegratedSecurity##',
    [switch]$EFPoshLog
)

if($null -eq ( Get-Module EFPosh )){
    Import-Module EFPosh -ErrorAction Stop
}

if($EFPoshLog){
    $Env:EFPoshLog = 'true'
}
else{
    $Env:EFPoshLog = $null
}

#region Class Definitions

##ClassDefinitions##

#endregion

$Tables = @()

#region TableArray

##TableArray##

#EndRegion


return (New-EFPoshContext -MSSQLServer $Server -MSSQLDatabase $Database -MSSQLIntegratedSecurity $IntegratedSecurity -Entities $Tables)
