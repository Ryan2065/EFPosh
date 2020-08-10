$Location = Get-Location
$ErrorActionPreference = 'Stop'
$null = . "$PSScriptRoot\ClearCompiledFiles.ps1"
Push-Location "$PSScriptRoot\EFPosh"

dotnet publish --self-contained --configuration release --framework net472
#dotnet publish --self-contained --configuration release --runtime linux-x64 --framework netstandard2.0
#dotnet publish --self-contained --configuration release --runtime osx-x64 --framework netstandard2.0

if(Test-Path "$PSScriptRoot\Module\EFPosh\Dependencies"){
    & cmd /c rd "$PSScriptRoot\Module\EFPosh\Dependencies\" /s /q
}

$null = New-Item "$PSScriptRoot\Module\EFPosh\Dependencies" -ItemType Directory -Force

Copy-Item -Path ".\EFPosh\bin\Release\net472\publish\*" -Destination "$PSScriptRoot\Module\EFPosh\Dependencies\" -Force -Recurse
Copy-Item -Path ".\BinaryExpressionConverter\bin\Release\net472\publish\*" -Destination "$PSScriptRoot\Module\EFPosh\Dependencies\" -Force -Recurse

Push-Location $Location

