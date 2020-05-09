if($PSScriptRoot){
    $ScriptLocation = $PSScriptRoot
}
else{
    $ScriptLocation = "C:\Users\Ryan2\OneDrive\Code\EFPosh\src"
}

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

$Context = New-EFPoshContext -ConnectionString $SQLiteConnectionString -DBType 'SQLite' -Entities $Tables -EnsureCreated

$NewObject = [TestTableTwo]::new()
$NewObject.Name = 'MyTest'

$Context.Add( $NewObject )
$Context.SaveChanges()

$NewObject = [TestTableTwo]::new()
$NewObject.Name = 'MyTest2'
$Context.Add( $NewObject )
$Context.SaveChanges()


$QueryObject = New-EFPoshQuery -Type 'TestTableTwo'
$QueryObject.Where("Name=@0",'MyTest')
$QueryObject.ToList()


$k = $QueryObject.FirstOrDefault()
$k.Name = '2'
$Context.SaveChanges()

$QueryObject = New-EFPoshQuery -Type 'TestTableTwo'
$QueryObject.ToList()