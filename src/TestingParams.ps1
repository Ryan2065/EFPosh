Function MyTest{
    Param(
        $InputObject,
        [switch]$eq,
        $InputObject2
    )
    Get-PSCallStack
}
<#
Class MyClass{
    [string]$Prop1
    [string]$Prop2
}
#>
$x = [myclass]::new()
$x.Prop1 = 'test'

$cs = MyTest $x.Prop1 -eq $x.Prop2

$cs

$HashExample = @{ 'Val1' = 'a2' }
$ValName = 'Val1'

$HashExample.$valname