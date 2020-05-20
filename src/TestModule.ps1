if($PSScriptRoot){
    $ScriptLocation = $PSScriptRoot
}
else{
    $ScriptLocation = "C:\Users\Ryan2\OneDrive\Code\EFPosh\src"
}

if($null -eq ( Get-Module EFPosh )){
    . "$PSScriptRoot\buildModule.ps1"
}

$ErrorActionPreference = 'Stop'
Import-Module "$ScriptLocation\Module\EFPosh" -Force

$DBFile = "$ScriptLocation\bin\MyDatabase.sqlite"

$SQLiteConnectionString = "Filename=$DBFile"

if(Test-Path $DBFile){
    #Using cmd since Posh7 can't delete from OneDrive
    & cmd /c del $DBFile /F /Q
}

Class TestTableOne {
    [string]$MyUniqueId
    [string]$Name
}

Class TestTableTwo {
    [int]$MyOtherUniqueId
    [string]$Name
}

$Tables = @(
    ( New-EFPoshEntityDefinition -Type 'TestTableOne' -PrimaryKey 'MyUniqueId' ),
    ( New-EFPoshEntityDefinition -Type 'TestTableTwo' -PrimaryKey 'MyOtherUniqueId' )
)

$Context = New-EFPoshContext -SQLiteFile $DBFile -Entities $Tables -EnsureCreated

$NewObject = [TestTableTwo]::new()
$NewObject.Name = 'MyTest'

$Context.Add( $NewObject )
$Context.SaveChanges()

$NewObject = [TestTableTwo]::new()
$NewObject.Name = 'MyTest2'
$Context.Add( $NewObject )
$Context.SaveChanges()

$Results1 = $Context.TestTableTwo.Name.Equals("MyTest2").And.MyOtherUniqueId.Equals(2).ToList()
$results2 = $Context.TestTableTwo.Name.NotEquals("MyTest2").ToList()
$Context.TestTableTwo.Name.Contains("2").ToList()



return

$QueryObject = New-EFPoshQuery -Type 'TestTableTwo'
$QueryObject.Where("Name=@0",'MyTest')
$QueryObject.ToList()


$k = $QueryObject.FirstOrDefault()
$k.Name = '2'
$Context.SaveChanges()

$QueryObject = New-EFPoshQuery -Type 'TestTableTwo'
$QueryObject.ToList()
