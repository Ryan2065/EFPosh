Function New-EFPoshQuery {
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory = $true)]
        [EFPosh.PoshContextInteractions]$DBContext,
        [Parameter(Mandatory = $true)]
        [ArgumentCompleter({
            Param($CommandName, $ParameterName, $WordToComplete, $CommandAst, $FakeBoundParameters)
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
            $Script:EFPoshQuery = $Script:EFPoshQuery.FromQuery($FromQuery, $FromQueryParams)
        }
        else{
            $Script:EFPoshQuery = $Script:EFPoshQuery.FromQuery($FromQuery)
        }
    }
}