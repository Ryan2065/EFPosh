Function Get-EFPoshVariableValue{
    Param(
        [string]$base64Expression
    )
    $Expression = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($base64Expression))
    $returnObject = [BinaryExpressionConverter.PoshBinaryConverterObject]::new()
    $returnObject.Value = Invoke-Expression $Expression
    return $returnObject
}