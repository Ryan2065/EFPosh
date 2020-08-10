Class MyType {
    [string]$Name
}

$Type = ([MyType]::new()).GetType()

Import-Module $PSSCriptRoot\bin\debug\net472\BinaryExpressionConverter.dll -force
$PropName = 'Name'
$a = 'a'
$CrazyExp = @('a','b')
$HashExample = @{ 'Val1' = 'a2' }
$ValName = 'Val1'
$Expression = { $_."$PropName" -eq $HashExample.$ValName }
$ConvertedExpression = ConvertTo-BinaryExpression -FuncType $Type -Expression $Expression
$ConvertedExpression
$d = @("a")
$Expression = { @("a") -contains $_.Name }
$ConvertedExpression = ConvertTo-BinaryExpression -FuncType $Type -Expression $Expression
$ConvertedExpression

$Expression = { $_.Name -like 'test%' }
$ConvertedExpression = ConvertTo-BinaryExpression -FuncType $Type -Expression $Expression
$ConvertedExpression