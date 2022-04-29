Param([switch]$SkipBuild)
if(-not $SkipBuild){
    #. C:\Users\Ryan2\OneDrive\Code\EFPosh\build.ps1
}

$ErrorActionPreference = 'Stop'

Import-Module Pester -MinimumVersion 5.0 -Verbose:$false
$EFposhModuleLocation = [System.IO.Path]::Combine((Get-Item $PSScriptRoot).Parent.FullName, "Module", "EFPosh")
Import-Module $EFposhModuleLocation -Verbose:$false -Force

# VSCode locks the sqlite file
Get-Process sqlite-v3.26.0-win32-x86 -ErrorAction SilentlyContinue | Stop-Process -Force

$BinFolder = [System.IO.Path]::Combine($PSScriptRoot, "bin")

if(Test-Path $BinFolder -ErrorAction SilentlyContinue){
    if($IsLinux){
        Remove-Item $BinFolder -Force -Recurse
    }
    else{
        #Remove-Item doesn't work on Posh5
        cmd /c rd "$BinFolder" /s /q
    }
    
}

$VerbosePreference = "Continue"
$DebugPreference = "Continue"

$null = New-Item $BinFolder -ItemType Directory -Force

. ( [System.IO.Path]::Combine($PSScriptRoot, "_TestClasses.ps1") )

Function RemoveDb{
    Param(
        [string]$DbName
    )
    $connectionString = $env:EFPoshMasterSqlServerConnectionString
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

$SqliteFile = [System.IO.Path]::Combine($PSScriptRoot, 'bin', 'EFPoshTestDb.sqlite')

$DbContexts['Sqlite'] = New-EFPoshContext -SQLiteFile $SqliteFile @ContextParams

RemoveDb -DbName 'EFPoshTestDb'
$DbContexts['MSSQL'] = New-EFPoshContext -ConnectionString $env:EFPoshSqlServerConnectionString -DBType MSSQL @ContextParams


$Containers = @()

$ModifyDataFile = [System.IO.Path]::Combine($PSScriptRoot, '1_ModifyData.tests.ps1')
$SearchDataFile = [System.IO.Path]::Combine($PSScriptRoot, '2_SearchData.tests.ps1')

foreach($key in $DbContexts.Keys){
    $Containers += New-PesterContainer -Path $ModifyDataFile -Data @{ DbType = $key; DbContext = $DbContexts[$key] }
    $Containers += New-PesterContainer -Path $SearchDataFile -Data @{ DbType = $key; DbContext = $DbContexts[$key]; ContextParams = $ContextParams }
}
Invoke-Pester -Container $Containers