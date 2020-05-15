$Location = Get-Location
$ErrorActionPreference = 'Stop'
Push-Location "$PSScriptRoot\EFPosh"

dotnet publish --self-contained --configuration release --framework net472
#dotnet publish --self-contained --configuration release --runtime linux-x64 --framework netstandard2.0
#dotnet publish --self-contained --configuration release --runtime osx-x64 --framework netstandard2.0

$Files = ".\EFPosh\bin\Release\net472\publish\*"

if(Test-Path "$PSScriptRoot\Module\EFPosh\bin"){
    & cmd /c rd "$PSScriptRoot\Module\EFPosh\bin\" /s /q
}

$null = New-Item "$PSScriptRoot\Module\EFPosh\bin" -ItemType Directory -Force

Copy-Item -Path $Files -Destination "$PSScriptRoot\Module\EFPosh\bin\" -Force -Recurse

Push-Location $Location

