Function Remove-EFPoshEntity{
    <#
    .SYNOPSIS
    Removes an entity from a database
    
    .DESCRIPTION
    Will remove an entity from a database and queue it for removal. Use -SaveChanges switch to commit change immediately
    
    .PARAMETER DbContext
    Optional - DbContext you want to remove the entity from. Will use last created one if not provided
    
    .PARAMETER Entity
    Existing entity we want to remove from the database
    
    .PARAMETER SaveChanges
    Used if we want the changes to be commited immediately. If not called - Save-EFPoshChanges will need to be called manually to save the changes.
    Can also save changes using $DbContext.SaveChanges();
    
    .EXAMPLE
    Remove-EFPoshEntity -DbContext $MyDbContext -Entity $MyEntity
    Save-EFPoshChanges
    
    .NOTES
    .Author: Ryan Ephgrave
    #>
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$false)]
        [object]$DbContext,
        [Parameter(Mandatory=$true)]
        [object]$Entity,
        [switch]$SaveChanges
    )
    if(-not $PSBoundParameters.ContainsKey('DbContext')){
        $DbContext = $Script:LatestDBContext
    }
    if($null -eq $DbContext){
        throw "Null DbContext - Run New-EFPoshContext to get one and provide it."
        return
    }
    if($Entity.Count -gt 1){
        $DbContext.RemoveRange($Entity)
    }
    else{
        $DbContext.Remove($Entity[0])
    }
    if($SaveChanges){
        Save-EFPoshChanges -DbContext $DbContext
    }
}