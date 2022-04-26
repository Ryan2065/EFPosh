using namespace System.ComponentModel.DataAnnotations.Schema;
using namespace System.ComponentModel.DataAnnotations;


Class TestTableOne {
    [Key()]
    [int] $MyUniqueId
    [string]$Name

    [DateTime]$Created

    #[TestTableTwo]$RelationshipOne
}

Class TestTableTwo {
    [Key()]
    [int]$MyOtherUniqueId
    [string]$Name

    #[ForeignKey("RelationshipTwo")]
    [int]$MyUniqueId
    
    #[TestTableOne]$RelationshipTwo

    #[ForeignKey("TestTableTwoId")]
    #[System.Collections.Generic.ICollection[TestTableThree]]$TableThrees
}

Class TestTableThree{
    [Key()]
    [DatabaseGeneratedAttribute([DatabaseGeneratedOption]::Identity)]
    [int]$Id

    [string]$Name

    [ForeignKey("TestTableTwo")]
    [int]$TestTableTwoId
    [TestTableTwo]$TestTableTwo
}


$Tables = @(
    ( New-EFPoshEntityDefinition -Type 'TestTableOne'),
    ( New-EFPoshEntityDefinition -Type 'TestTableTwo' )
    #( New-EFPoshEntityDefinition -Type 'TestTableThree' )
)
<#
function Invoke-SQL {
    param(
        [string] $dataSource,
        [string] $database,
        [string] $sqlCommand = $(throw "Please specify a query.")
      )

    $connectionString = "Data Source=$dataSource; " +
            "Integrated Security=SSPI; " +
            "Initial Catalog=$database"

    $connection = new-object system.data.SqlClient.SQLConnection($connectionString)
    $command = new-object system.data.sqlclient.sqlcommand($sqlCommand,$connection)
    $connection.Open()

    $adapter = New-Object System.Data.sqlclient.sqlDataAdapter $command
    $dataset = New-Object System.Data.DataSet
    $adapter.Fill($dataSet) | Out-Null

    $connection.Close()
    $dataSet.Tables

}

$Sql = @'
Use {0};

ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

Use master;

DROP DATABASE {0};
'@ -f "EFPoshTest$($PSVersionTable.PSEdition)"


try{
    Invoke-SQL -dataSource 'Lab-CM.Home.Lab' -database "EFPoshTest$($PSVersionTable.PSEdition)" -sqlCommand $sql

}
catch {}
#>
$DbName = "EFPoshTest$($PSVersionTable.PSEdition)"

New-EFPoshContext -MSSQLServer 'Lab-CM.Home.Lab' -MSSQLDatabase "EFPoshTest$($PSVersionTable.PSEdition)" -MSSQLIntegratedSecurity $true -Entities $Tables -EnsureCreated


$SqliteFile = [System.IO.Path]::Combine($PSScriptRoot, "bin", "EFPoshTest$($PSVersionTable.PSEdition).sqlite" )

if(Test-Path $SqliteFile){
    $null = Remove-Item $SqliteFile -Force
}

New-EFPoshContext -SQLiteFile $SqliteFile -Entities $Tables -EnsureCreated