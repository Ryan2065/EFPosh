Function Get-EFPoshExpressionBase {
    <#
    .SYNOPSIS
    Will take a PowerShell variable expression and expand it out.
    
    .DESCRIPTION
    Takes a variable expression (lowest level) and expands it out until it runs into something it shouldn't expand - like $_
    
    .PARAMETER Ast
    Expression we are expanding
    
    .EXAMPLE
    $Expression = { $_.Name -eq $test }.Ast
    $vexp = Get-VariableExpressionAst $Expression # not shown - will return the AST obj for $test
    Get-EFPoshExpressionBase -Ast $vexp
    # expands out $test to { $_.Name -eq $test} - it will see the new one has $_, which we don't want, and just return $test
    
    .NOTES
    .Author: Ryan Ephgrave
    #>
    Param(
        [System.Management.Automation.Language.Ast]$Ast
    )
    # $_ is special and cannot be evaluated
    if($Ast.ToString().Contains('$_')){ return }
    if($null -eq $Ast.Parent -or $Ast.Parent.ToString().Contains('$_')){ return $ast.ToString() }

    Get-EFPoshExpressionBase -Ast $Ast.Parent
}