
$CommandFiles = Get-ChildItem -Path "$PSScriptRoot\Commands" -Filter '*.ps1'
foreach($file in $CommandFiles){
    Write-Verbose "Importing $($file.FullName)"
    . $file.FullName
}

$PrivateCommandFiles = Get-ChildItem -Path "$PSScriptRoot\PrivateCommands" -Filter '*.ps1'
foreach($file in $PrivateCommandFiles){
    Write-Verbose "Importing $($file.FullName)"
    . $file.FullName
}

Export-ModuleMember -Function $CommandFiles.BaseName