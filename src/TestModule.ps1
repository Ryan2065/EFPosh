if($PSScriptRoot){
    $ScriptLocation = $PSScriptRoot
}
else{
    $ScriptLocation = "C:\Users\Ryan2\OneDrive\Code\EFPosh\src"
}
$Env:EFPoshLog = 'true'
if($null -eq ( Get-Module EFPosh )){
    #. "$PSScriptRoot\buildModule.ps1"
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
    [int]$MyUniqueId
    [string]$Name
    [TestTableTwo]$RelationshipOne
}

Class TestTableTwo {
    [int]$MyOtherUniqueId
    [int]$MyUniqueId
    [string]$Name
    [TestTableOne]$RelationshipTwo
}

$Tables = @(
    ( New-EFPoshEntityDefinition -Type 'TestTableOne' -PrimaryKey 'MyUniqueId' ),
    ( New-EFPoshEntityDefinition -Type 'TestTableTwo' -PrimaryKey 'MyOtherUniqueId' )
)

#$Relationship = @( New-EFPoshEntityRelationship -SourceTypeName 'TestTableOne' -TargetTypeName 'TestTableTwo' -RelationshipType 'OneToOne' -SourceKey MyUniqueId -TargetKey MyUniqueId2 -SourceRelationshipProperty 'RelationshipOne' -TargetRelationshipProperty 'RelationshipTwo' )

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
$result = Search-EFPosh -Entity $Context.TestTableOne -Expression { $names -contains $_."$PropertyName" }

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
