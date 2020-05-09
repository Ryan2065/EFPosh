
$CommandFiles = Get-ChildItem -Path "$PSScriptRoot\Commands" -Filter '*.ps1'
foreach($file in $CommandFiles){
    Write-Verbose "Importing $($file.FullName)"
    . $file.FullName
}

Export-ModuleMember -Function $CommandFiles.BaseName