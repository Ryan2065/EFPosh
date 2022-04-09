<#
  The PowerShellEditorServices causes some weird issues with this module so
  this script was created to debug with the errors it creates. They have been resolved
  so this shouldn't be needed anymore, but keeping it here just in case.
#>


$DebugRuntime = "net472"
if($PSVersionTable.PSVersion.Major -gt 5){
  $DebugRuntime = "net6.0"
}

$ErrorActionPreference = 'SilentlyContinue'
$ErrorActionPreference = 'Stop'

Function Join-MultiplePaths{
  Param(
      [string[]]$Paths
      )
  if($paths.Count -lt 2){
    return $Paths
  }
  $newPath = Join-Path $Paths[0] $Paths[1]
  if($paths.count -gt 2){
    $newPathArray = @($newPath)
    for($i = 2; $i -lt $paths.count; $i++){
      $newPathArray += $paths[$i]
    }
    Join-MultiplePaths $newPathArray
  }
  else{
    $newPath
  }
}
$efPoshDLL = Join-MultiplePaths @($PSScriptRoot, "EFPosh", "EFPosh", "bin", "debug", $DebugRuntime, "EFPosh.dll")
$BinaryExpDll = Join-MultiplePaths @($PSScriptRoot, "EFPosh", "EFPosh", "bin", "debug", $DebugRuntime, "BinaryExpressionConverter.dll")
$null = Add-Type -Path $efPoshDLL
Import-Module $BinaryExpDll -Force

$sqlFile = Join-MultiplePaths @($PSScriptRoot, "bin", "sql.sqlite")

Remove-Item $sqlFile -Force -ErrorAction SilentlyContinue

$VerbosePreference = 'Continue'
$DebugPreference = 'Continue'

$ConnectionString = "Filename=$sqlFile"
Class MyTest {
    [int]$MyTestId
    [string]$test
    [System.Collections.Generic.ICollection[MyTest2]]$Tests
}

Class MyTest2{
  [int]$MyTest2Id
  [string]$TestColumn
  [int]$MyTestId
  [MyTest]$Test
}
$VerbosePreference = 'Continue'
$Script:LatestDBContext = [EFPosh.PoshContextInteractions]::new()
  $Type = 'MyTest'
  $newEntity = [EFPosh.PoshEntity]::new()
  $newEntity.Type = (New-Object -TypeName $Type).GetType()
  $newEntity2 = [EFPosh.PoshEntity]::new()
  $newEntity2.Type = (New-Object -TypeName 'MyTest2').GetType()
  $DBType = 'SQLITE'
  $Entities = @(
      $newEntity,
      $newEntity2
  )
  $null = $Script:LatestDBContext.NewPoshContext($ConnectionString, $DBType, $Entities, $true, $false)
  $test = [MyTest]::New();
$test.test = "my test"
$Script:LatestDBContext.Add($test)
$test = [MyTest]::New();
$test.test = "aamy test"
$Script:LatestDBContext.Add($test)
$test = [MyTest]::New();
$test.test = "zzmy test"
$Script:LatestDBContext.Add($test)
$test = [MyTest2]::New();
$test.TestColumn = "zzmy test"
$test.MyTestId = 1
$Script:LatestDBContext.Add($test)

$Script:LatestDBContext.SaveChanges()


$test = [MyTest]::New();
$test.test = "my test"
$Script:LatestDBContext.Add($test)
$test = [MyTest]::New();
$test.test = "aamy test"
$Script:LatestDBContext.Add($test)
$test = [MyTest]::New();
$test.test = "zzmy test"
$Script:LatestDBContext.Add($test)
$Script:LatestDBContext.SaveChanges()
#$Script:LatestDBContext.MyTest.Select("test")
#$Script:LatestDBContext.MyTest.ToList();

write-host 'order by test'

#$Script:LatestDBContext.MyTest.OrderByDescending("test")
#$Script:LatestDBContext.MyTest.ToList();

Function MyTest([object]$DbContextEntity, [string[]]$SelectList){
  $ConvertedExpression = ConvertTo-BinaryExpression -FuncType $DbContextEntity.GetBaseType() -Expression {$_.test -eq 'my test'} -Arguments @($Arguments)
  $DbContextEntity.ApplyExpression($ConvertedExpression)
  $DbContextEntity.Select($SelectList);
  $DbContextEntity.ToList();
}

MyTest -DbContextEntity $Script:LatestDBContext.MyTest -SelectList 'test'

$Script:LatestDBContext.MyTest.Include("Tests")
$mytestResults = $Script:LatestDBContext.MyTest.ToList()

$ConvertedExpression = ConvertTo-BinaryExpression -FuncType $Script:LatestDBContext.MyTest2.GetBaseType() -Expression {$_.Test.MyTestId -eq 1} 
$Script:LatestDBContext.MyTest2.ApplyExpression($ConvertedExpression)
return
