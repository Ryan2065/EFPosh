Function New-EFPoshQuery{
    Param(
        [Parameter(Mandatory = $true)]
        [Type]$Type,
        [Parameter(Mandatory=$false)]
        [EFPosh.PoshContextInteractions]$Context
    )
    if($null -eq $Context){
        if($null -eq $Script:LatestDBContext){
            throw 'Context not created - call New-EFPoshContext first'
        }
        else{
            $Context = $Script:LatestDBContext
        }
    }
    
    return $Context.GetType().GetMethod("NewQuery").MakeGenericMethod($Type).Invoke($Context, $null)
}