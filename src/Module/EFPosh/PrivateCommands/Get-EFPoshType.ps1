Function Get-EFPoshType{
    <#
    .SYNOPSIS
    PowerShell classes, if you re-run your class defintion it'll re-create the class type with a same name but from different assembly.
    This attempts to figure out the newest veresion of the class
    
    .DESCRIPTION
    PowerShell classes, if you re-run your class defintion it'll re-create the class type with a same name but from different assembly.
    This attempts to figure out the newest veresion of the class
    
    .PARAMETER TypeName
    The type we have to do this for
    
    .EXAMPLE
    Class MyClass {  }
    Get-EFPoshType -TypeName 'MyClass'
    
    .NOTES
    .Author: Ryan Ephgrave
    #>
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
