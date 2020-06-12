Param(
    [string]$MSSqlServer,
    [string]$MSSqlDB,
    [bool]$IntegratedSecurity,
    [PSCredential]$SQLiteAlternativeCredentials,
    [PSCredential]$MSSQLAlternativeCredentials
)

$SettingsHash = @{
    'MSSqlServer' = $MSSqlServer
    'MSSqlDB' = $MSSqlDB
    'IntegratedSecurity' = $IntegratedSecurity
    'SQLiteAlternativeCredentials-User' = $SQLiteAlternativeCredentials.UserName
    'SQLiteAlternativeCredentials-Pw' = $SQLiteAlternativeCredentials.Password | ConvertFrom-SecureString
    'MSSQLAlternativeCredentials-User' = $MSSQLAlternativeCredentials.UserName
    'MSSQLAlternativeCredentials-Pw' = $MSSQLAlternativeCredentials.Password | ConvertFrom-SecureString
}

$SettingsFile = "$PSScriptRoot\bin\TestSettings.json"
if(Test-Path $SettingsFile) { Remove-Item $SettingsFile -Force }
$null = $SettingsHash | ConvertTo-Json | Out-File -FilePath $SettingsFile -Force -Encoding utf8
