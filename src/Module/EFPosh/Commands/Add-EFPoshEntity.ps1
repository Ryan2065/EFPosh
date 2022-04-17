Function Add-EFPoshEntity{
    <#
    .SYNOPSIS
    Adds an entity to a database
    
    .DESCRIPTION
    Will add an entity to a database and queue it for inserting. Use -SaveChanges switch to commit change immediately
    
    .PARAMETER DbContext
    Optional - DbContext you want to add the entity to. Will use last created one if not provided
    
    .PARAMETER Entity
    Created entity we want to add to the database
    
    .PARAMETER SaveChanges
    Used if we want the changes to be commited immediately. If not called - Save-EFPoshChanges will need to be called manually to save the changes.
    Can also save changes using $DbContext.SaveChanges();
    
    .EXAMPLE
    Add-EFPoshEntity -DbContext $MyDbContext -Entity $MyEntity
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
    $DbContext.Add($Entity)
    if($SaveChanges){
        Save-EFPoshChanges -DbContext $DbContext
    }
}