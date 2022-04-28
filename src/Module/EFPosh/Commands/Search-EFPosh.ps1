Function Search-EFPosh{
    <#
    .SYNOPSIS
    Takes a PowerShell expression, converts to BinaryExpression, and then runs it against SQL
    
    .DESCRIPTION
    Will convert a PowerShell expression block to the Linq equivalent and then run it
    
    .PARAMETER Entity
    Entity from the DBContext - Has to have been created with New-EFPoshContext
    
    .PARAMETER Expression
    Expression to run against the entity

    .PARAMETER Arguments
    Arguments for the expression when $0 is used
    
    .PARAMETER AsNoTracking
    Should tracking be set up? If you don't want to edit the results, this could significantly reduce the query time
    
    .PARAMETER Include
    Should any navigation properties be included? 
    
    .PARAMETER Take
    How many results should be returned
    
    .PARAMETER Skip
    Do we skip any results
    
    .PARAMETER OrderBy
    Order by what property
    
    .PARAMETER OrderByDescending
    Order by what property descending
    
    .PARAMETER Distinct
    Should results be distinct
    
    .PARAMETER Select
    Which properties do we return - default is all
    
    .PARAMETER ToList
    Will return results in a List<T>
    
    .PARAMETER FirstOrDefault
    Will return only the first result or null if none are found
    
    .PARAMETER Any
    Will return bool true if results are found, false if nothing is found.
    
    .EXAMPLE
    Search-EFPosh -Entity $Context.TableOne -Expression { $_.Name -eq 'Test' }

    .EXAMPLE
    Search-EFPosh -Entity $Context.TableOne -Expression { $_.Name -contains $0 } -Arguments @($Example)
    
    .NOTES
    .Author: Ryan Ephgrave
    #>
    [CmdletBinding(DefaultParameterSetName="ToList")]
    Param(
        [Parameter(Mandatory=$true, ParameterSetName = 'ToList')]
        [Parameter(Mandatory=$true, ParameterSetName = 'FirstOrDefault')]
        [Parameter(Mandatory=$true, ParameterSetName = 'Any')]
        [object]$Entity,
        [Parameter(Mandatory=$false, ParameterSetName = 'ToList')]
        [Parameter(Mandatory=$false, ParameterSetName = 'FirstOrDefault')]
        [Parameter(Mandatory=$false, ParameterSetName = 'Any')]
        [ScriptBlock]$Expression,
        [Parameter(Mandatory=$false, ParameterSetName = 'ToList')]
        [Parameter(Mandatory=$false, ParameterSetName = 'FirstOrDefault')]
        [Parameter(Mandatory=$false, ParameterSetName = 'Any')]
        [object[]]$Arguments,
        [Parameter(Mandatory=$false, ParameterSetName = 'ToList')]
        [Parameter(Mandatory=$false, ParameterSetName = 'FirstOrDefault')]
        [Parameter(Mandatory=$false, ParameterSetName = 'Any')]
        [switch]$AsNoTracking,
        [Parameter(Mandatory=$false, ParameterSetName = 'ToList')]
        [Parameter(Mandatory=$false, ParameterSetName = 'FirstOrDefault')]
        [Parameter(Mandatory=$false, ParameterSetName = 'Any')]
        [string[]]$Include,
        [Parameter(Mandatory=$false, ParameterSetName = 'ToList')]
        [Parameter(Mandatory=$false, ParameterSetName = 'FirstOrDefault')]
        [Parameter(Mandatory=$false, ParameterSetName = 'Any')]
        [string[]]$ThenInclude,
        [Parameter(Mandatory=$false, ParameterSetName = 'ToList')]
        [Parameter(Mandatory=$false, ParameterSetName = 'FirstOrDefault')]
        [Parameter(Mandatory=$false, ParameterSetName = 'Any')]
        [int]$Take,
        [Parameter(Mandatory=$false, ParameterSetName = 'ToList')]
        [Parameter(Mandatory=$false, ParameterSetName = 'FirstOrDefault')]
        [Parameter(Mandatory=$false, ParameterSetName = 'Any')]
        [int]$Skip,
        [Parameter(Mandatory=$false, ParameterSetName = 'ToList')]
        [Parameter(Mandatory=$false, ParameterSetName = 'FirstOrDefault')]
        [Parameter(Mandatory=$false, ParameterSetName = 'Any')]
        [string]$OrderBy,
        [Parameter(Mandatory=$false, ParameterSetName = 'ToList')]
        [Parameter(Mandatory=$false, ParameterSetName = 'FirstOrDefault')]
        [Parameter(Mandatory=$false, ParameterSetName = 'Any')]
        [string]$OrderByDescending,
        [Parameter(Mandatory=$false, ParameterSetName = 'ToList')]
        [Parameter(Mandatory=$false, ParameterSetName = 'FirstOrDefault')]
        [Parameter(Mandatory=$false, ParameterSetName = 'Any')]
        [switch]$Distinct,
        [Parameter(Mandatory=$false, ParameterSetName = 'ToList')]
        [Parameter(Mandatory=$false, ParameterSetName = 'FirstOrDefault')]
        [Parameter(Mandatory=$false, ParameterSetName = 'Any')]
        [string[]]$Select,
        [Parameter(Mandatory=$false, ParameterSetName = 'ToList')]
        [switch]$ToList,
        [Parameter(Mandatory=$true, ParameterSetName = 'FirstOrDefault')]
        [switch]$FirstOrDefault,
        [Parameter(Mandatory=$true, ParameterSetName = 'Any')]
        [switch]$Any
    )
    if($Expression){
        try{
            $Entity.ApplyExpression($Expression, $Arguments)
        }
        catch{
            throw
            return
        }
    }
    if($PSCmdlet.ParameterSetName -eq 'ToList'){
        $ToList = $true
    }
    if($Include){
        $includeCount = 0
        foreach($instance in $Include){
            $thenInstance = $null
            if($ThenInclude){
                $thenInstance = @($ThenInclude)[$includeCount]
            }
            if(-not ( [string]::IsNullOrEmpty($instance))){
                $Entity.Include($instance, $thenInstance)
            }
            $includeCount++
        }
    }
    if($AsNoTracking){
        $Entity.AsNoTracking()
    }
    if($Take){
        $Entity.Take($Take)
    }
    if($Skip){
        $Entity.Skip($Skip)
    }
    if($OrderBy){
        $Entity.OrderBy($OrderBy)
    }
    if($OrderByDescending){
        $Entity.OrderBy($OrderByDescending)
    }
    if($Distinct){
        $Entity.Distinct()
    }
    if($Select){
        $Entity.Select($Select)
    }
    if($ToList){
        return $Entity.ToList()
    }
    if($FirstOrDefault){
        return $Entity.FirstOrDefault()
    }
    if($Any){
        return $Entity.Any()
    }
}