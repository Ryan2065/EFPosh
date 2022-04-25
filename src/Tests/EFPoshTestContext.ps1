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

$SqlCmd = New-Object System.Data.SqlClient.SqlCommand
$Sql = @'
Use {0};

ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

Use master;

DROP DATABASE {0};
'@ -f "EFPoshTest$($PSVersionTable.PSEdition)"
try{
    $SqlCmd.CommandText = $sql
    $sqlConnection = New-Object System.Data.SqlClient.SqlConnection "Server=Lab-CM.Home.Lab;Integrated Security=True;"
    $null = $sqlConnection.Open()
    $SqlCmd.Connection = $sqlConnection
    $Reader= $SqlCmd.ExecuteReader()
    $null = $SqlConnection.Close()
}
catch {}

$DbName = "EFPoshTest$($PSVersionTable.PSEdition)"
WRite-host $DbName

New-EFPoshContext -MSSQLServer 'Lab-CM.Home.Lab' -MSSQLDatabase "EFPoshTest$($PSVersionTable.PSEdition)" -MSSQLIntegratedSecurity $true -Entities $Tables -EnsureCreated


$SqliteFile = [System.IO.Path]::Combine($PSScriptRoot, "bin", "EFPoshTest$($PSVersionTable.PSEdition).sqlite" )

if(Test-Path $SqliteFile){
    $null = Remove-Item $SqliteFile -Force
}

New-EFPoshContext -SQLiteFile $SqliteFile -Entities $Tables -EnsureCreated