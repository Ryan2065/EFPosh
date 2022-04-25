$env:EFPoshDependencyFolder = Get-Location

Import-Module ([System.IO.Path]::Combine((Get-Item $PSScriptRoot).Parent.Parent.FullName, 'Module', 'EFPosh')) -Force

if(Test-Path 'EAMonitor.sqlite' -ErrorAction SilentlyContinue){
    $null =Remove-Item 'EAMonitor.Sqlite' -Force
}

Import-Module "C:\Users\Ryan2\OneDrive\Code\EAMonitor\src\EAMonitor" -Force

Initialize-EAMonitor -SqliteFilePath 'EAMonitor.Sqlite' -CreateDb

Import-EAMonitor -Path 'C:\Users\Ryan2\OneDrive\Code\EAMonitor\src\FakeMonitor.tests.ps1'

Set-EAMonitorSetting -SettingKey 'TestKey' -Value 'MyValue2'
Get-EAMonitorSetting -MonitorName 'FakeMonitor'

