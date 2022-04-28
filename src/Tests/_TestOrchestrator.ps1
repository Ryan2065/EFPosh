. C:\Users\Ryan2\OneDrive\Code\EFPosh\build.ps1

Import-Module Pester -Verbose:$false

Import-Module C:\Users\Ryan2\OneDrive\Code\EFPosh\src\Module\EFPosh -Force

if(Test-Path "$PSScriptRoot\bin" -ErrorAction SilentlyContinue){
    cmd /c rd "$PSScriptRoot\bin" /s /q
}

$null = New-Item "$PSScriptRoot\bin" -ItemType Directory -Force

. "$PSScriptRoot\_TestClasses.ps1"

Function RemoveDb{
    Param(
        [string]$ServerName,
        [string]$DbName
    )
    $connectionString = "Data Source=$ServerName; " +
    "Integrated Security=SSPI; " +
    "Initial Catalog=master"
    $connection = new-object system.data.SqlClient.SQLConnection($connectionString)
    $connection.Open()
    try{
        $sqlCommand = "
        Use {0};
        ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
        Use master;
        DROP DATABASE {0};
        " -f $DbName
        $command = new-object system.data.sqlclient.sqlcommand($sqlCommand,$connection)
        $adapter = New-Object System.Data.sqlclient.sqlDataAdapter $command
        $dataset = New-Object System.Data.DataSet
        $null = $adapter.Fill($dataSet)
    }
    catch{
        # probably doesn't exist
    }
    $connection.Close()
}

$DbContexts = @{}

$ContextParams = @{
    'Entities' = @(
        (New-EFPoshEntityDefinition -Type 'TestTableOne'),
        (New-EFPoshEntityDefinition -Type 'TestTableTwo'),
        (New-EFPoshEntityDefinition -Type 'TestTableThree')
    )
    'EnsureCreated' = $true
}

$DbContexts['Sqlite'] = New-EFPoshContext -SQLiteFile "$PSScriptRoot\bin\EFPoshTestDb.sqlite" @ContextParams
RemoveDb -ServerName 'Lab-CM.Home.Lab' -DbName 'EFPoshTestDb'
$DbContexts['MSSQL'] = New-EFPoshContext -MSSQLServer 'Lab-CM.Home.Lab' -MSSQLDatabase 'EFPoshTestDb' -MSSQLIntegratedSecurity $true @ContextParams

foreach($key in $DbContexts.Keys){
    $Container = New-PesterContainer -Path "$PSScriptRoot\1_ModifyData.tests.ps1" -Data @{ DbType = $key; DbContext = $DbContexts[$key] }
    Invoke-Pester -Container $Container
}