$Location = Get-Location
$ErrorActionPreference = 'Stop'
Push-Location "$PSScriptRoot\EFPosh"

dotnet publish --self-contained --configuration release

$Files = ".\EFPosh\bin\Release\net472\publish\*"

if(Test-Path "$PSScriptRoot\Module\bin"){
    & cmd /c rd "$PSScriptRoot\Module\bin\" /s /q
}

$null = New-Item "$PSScriptRoot\Module\bin" -ItemType Directory -Force

Copy-Item -Path $Files -Destination "$PSScriptRoot\Module\bin\" -Force -Recurse

Push-Location $Location

