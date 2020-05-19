Function Get-EFPoshType{
    Param(
        [string]$TypeName
    )
    $returnType = $null
    try{
        $returnType = (New-Object -TypeName $Type).GetType()
    }
    catch{

    }
    if($null -ne $returnType) { return $returnType }

    # This should find all "PowerShell Class" virtual assemblies. Because of scope, we might
    # not be able to find the provided PowerShell class through normal means, but we can find it through the AppDomain
    $PotentialAssemblies = [AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { $null -eq $_.Location }
    foreach($a in $PotentialAssemblies){
        $ExportedTypes = $a.GetTypes()
        foreach($ExportedType in $ExportedTypes){
            if($ExportedType.Name -eq $TypeName){
                # Setting it this way so the last loaded gets returned
                $returnType = $ExportedType
            }
        }
    }
    return $returnType
}
