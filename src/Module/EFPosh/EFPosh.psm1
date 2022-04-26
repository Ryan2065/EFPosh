if(-not [string]::IsNullOrEmpty($env:EFPoshDependencyFolder) -and 
    ( Test-Path "$($env:EFPoshDependencyFolder)\EFPosh.dll" -ErrorAction SilentlyContinue ))
{
    Import-Module "$PSScriptRoot\Dependencies\EFPosh\net6.0\EFPosh.dll" -Force
    $assembly = [System.Reflection.Assembly]::Load("EFPosh.Shared")
    Add-Type -Path $assembly.Location
    $assembly = [System.Reflection.Assembly]::Load("EFPosh.EFInteractions")
    Add-Type -Path $assembly.Location
}
elseif($PSVersionTable.PSVersion.Major -gt 5){
    Import-Module "$PSScriptRoot\Dependencies\EFPosh\net6.0\EFPosh.dll" -Force
    $assembly = [System.Reflection.Assembly]::Load("EFPosh.Shared")
    Add-Type -Path $assembly.Location
}
else{
    Import-Module "$PSScriptRoot\Dependencies\EFPosh\netstandard2.0\EFPosh.dll" -Force
    $null = Add-Type -Path "$PSScriptRoot\Dependencies\EFPosh\netstandard2.0\EFPosh.Shared.dll"
}




<#
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
}#>

$CommandFiles = Get-ChildItem -Path "$PSScriptRoot\Commands" -Filter '*.ps1'
foreach($file in $CommandFiles){
    . $file.FullName
}

$PrivateCommandFiles = Get-ChildItem -Path "$PSScriptRoot\PrivateCommands" -Filter '*.ps1'
foreach($file in $PrivateCommandFiles){
    . $file.FullName
}

Export-ModuleMember -Function $CommandFiles.BaseName -Cmdlet 'ConvertTo-BinaryExpression','New-EFPoshContext'