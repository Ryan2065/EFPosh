Param(
    [string]$Server = '##Server##',
    [string]$Database = '##Database##',
    [bool]$IntegratedSecurity = '##IntegratedSecurity##',
    [PScredential]$Credential,
    [ValidateSet('Interactive', 'Network', 'Batch', 'Service', 'Unlock', 'NetworkCleartext', 'NewCredentials')]
    [string]$LogonType = 'Network'
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

$optionalParams = @{}
if($Credential){
    $optionalParams['Credential'] = $Credential
    $optionalParams['LogonType'] = $LogonType
}

return (New-EFPoshContext -MSSQLServer $Server -MSSQLDatabase $Database -MSSQLIntegratedSecurity $IntegratedSecurity -Entities $Tables @optionalParams)
