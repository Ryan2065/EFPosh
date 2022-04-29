Function Save-EFPoshChanges{
    <#
    .SYNOPSIS
    Saves any queued changes in the DbContext
    
    .DESCRIPTION
    DbContexts track changes made to entities and will queue them up. Calling SaveChanges will push them to the DB
    
    .PARAMETER DbContext
    DbContext you want SaveChanges to be run against. If not provided, will use the last DbContext created
    
    .EXAMPLE
    Save-EFPoshChanges
    
    .NOTES
    .Author: Ryan Ephgrave
    #>
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$false)]
        [object]$DbContext
    )
    if(-not $PSBoundParameters.ContainsKey('DbContext')){
        $DbContext = $Script:LatestDBContext
    }
    if($null -eq $DbContext){
        throw "Null DbContext - Run New-EFPoshContext to get one and provide it."
        return
    }
    $DbContext.SaveChanges()
}