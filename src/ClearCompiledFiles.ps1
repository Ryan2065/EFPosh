Push-Location "$PSScriptRoot\EFposh\EFPosh"

if(Test-Path .\bin){
    Get-ChildItem ".\bin" -Recurse -Include '*.*' | ForEach-Object {
        $null = & cmd /c del "$($_.FullName)" /f /s /q
    }
    $null = cmd /c rd bin /S /Q
}

if(Test-Path .\obj){
    Get-ChildItem ".\obj" -Recurse -Include '*.*' | ForEach-Object {
        $null = & cmd /c del "$($_.FullName)" /f /s /q
    }
    $null = cmd /c rd obj /S /Q
}

Push-Location "$PSScriptRoot\EFPosh\EFPosh.InformationSchemaDB"
if(Test-Path .\bin){
    Get-ChildItem ".\bin" -Recurse -Include '*.*' | ForEach-Object {
        $null = & cmd /c del "$($_.FullName)" /f /s /q
    }
    $null = cmd /c rd bin /S /Q
}

if(Test-Path .\obj){
    Get-ChildItem ".\obj" -Recurse -Include '*.*' | ForEach-Object {
        $null = & cmd /c del "$($_.FullName)" /f /s /q
    }
    $null = cmd /c rd obj /S /Q
}

Push-Location "$PSScriptRoot\EFPosh\BinaryExpressionConverter"
if(Test-Path .\bin){
    Get-ChildItem ".\bin" -Recurse -Include '*.*' | ForEach-Object {
        $null = & cmd /c del "$($_.FullName)" /f /s /q
    }
    $null = cmd /c rd bin /S /Q
}

if(Test-Path .\obj){
    Get-ChildItem ".\obj" -Recurse -Include '*.*' | ForEach-Object {
        $null = & cmd /c del "$($_.FullName)" /f /s /q
    }
    $null = cmd /c rd obj /S /Q
}
