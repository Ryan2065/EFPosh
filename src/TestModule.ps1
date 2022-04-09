using module C:\Users\Ryan2\OneDrive\Code\EFPosh\src\Module\EFPosh
using namespace System.ComponentModel.DataAnnotations.Schema;
using namespace System.ComponentModel.DataAnnotations;

if($PSScriptRoot){
    $ScriptLocation = $PSScriptRoot
}
else{
    $ScriptLocation = "C:\Users\Ryan2\OneDrive\Code\EFPosh\src"
}
$Env:EFPoshLog = 'true'
if($null -eq ( Get-Module EFPosh )){
    . "$PSScriptRoot\buildModule.ps1"
    Import-Module "$ScriptLocation\Module\EFPosh" -Force
}

$ErrorActionPreference = 'Continue'


$DBFile = "$ScriptLocation\bin\MyDatabase.sqlite"

$SQLiteConnectionString = "Filename=$DBFile"

if(Test-Path $DBFile){
    #Using cmd since Posh7 can't delete from OneDrive
    & cmd /c del $DBFile /F /Q
}

Class TestTableOne {
    [Key()]
    [int] $MyUniqueId
    [string]$Name
    [TestTableTwo]$RelationshipOne
}

Class TestTableTwo {
    [Key()]
    [int]$MyOtherUniqueId

    [ForeignKey("RelationshipTwo")]
    [int]$MyUniqueId
    [string]$Name
    [TestTableOne]$RelationshipTwo
}

$Tables = @(
    ( New-EFPoshEntityDefinition -Type 'TestTableOne'),
    ( New-EFPoshEntityDefinition -Type 'TestTableTwo' )
)

$Context = New-EFPoshContext -SQLiteFile $DBFile -Entities $Tables -EnsureCreated

$One = [TestTableOne]::new()
$One.Name = 'NewNameTest'
$Context.Add( $One )
$Context.SaveChanges()

$One = [TestTableOne]::new()
$One.Name = 'NewNameTest2'
$Context.Add( $One )
$Context.SaveChanges()

$NewObject = [TestTableTwo]::new()
$NewObject.Name = 'MyTest'
$NewObject.MyUniqueId = $One.MyUniqueId
$Context.Add( $NewObject )
$Context.SaveChanges()

$Name = 'NewNameTest'
$PropertyName = 'Name'
$Names = @( $Name, 'Test2' )
$result = Search-EFPosh -Entity $Context.TestTableOne -Expression { $0 -contains $_."$1" } -Arguments @($Names, $PropertyName)

$result.count

return

$QueryObject = New-EFPoshQuery -Type 'TestTableTwo'
$QueryObject.Where("Name=@0",'MyTest')
$QueryObject.ToList()


$k = $QueryObject.FirstOrDefault()
$k.Name = '2'
$Context.SaveChanges()

$QueryObject = New-EFPoshQuery -Type 'TestTableTwo'
$QueryObject.ToList()
