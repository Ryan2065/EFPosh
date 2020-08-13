# EFPosh

## Entity Framework for PowerShell

This module exposes a lot of Entity Framework Core features through PowerShell! Now you can create and interact with databases without having to write any SQL!

## Getting Started

Install the module with ```Install-Module EFPosh```

Entity Framework works by taking existing classes and mapping them to databases. This works with any class, whether it be defined in PowerShell or a dll. To quickly get started with this module, define your classes in PowerShell:

``` PowerShell
Class TestTableOne {
    [string]$MyUniqueId
    [string]$Name
}

Class TestTableTwo {
    [int]$MyOtherUniqueId
    [string]$Name
}
```

Then build a Table array:

``` PowerShell
$Tables = @(
    ( New-EFPoshEntityDefinition -Type 'TestTableOne' -PrimaryKey 'MyUniqueId' ),
    ( New-EFPoshEntityDefinition -Type 'TestTableTwo' -PrimaryKey 'MyOtherUniqueId' )
)
```

Note: New-EFPoshEntityDefinition has a "Keyless" option. Keyless means there's no unique column. This will model the table/view but make it read-only. A key is required to write to the database. Keys default to a column named "Id" but can be defined with -PrimaryKey (which accepts an array of columns)

After building the table array, build your database context!

``` PowerShell
$DBFile = "$PSScriptRoot\bin\MyDatabase.sqlite"
$Context = New-EFPoshContext -ConnectionString $SQLiteConnectionString -DBType 'SQLite' -Entities $Tables -EnsureCreated
```

EnsureCreated will create the database if it does not exist, with the tables specified. If the database exists, it will *not* check to ensure the database columns have not changed. That is currently not supported. If your columns change, you have to create a new DB to get the new schema or update the schema yourself.

Now your ```$Context``` object lets you do a lot of normal Entity Framework actions!

You can add an object to the database:

``` PowerShell
$NewObject = [TestTableTwo]::new()
$NewObject.Name = 'MyTest'

$Context.Add( $NewObject )
$Context.SaveChanges()
```

You can query for existing objects:

``` PowerShell
Search-EFPosh -Entity $Context.TestTableTwo -Expression { $_.Name -eq 'MyTest' }
```

Querying with variables is easy also, but not straight-forward:

``` PowerShell
$SearchFor = 'MyTest'
Search-EFPosh -Entity $Context.TestTableTwo -Expression { $_.Name -eq $0 } -Arguments @($SearchFor)
```

In the above example, use number variables representing the index in the Arguments array. So $0 corresponds to index 0 in Arguments.

You can easily edit objects in the database:

``` PowerShell
$Result = Search-EFPosh -Entity $Context.TestTableTwo -FirstOrDefault
$Result.Name = 'MyNewName'
$Context.SaveChanges()
```

Please let me know if you have any issues!
