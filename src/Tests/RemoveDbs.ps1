$SqliteFiles = @(
    "C:\Users\Public\bin\EFPoshTestDb.sqlite",
    "$PSScriptRoot\bin\EFPoshTestDb.sqlite"
)

$MSSqlDbs = @(
    'EFPoshTests',
    'EFPoshTestsCredential'
)

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

Foreach($file in $SqliteFiles) {
    if(Test-Path $file) {
        $null = Remove-Item $File -Force
    }
}

$RemoveSqlDb = @'
IF EXISTS ( Select * from sys.databases d where d.name = '{0}' )
BEGIN
    DROP DATABASE {0};
END
'@

$RemoveSqlDbInCatch = @'
Use {0};

ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

Use master;

DROP DATABASE {0};
'@

Foreach($db in $MSSqlDbs) {
    try{
        $null = Invoke-SQL -dataSource $Global:TestSettings.MSSqlServer -database 'master' -sqlCommand ( $RemoveSqlDb -f $Db )
    }
    catch {
        $null = Invoke-SQL -dataSource $Global:TestSettings.MSSqlServer -database 'master' -sqlCommand ( $RemoveSqlDbInCatch -f $Db )
    }
}

$AlternateUser = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $Global:TestSettings.'SQLiteAlternativeCredentials-User', ($Global:TestSettings.'SQLiteAlternativeCredentials-Pw' | ConvertTo-SecureString)
$FilePath = "C:\Users\$($AlternateUser.GetNetworkCredential().UserName)\EFPoshTestDb.sqlite"
$UserName = $AlternateUser.GetNetworkCredential().UserName
$Domain = $AlternateUser.GetNetworkCredential().Domain
$Password = $AlternateUser.GetNetworkCredential().Password
$tokenHandle = 0
Add-Type -Namespace Import -Name Win32 -MemberDefinition @' 
    [DllImport("advapi32.dll", SetLastError = true)] 
    public static extern bool LogonUser(string user, string domain, string password, int logonType, int logonProvider, out IntPtr token); 
 
    [DllImport("kernel32.dll", SetLastError = true)] 
    public static extern bool CloseHandle(IntPtr handle); 
'@ 
$null = [Import.Win32]::LogonUser($Username, $Domain, $Password, 2, 0, [ref]$tokenHandle)
[System.Security.Principal.WindowsIdentity]::RunImpersonated($tokenHandle, {
    if(Test-Path $FilePath) {
        $null = Remove-Item $FilePath -Force
    }
})
[void][Import.Win32]::CloseHandle($tokenHandle)
$Username = $Domain = $Password = $null
