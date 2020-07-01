Param(
    [ValidateSet('MSSQL', 'SQLITE')]
    $DbType,
    [switch]$WithCredentials
)

$TestSettings = $Global:TestSettings

Class TableOne {
    [int]$Id
    [string]$Name
}

Class TableTwo {
    [int]$Id
    [string]$ColumnTwo
    [DateTime]$Date
    [TableThree]$TableThree
}

Class TableThree{
    [int]$Id
    [int]$TableTwoId
    [string]$Name
    [TableTwo]$TableTwo
}

Class FromSQLClass{
    [int]$Id
    [string]$Name
}

$ContextParams = @{
    'Entities' = @(
        (New-EFPoshEntityDefinition -Type 'TableOne' -PrimaryKeys 'Id'),
        (New-EFPoshEntityDefinition -Type 'TableTwo' -PrimaryKeys 'Id')
    )
    'EnsureCreated' = $true
}

$Context = $null

switch ($DbType.ToUpper()) {
    'SQLITE' {
        $FilePath = "$PSScriptRoot\bin\EFPoshTestDb.sqlite"
        if(-not ( Test-Path "$PSScriptRoot\bin" )) {
            $null = New-Item -Path "$PSScriptRoot\bin" -ItemType Directory -Force
        }
        if($WithCredentials){
            $AlternateUser = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $TestSettings.'SQLiteAlternativeCredentials-User', ($TestSettings.'SQLiteAlternativeCredentials-Pw' | ConvertTo-SecureString)
            $ContextParams['Credential'] = $AlternateUser
            $ContextParams['LogonType'] = 'Interactive'
            $FilePath = "C:\Users\$($AlternateUser.GetNetworkCredential().UserName)\EFPoshTestDb.sqlite"
        }
        if(Test-Path $FilePath){
            $null = Remove-Item -Path $FilePath -Force
        }
        $Context = New-EFPoshContext -SQLiteFile $FilePath @ContextParams
    }
    'MSSQL' {
        $ContextParams['Entities'] += New-EFPoshEntityDefinition -Type 'FromSQLClass' -Keyless -FromSql 'Select * from TableOne'
        $DbName = 'EFPoshTests'
        if($WithCredentials){
            $ContextParams['Credential'] = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $TestSettings.'MSSQLAlternativeCredentials-User', ($TestSettings.'MSSQLAlternativeCredentials-Pw' | ConvertTo-SecureString)
            $DbName = 'EFPoshTestsCredential'
        }
        $Context = New-EFPoshContext -MSSQLServer $TestSettings.MSSqlServer -MSSQLDatabase $DbName -MSSQLIntegratedSecurity $TestSettings.IntegratedSecurity @ContextParams
    }
}

return $Context