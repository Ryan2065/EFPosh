$Location = Get-Location
$ErrorActionPreference = 'Stop'
$null = . "$PSScriptRoot\ClearCompiledFiles.ps1"
Push-Location "$PSScriptRoot\EFPosh\EFPosh"

dotnet restore

dotnet publish --self-contained false --configuration release --framework net472 --no-restore
dotnet publish --self-contained false --configuration release --framework net6.0 --no-restore 

if(Test-Path "$PSScriptRoot\Module\EFPosh\Dependencies"){
    & cmd /c rd "$PSScriptRoot\Module\EFPosh\Dependencies\" /s /q
}

$null = New-Item "$PSScriptRoot\Module\EFPosh\Dependencies" -ItemType Directory -Force
$null = New-Item "$PSScriptRoot\Module\EFPosh\Dependencies\net472" -ItemType Directory -Force
$null = New-Item "$PSScriptRoot\Module\EFPosh\Dependencies\net6.0" -ItemType Directory -Force
Copy-Item -Path ".\bin\Release\net472\publish\*" -Destination "$PSScriptRoot\Module\EFPosh\Dependencies\net472\" -Force -Recurse
Copy-Item -Path ".\bin\Release\net6.0\publish\*" -Destination "$PSScriptRoot\Module\EFPosh\Dependencies\net6.0\" -Force -Recurse
Push-Location $Location

