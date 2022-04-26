#File should be able to be called from Posh 5 or Posh 7
. C:\Users\Ryan2\OneDrive\Code\EFPosh\build.ps1
Import-Module Pester -MinimumVersion 5.3 -Verbose:$false

$ErrorActionPreference = 'Stop'

if(-not (Test-Path "$PSScriptRoot\bin")){
    $null = New-Item "$PSScriptRoot\bin" -Type Directory -Force
}

$Parent = (Get-Item $PSScriptRoot).Parent.FullName

$Global:DebugPreference = 'Continue'
Import-Module "$Parent\Module\EFPosh" -Force

#Import-Module "C:\Users\Ryan2\OneDrive\Code\EAMonitor\src\EAMonitor\EAMonitor.psm1" -Force
#Initialize-EAMonitor -CreateDb -DetectMonitorModules -ConnectionString $env:AzureConString -DatabaseType MSSQL


$Contexts = . "$PSScriptRoot\EFPoshTestContext.ps1"

return

New-ModuleManifest -Path C:\Users\Ryan2\OneDrive\Code\EFPosh\src\EFPosh\BinaryExpressionConverter\BinaryExpressionConverter.psd1 -Author 'Ryan Ephgrave' -RootModule 'BinaryExpressionConverter.dll'
