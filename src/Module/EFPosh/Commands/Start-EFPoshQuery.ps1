Function Start-EFPoshQuery{
    [CmdletBinding(DefaultParameterSetName='ToList')]
    Param(
        [Parameter(Mandatory=$false, ParameterSetName = 'ToList')]
        [Parameter(Mandatory=$false, ParameterSetName = 'FirstOrDefault')]
        [Parameter(Mandatory=$false, ParameterSetName = 'Any')]
        [switch]$AsNoTracking,
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
        [Parameter(Mandatory=$true, ParameterSetName = 'ToList')]
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