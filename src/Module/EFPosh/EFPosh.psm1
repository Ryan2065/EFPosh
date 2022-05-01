if(-not [string]::IsNullOrEmpty($env:EFPoshDependencyFolder) -and 
    ( Test-Path "$($env:EFPoshDependencyFolder)\EFPosh.dll" -ErrorAction SilentlyContinue ))
{
    $null = Add-Type -Path "$($env:EFPoshDependencyFolder)\EFPosh.dll" -ErrorAction SilentlyContinue
    try{
        [EFPosh.AssemblyResolvers]::LoadSqlClient()
    }
    catch{}
    
}
elseif($PSVersionTable.PSVersion.Major -gt 5){
    
    $null = Add-Type -Path "$PSScriptRoot\Dependencies\net6.0\EFPosh.dll" -ErrorAction SilentlyContinue
    try{
        [EFPosh.AssemblyResolvers]::LoadSqlClient()
    }
    catch{}
}
else{
    $null = Add-Type -Path "$PSScriptRoot\Dependencies\net472\EFPosh.dll" -ErrorAction SilentlyContinue
    $null = Add-Type -Path "$PSScriptRoot\Dependencies\net472\System.ComponentModel.Annotations.dll" -ErrorAction SilentlyContinue
}

$CommandFiles = Get-ChildItem -Path "$PSScriptRoot\Commands" -Filter '*.ps1'
foreach($file in $CommandFiles){
    . $file.FullName
}

$PrivateCommandFiles = Get-ChildItem -Path "$PSScriptRoot\PrivateCommands" -Filter '*.ps1'
foreach($file in $PrivateCommandFiles){
    . $file.FullName
}

Export-ModuleMember -Function $CommandFiles.BaseName 