if(-not [string]::IsNullOrEmpty($env:EFPoshDependencyFolder) -and 
    ( Test-Path "$($env:EFPoshDependencyFolder)\EFPosh.dll" -ErrorAction SilentlyContinue ) -and 
    ( Test-Path "$($env:EFPoshDependencyFolder)\BinaryExpressionConverter.dll" -ErrorAction SilentlyContinue ))
{
    $DependencyFolder = $env:EFPoshDependencyFolder
    $null = Add-Type -Path "$($env:EFPoshDependencyFolder)\EFPosh.dll"
    Import-Module "$($env:EFPoshDependencyFolder)\BinaryExpressionConverter.dll" -Force
    if($PSVersionTable.PSVersion.Major -le 5){
        $null = Add-Type -Path "$($env:EFPoshDependencyFolder)\System.ComponentModel.Annotations.dll"
    }
}
elseif($PSVersionTable.PSVersion.Major -gt 5){
    $DependencyFolder = "$PSScriptRoot\Dependencies\net6.0"
}
else{
    $DependencyFolder = "$PSScriptRoot\Dependencies\net472"
}

Import-Module "$DependencyFolder\BinaryExpressionConverter.dll" -Force

if($PSVersionTable.PSVersion.Major -gt 5){
    write-host "loading resolver"
    write-host "dep folder: $($DependencyFolder)"
    $null = Add-Type -Path "C:\Users\Ryan2\OneDrive\Code\EFPosh\src\Module\EFPosh\Dependencies\ALC\EFPosh.ALC.dll"
    [EFPosh.ALC.LoadContextResolver]::LoadResolver($DependencyFolder)
    Add-Type -AssemblyName 'EFPosh' -PassThru
    #$null = Add-Type -Path "$DependencyFolder\EFPosh.dll"
    #$null = Add-Type -Path "$DependencyFolder\Microsoft.Data.Sqlite.dll"
}
else{
    $null = Add-Type -Path "$DependencyFolder\EFPosh.dll"
    $null = Add-Type -Path "$DependencyFolder\System.ComponentModel.Annotations.dll"
}

$CommandFiles = Get-ChildItem -Path "$PSScriptRoot\Commands" -Filter '*.ps1'
foreach($file in $CommandFiles){
    . $file.FullName
}

$PrivateCommandFiles = Get-ChildItem -Path "$PSScriptRoot\PrivateCommands" -Filter '*.ps1'
foreach($file in $PrivateCommandFiles){
    . $file.FullName
}

Export-ModuleMember -Function $CommandFiles.BaseName -Cmdlet 'ConvertTo-BinaryExpression'