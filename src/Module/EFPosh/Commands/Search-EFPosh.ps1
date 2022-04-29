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
    
    .PARAMETER FromSql
    Uses a SQL query as the base query instead of the entity

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
        [Parameter(Mandatory=$false, ParameterSetName = 'ToList')]
        [Parameter(Mandatory=$false, ParameterSetName = 'FirstOrDefault')]
        [Parameter(Mandatory=$false, ParameterSetName = 'Any')]
        [object]$DbContext,
        [Parameter(Mandatory=$true, ParameterSetName = 'ToList')]
        [Parameter(Mandatory=$true, ParameterSetName = 'FirstOrDefault')]
        [Parameter(Mandatory=$true, ParameterSetName = 'Any')]
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
        [string]$Entity,
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
        [Parameter(Mandatory=$false, ParameterSetName = 'FirstOrDefault')]
        [Parameter(Mandatory=$false, ParameterSetName = 'Any')]
        [string]$FromSql,
        [Parameter(Mandatory=$false, ParameterSetName = 'ToList')]
        [switch]$ToList,
        [Parameter(Mandatory=$true, ParameterSetName = 'FirstOrDefault')]
        [switch]$FirstOrDefault,
        [Parameter(Mandatory=$true, ParameterSetName = 'Any')]
        [switch]$Any
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
    $EntityObj = $DbContext."$($Entity)"

    if(-not [string]::IsNullOrEmpty($FromSql)){
        $EntityObj.FromSql($FromSql)
    }

    if($Expression){
        try{
            #I'd love to break this out to it's own function, but below the PsCmdlet session state stuff needs to be run in here
            #so it has to stay in the function called by the user.
            $ExpressionValues = New-Object 'System.Collections.Generic.Dictionary[[string],[object]]'
            $VariableValues = @{}
            $varAsts = $Expression.Ast.FindAll({
                param( [System.Management.Automation.Language.Ast] $AstObject )
        
            return ( $AstObject -is [System.Management.Automation.Language.VariableExpressionAst] )
            }, $true)
            foreach($varAst in $varAsts){
                try{
                    $ExpressionBase = Get-EFPoshExpressionBase -Ast $varAst
                    #Why aren't we just getting the variable value?
                    #To account for weird instances, like:  $var.PropertyName
                    #or $var."$propIwant"
                    if(-not [string]::IsNullOrEmpty($ExpressionBase)){
                        $VariableValues[$varAst.VariablePath.UserPath] = $PSCmdlet.SessionState.PSVariable.GetValue($varAst.VariablePath.UserPath)
                        $VariableValue = Invoke-Command -ScriptBlock {
                            Param([hashtable]$VariableValues, $ExpressionBase)
                            foreach($key in $VariableValues.Keys){
                                Set-Variable -Name $key -Value $VariableValues[$key]
                            }
                            return (Invoke-Expression $ExpressionBase)
                        } -ArgumentList @($VariableValues, $ExpressionBase) -ErrorAction SilentlyContinue
                        if($null -ne $VariableValue){
                            $ExpressionValues[$ExpressionBase.ToString()] = $VariableValue
                        }
                    }
                }
                catch{}
            }
            $EntityObj.ApplyExpression($Expression, $Arguments, $ExpressionValues)
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
                $EntityObj.Include($instance, $thenInstance)
            }
            $includeCount++
        }
    }
    if($AsNoTracking){
        $EntityObj.AsNoTracking()
    }
    if($Take){
        $EntityObj.Take($Take)
    }
    if($Skip){
        $EntityObj.Skip($Skip)
    }
    if($OrderBy){
        $EntityObj.OrderBy($OrderBy)
    }
    if($OrderByDescending){
        $EntityObj.OrderBy($OrderByDescending)
    }
    if($Distinct){
        $EntityObj.Distinct()
    }
    if($Select){
        $EntityObj.Select($Select)
    }
    if($ToList){
        return $EntityObj.ToList()
    }
    if($FirstOrDefault){
        return $EntityObj.FirstOrDefault()
    }
    if($Any){
        return $EntityObj.Any()
    }
}