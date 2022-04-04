<#
  The PowerShellEditorServices causes some weird issues with this module so
  this script was created to debug with the errors it creates. They have been resolved
  so this shouldn't be needed anymore, but keeping it here just in case.
#>


$ErrorActionPreference = 'SilentlyContinue'
#$null = Import-Module 'c:\Users\Ryan2\.vscode\extensions\ms-vscode.powershell-preview-2020.4.3\modules\PowerShellEditorServices\PowerShellEditorServices.psd1'; Start-EditorServices -HostName 'Visual Studio Code Host'  -AdditionalModules @('PowerShellEditorServices.VSCode') -BundledModulesPath 'c:\Users\Ryan2\.vscode\extensions\ms-vscode.powershell-preview-2020.4.3\modules' -EnableConsoleRepl -StartupBanner "=====> PowerShell Preview Integrated Console v2020.4.3 <=====" -LogLevel 'Normal' -LogPath 'c:\Users\Ryan2\.vscode\extensions\ms-vscode.powershell-preview-2020.4.3\logs\1589298796-bbb3adc4-f7a0-4247-b849-4827ca4a39561589298237173\EditorServices.log' -Stdio -SessionDetailsPath .\ -HostVersion 1.0 -HostProfileId 'PowerShell' -FeatureFlags @()
$ErrorActionPreference = 'Stop'
$null = Add-Type -Path "C:\users\ryan2\OneDrive\Code\EFPosh\src\EFPosh\EFPosh\bin\Debug\net472\EFPosh.dll"
Import-Module 'C:\Users\Ryan2\OneDrive\Code\EFPosh\src\EFPosh\BinaryExpressionConverter\bin\Release\net472\BinaryExpressionConverter.dll' -Force



Remove-Item 'c:\users\ryan2\sql.sqlite' -Force -ErrorAction SilentlyContinue

$VerbosePreference = 'Continue'
$DebugPreference = 'Continue'

$ConnectionString = "Filename=c:\users\ryan2\sql.sqlite"
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
