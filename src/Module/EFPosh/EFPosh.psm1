if($PSVersionTable.PSVersion.Major -gt 5){
    $null = Add-Type -Path "$PSScriptRoot\Dependencies\net6.0\EFPosh.dll"
    Import-Module "$PSScriptRoot\Dependencies\net6.0\BinaryExpressionConverter.dll" -Force
}
else{
    $null = Add-Type -Path "$PSScriptRoot\Dependencies\net472\EFPosh.dll"
    $null = Add-Type -Path "$PSScriptRoot\Dependencies\net472\System.ComponentModel.Annotations.dll"
    Import-Module "$PSScriptRoot\Dependencies\net472\BinaryExpressionConverter.dll" -Force
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