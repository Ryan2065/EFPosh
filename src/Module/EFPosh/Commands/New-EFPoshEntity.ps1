Function New-EFPoshEntity {
    <#
    .SYNOPSIS
    Creates a new object of a DbContext entity
    
    .DESCRIPTION
    Creates a new object of a DbContext entity. This is useful to insert new data into a Db. Create a new object, fill it with data, then call Add-EFPoshEntity
    
    .PARAMETER DbContext
    Optional parameter - the DbContext you want to create a new entity for. Last one created with New-EFPoshContext will be used if not specified
    
    .PARAMETER Entity
    Name of the entity (SQL Table) you want to create
    
    .EXAMPLE
    $Entity = New-EFPoshEntity -Entity 'TableName'
    Add-EFPoshEntity -Entity $Entity -SaveChanges
    
    .NOTES
    .Author: Ryan Ephgrave
    #>
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$false)]
        [object]$DbContext,
        [ArgumentCompleter({
            param ( $commandName,$parameterName,$wordToComplete,$commandAst,$fakeBoundParameters)
            if($wordToComplete -like '$*'){
                return
            }
            if($fakeBoundParameters.ContainsKey('DbContext')){
                $EntityNames = $DbContext.GetEntities()
            }
            else{
                $LatestContext = (Get-Module EFPosh).Invoke({ $Script:LatestDBContext })
                if($null -eq $LatestContext) { return }
                $EntityNames = $LatestContext.GetEntities()
            }
            foreach($ename in $EntityNames){
                if($ename -like "*$($wordToComplete)*"){
                    $eName
                }
            }
        })]
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]$Entity
    )
    if(-not $PSBoundParameters.ContainsKey('DbContext')){
        $DbContext = $Script:LatestDBContext
    }
    if($null -eq $DbContext){
        throw "Null DbContext - Run New-EFPoshContext to get one and provide it."
        return
    }
    if($DbContext.GetEntities() -notcontains $Entity){
        throw "Entity not found in DbContext - If no DbContext was specified, the last created one was used."
        return
    }
    return (($DbContext."$($Entity)").New())
}