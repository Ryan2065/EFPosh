$PrivateCommandFiles = Get-ChildItem -Path "$PSScriptRoot\PrivateCommands" -Filter '*.ps1'
foreach($file in $PrivateCommandFiles){
    . $file.FullName
}

$CommandFiles = Get-ChildItem -Path "$PSScriptRoot\Commands" -Filter '*.ps1'
foreach($file in $CommandFiles){
    Write-Host "Importing $($file.FullName)"
    . $file.FullName
}

Export-ModuleMember -Function $CommandFiles.BaseName