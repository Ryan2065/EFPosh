Import-Module Pester
$VerbosePreference = 'SilentlyContinue'
$ErrorActionPreference = 'Continue'

Write-Verbose 'Building module with newest version'
$ParentDirectory = ( Get-Item $PSScriptRoot ).Parent.FullName
. "$ParentDirectory\buildModule.ps1"

Import-Module "$ParentDirectory\Module\EFPosh" -Force
try{
    $Global:TestSettings = Get-Content "$PSScriptRoot\bin\TestSettings.json" | ConvertFrom-JSON
}
catch {
    throw 'Error getting settings - Please run Set_Settings.ps1 with the parameters'
}

Write-Output 'Cleaning up from old tests'
. "$PSScriptRoot\RemoveDbs.ps1"

Write-Output 'Starting tests...'
Invoke-Pester -Path "$PSScriptRoot\1_CreateDbs.tests.ps1"
Invoke-Pester -Path "$PSScriptRoot\2_AddData.tests.ps1"
Invoke-Pester -Path "$PSScriptRoot\3_QueryData.tests.ps1"
Invoke-Pester -Path "$PSScriptRoot\4_NewQueryData.tests.ps1"

Write-Output 'Cleaning up'
. "$PSScriptRoot\RemoveDbs.ps1"

$VerbosePreference = 'SilentlyContinue'