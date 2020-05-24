Function Add-EFPoshQuery {
    <#
    .SYNOPSIS
    Builds the IQueryable
    
    .DESCRIPTION
    Will build an IQueryable after New-EFPoshQuery is called - Auto complete of Property only works if New-EFPoshQuery is run
    
    .PARAMETER Property
    Name of the property you wish to query against
    
    .PARAMETER Equals
    Builds an Equals expression
    
    .PARAMETER NotEquals
    Builds a NotEquals expression
    
    .PARAMETER Contains
    If an array is passed, it turns into a SQL query of "Property In ( '','','' )" - however, if the object passed in is not an array, it defaults to PropertyName like '%Value%'
    
    .PARAMETER NotContains
    If an array is passed, it turns into a SQL query of "Property Not In ( '','','' )" - however, if the object passed in is not an array, it defaults to PropertyName not like '%Value%'
    
    .PARAMETER StartsWith
    Builds a StartsWith queryable
    
    .PARAMETER EndsWith
    Builds an EndsWith queryable
    
    .PARAMETER GreaterThan
    Builds a GreaterThan queryable
    
    .PARAMETER LessThan
    Builds a LessThan queryable
    
    .PARAMETER GreaterThanOrEqualTo
    Builds a GreaterThanOrEqualTo queryable
    
    .PARAMETER LessThanOrEqualTo
    Builds a LessThanOrEqualTo queryable
    
    .PARAMETER And
    Tells the system another expression will be added as an and
    
    .PARAMETER Or
    Tells the system another expression will be added as an or
    
    .NOTES
    .Author: Ryan Ephgrave
    #>
    Param(
        [Parameter(Mandatory=$true, ParameterSetName = 'Equals')]
        [Parameter(Mandatory=$true, ParameterSetName = 'NotEquals')]
        [Parameter(Mandatory=$true, ParameterSetName = 'Contains')]
        [Parameter(Mandatory=$true, ParameterSetName = 'NotContains')]
        [Parameter(Mandatory=$true, ParameterSetName = 'StartsWith')]
        [Parameter(Mandatory=$true, ParameterSetName = 'EndsWith')]
        [Parameter(Mandatory=$true, ParameterSetName = 'LessThan')]
        [Parameter(Mandatory=$true, ParameterSetName = 'GreaterThan')]
        [Parameter(Mandatory=$true, ParameterSetName = 'GreaterThanOrEqualTo')]
        [Parameter(Mandatory=$true, ParameterSetName = 'LessThanOrEqualTo')]
        [ArgumentCompleter({
            Param($CommandName, $ParameterName, $WordToComplete, $CommandAst, $FakeBoundParameters)
            $PoshQuery = ( Get-Module EFPosh ).Invoke( { $Script:EFPoshQuery } )
            $PropertyList = $PoshQuery.GetProperties()
            foreach($instance in $PropertyList){
                if($instance -like "$($WordToComplete)*"){
                    New-Object System.Management.Automation.CompletionResult (
                        $instance,
                        $instance,
                        'ParameterValue',
                        $instance
                    )
                }
            }
        })]
        [string]$Property,
        [Parameter(Mandatory=$true, ParameterSetName = 'Equals')]
        [object]$Equals,
        [Parameter(Mandatory=$true, ParameterSetName = 'NotEquals')]
        [object]$NotEquals,
        [Parameter(Mandatory=$true, ParameterSetName = 'Contains')]
        [object]$Contains,
        [Parameter(Mandatory=$true, ParameterSetName = 'NotContains')]
        [object]$NotContains,
        [Parameter(Mandatory=$true, ParameterSetName = 'StartsWith')]
        [object]$StartsWith,
        [Parameter(Mandatory=$true, ParameterSetName = 'EndsWith')]
        [object]$EndsWith,
        [Parameter(Mandatory=$true, ParameterSetName = 'GreaterThan')]
        [object]$GreaterThan,
        [Parameter(Mandatory=$true, ParameterSetName = 'LessThan')]
        [object]$LessThan,
        [Parameter(Mandatory=$true, ParameterSetName = 'GreaterThanOrEqualTo')]
        [object]$GreaterThanOrEqualTo,
        [Parameter(Mandatory=$true, ParameterSetName = 'LessThanOrEqualTo')]
        [object]$LessThanOrEqualTo,
        [Parameter(Mandatory=$false, ParameterSetName = 'Equals')]
        [Parameter(Mandatory=$false, ParameterSetName = 'NotEquals')]
        [Parameter(Mandatory=$false, ParameterSetName = 'Contains')]
        [Parameter(Mandatory=$false, ParameterSetName = 'NotContains')]
        [Parameter(Mandatory=$false, ParameterSetName = 'StartsWith')]
        [Parameter(Mandatory=$false, ParameterSetName = 'EndsWith')]
        [Parameter(Mandatory=$false, ParameterSetName = 'LessThan')]
        [Parameter(Mandatory=$false, ParameterSetName = 'GreaterThan')]
        [Parameter(Mandatory=$false, ParameterSetName = 'GreaterThanOrEqualTo')]
        [Parameter(Mandatory=$false, ParameterSetName = 'LessThanOrEqualTo')]
        [switch]$And,
        [Parameter(Mandatory=$false, ParameterSetName = 'Equals')]
        [Parameter(Mandatory=$false, ParameterSetName = 'NotEquals')]
        [Parameter(Mandatory=$false, ParameterSetName = 'Contains')]
        [Parameter(Mandatory=$false, ParameterSetName = 'NotContains')]
        [Parameter(Mandatory=$false, ParameterSetName = 'StartsWith')]
        [Parameter(Mandatory=$false, ParameterSetName = 'EndsWith')]
        [Parameter(Mandatory=$false, ParameterSetName = 'LessThan')]
        [Parameter(Mandatory=$false, ParameterSetName = 'GreaterThan')]
        [Parameter(Mandatory=$false, ParameterSetName = 'GreaterThanOrEqualTo')]
        [Parameter(Mandatory=$false, ParameterSetName = 'LessThanOrEqualTo')]
        [switch]$Or
    )
    if($null -eq $Script:EFPoshQuery){
        throw 'Please run New-EFPoshQuery first and select which Entity we are querying against'
        return
    }
    $ComparisionValue = $null
    $BoolSetComparision = $false
    foreach($var in $PSBoundParameters.Keys){
        if($var -ne 'Or' -and $var -ne 'And' -and $var -ne 'Property'){
            $ComparisionValue = $PSBoundParameters[$var]
            $BoolSetComparision = $true
        }
    }
    if($BoolSetComparision){
        $Script:EFPoshQuery = $Script:EFPoshQuery."$($Property)"."$($PSCmdlet.ParameterSetName)"($ComparisionValue)
    }
    else{
        throw "This should never be hit - if you see this error, the cool thing I did didn't work - Create an issue on GitHub with this error"
    }
    if($Or) {
        $Script:EFPoshQuery = $Script:EFPoshQuery.Or
    }
    if($And){
        $Script:EFPoshQuery = $Script:EFPoshQuery.And
    }
}