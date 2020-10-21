Param(
    [string]$Server = '##Server##',
    [string]$Database = '##Database##',
    [bool]$IntegratedSecurity = '##IntegratedSecurity##',
    [PScredential]$Credential,
    [ValidateSet('Interactive', 'Network', 'Batch', 'Service', 'Unlock', 'NetworkCleartext', 'NewCredentials')]
    [string]$LogonType = 'NewCredentials',
    [switch]$EFPoshLog
)

if($null -eq ( Get-Module EFPosh )){
    Import-Module EFPosh -ErrorAction Stop
}

if($EFLog){
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

$optionalParams = @{}
if($Credential){
    $optionalParams['Credential'] = $Credential
    $optionalParams['LogonType'] = $LogonType
}

return (New-EFPoshContext -MSSQLServer $Server -MSSQLDatabase $Database -MSSQLIntegratedSecurity $IntegratedSecurity -Entities $Tables @optionalParams)
