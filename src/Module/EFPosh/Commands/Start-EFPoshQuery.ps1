Function Start-EFPoshQuery{
    <#
    .SYNOPSIS
    Runs the query built with New-EFPoshQuery and Add-EFPoshQuery
    
    .DESCRIPTION
    Runs the query
    
    .PARAMETER AsNoTracking
    If you don't plan to edit the results and the DBContext is not read-only, use this
    
    .PARAMETER Include
    Any tables you want included with the results (will add joins to the query) - relationships have to be built before this can be used
    
    .PARAMETER Take
    How many results do you want? Defaults to all results
    
    .PARAMETER Skip
    How many results do you want to skip? Defaults to 0
    
    .PARAMETER OrderBy
    Order by what column?
    
    .PARAMETER OrderByDescending
    Order by what column descending?
    
    .PARAMETER Distinct
    Do you only want distinct results?
    
    .PARAMETER ToList
    Will return all results in a System.Collections.Generic.List 
    
    .PARAMETER FirstOrDefault
    Will return the first result or null
    
    .PARAMETER Any
    Will return true or false if there are results
    
    .NOTES
    .Author: Ryan Ephgrave
    #>
    [CmdletBinding(DefaultParameterSetName='ToList')]
    Param(
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
    if($null -eq $Script:EFPoshQuery){
        throw 'Please run New-EFPoshQuery first and select which Entity we are querying against'
        return
    }
    if($PSCmdlet.ParameterSetName -eq 'ToList'){
        $ToList = $true
    }
    if($Include){
        foreach($instance in $Include){
            if(-not ( [string]::IsNullOrEmpty($instance ))){
                $Script:EFPoshQuery = $Script:EFPoshQuery.Include($instance)
            }
        }
    }
    if($AsNoTracking){
        $Script:EFPoshQuery = $Script:EFPoshQuery.AsNoTracking()
    }
    if($Take){
        $Script:EFPoshQuery = $Script:EFPoshQuery.Take($Take)
    }
    if($Skip){
        $Script:EFPoshQuery = $Script:EFPoshQuery.Skip($Skip)
    }
    if($OrderBy){
        $Script:EFPoshQuery = $Script:EFPoshQuery.OrderBy($OrderBy)
    }
    if($OrderByDescending){
        $Script:EFPoshQuery = $Script:EFPoshQuery.OrderBy("$OrderByDescending descending")
    }
    if($Distinct){
        $Script:EFPoshQuery = $Script:EFPoshQuery.Distinct()
    }
    if($Select){
        $Script:EFPoshQuery = $Script:EFPoshQuery.Select($Select)
    }
    $tempQuery = $Script:EFPoshQuery
    $Script:EFPoshQuery = $null
    if($ToList){
        return $tempQuery.ToList()
    }
    if($FirstOrDefault){
        return $tempQuery.FirstOrDefault()
    }
    if($Any){
        return $tempQuery.Any()
    }
}