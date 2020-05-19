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
        [string]$Entity
    )
    return $DBContext."$Entity"
}