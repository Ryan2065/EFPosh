Function New-EFPoshQuery {
    <#
    .SYNOPSIS
    The starting process of building an Iqueryable - To be used with Add-EFPoshQuery and Start-EFPoshQuery
    
    .DESCRIPTION
    The starting process of building an Iqueryable - To be used with Add-EFPoshQuery and Start-EFPoshQuery
    
    .PARAMETER DBContext
    DBContext you want to run a query against
    
    .PARAMETER Entity
    Entity you are querying (what table/view do you want to query)
    
    .PARAMETER FromQuery
    If the DBContext is MSSql - you can bring your own SQL query
    
    .PARAMETER FromQueryParams
    If your query has parameters, supply them
    
    .NOTES
    .Author: Ryan Ephgrave
    #>
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory = $true)]
        [EFPosh.PoshContextInteractions]$DBContext,
        [Parameter(Mandatory = $true)]
        [ArgumentCompleter({
            Param($CommandName, $ParameterName, $WordToComplete, $CommandAst, $FakeBoundParameters)
            foreach($key in $FakeBoundParameters.Keys){
                if($key -eq 'DBContext'){
                    $EntityList = $FakeBoundParameters.DBContext.GetEntities()
                    foreach($instance in $EntityList){
                        if($instance -like "$($WordToComplete)*"){
                            New-Object System.Management.Automation.CompletionResult (
                                $instance,
                                $instance,
                                'ParameterValue',
                                $instance
                            )
                        }
                    }
                }
            }
        })]
        [string]$Entity,
        [Parameter(Mandatory = $false)]
        [string]$FromQuery,
        [object[]]$FromQueryParams
    )
    $Script:EFPoshQuery = $null
    $Script:EFPoshQuery = $DBContext."$Entity"
    if($FromQuery){
        if($FromQueryParams){
            $Script:EFPoshQuery = $Script:EFPoshQuery.FromSql($FromQuery, $FromQueryParams)
        }
        else{
            $Script:EFPoshQuery = $Script:EFPoshQuery.FromSql($FromQuery)
        }
    }
}