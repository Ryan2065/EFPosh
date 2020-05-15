<#
  The PowerShellEditorServices causes some weird issues with this module so
  this script was created to debug with the errors it creates. They have been resolved
  so this shouldn't be needed anymore, but keeping it here just in case.
#>


$ErrorActionPreference = 'SilentlyContinue'
$null = Import-Module 'c:\Users\Ryan2\.vscode\extensions\ms-vscode.powershell-preview-2020.4.3\modules\PowerShellEditorServices\PowerShellEditorServices.psd1'; Start-EditorServices -HostName 'Visual Studio Code Host'  -AdditionalModules @('PowerShellEditorServices.VSCode') -BundledModulesPath 'c:\Users\Ryan2\.vscode\extensions\ms-vscode.powershell-preview-2020.4.3\modules' -EnableConsoleRepl -StartupBanner "=====> PowerShell Preview Integrated Console v2020.4.3 <=====" -LogLevel 'Normal' -LogPath 'c:\Users\Ryan2\.vscode\extensions\ms-vscode.powershell-preview-2020.4.3\logs\1589298796-bbb3adc4-f7a0-4247-b849-4827ca4a39561589298237173\EditorServices.log' -Stdio -SessionDetailsPath .\ -HostVersion 1.0 -HostProfileId 'PowerShell' -FeatureFlags @()
$ErrorActionPreference = 'Stop'
$null = Add-Type -Path "C:\users\ryan2\OneDrive\Code\EFPosh\src\EFPosh\EFPosh\bin\Debug\net472\EFPosh.dll"

$Script:LatestDBContext = [EFPosh.PoshContextInteractions]::new()
$ConnectionString = "Filename=c:\users\ryan2\sql.sqlite"
Class MyTest {
    [int]$Id
    [string]$test
}
$Type = 'MyTest'
$newEntity = [EFPosh.PoshEntity]::new()
$newEntity.Type = (New-Object -TypeName $Type).GetType()
$DBType = 'SQLITE'
$Entities = @(
    $newEntity
)
$null = $Script:LatestDBContext.NewPoshContext($ConnectionString, $DBType, $Entities, $false, $null)