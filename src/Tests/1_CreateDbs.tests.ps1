$Global:TestSettings = Get-Content "$PSScriptRoot\bin\TestSettings.json" | ConvertFrom-JSON
$Global:DbContexts = @()
Describe 'Build DbContexts' {
    It 'Should return Context objects' {
        $SqlLiteContext = . "$PSScriptRoot\TestContext.ps1" -DbType 'SQLITE'
        $SqlLiteContextCredentials = . "$PSScriptRoot\TestContext.ps1" -DbType 'SQLITE' -WithCredentials
        $MSSqlContext = . "$PSScriptRoot\TestContext.ps1" -DbType 'MSSQL'
        $MSSqlContextCredentials = . "$PSScriptRoot\TestContext.ps1" -DbType 'MSSQL' -WithCredentials
        $Global:DbContexts += @{
            'Name' = 'SqlLiteContext'
            'DbContext' = $SqlLiteContext
        }
        $Global:DbContexts += @{
            'Name' = 'SqlLiteContextCredentials'
            'DbContext' = $SqlLiteContextCredentials
        }
        $Global:DbContexts += @{
            'Name' = 'MSSqlContext'
            'DbContext' = $MSSqlContext
        }
        $Global:DbContexts += @{
            'Name' = 'MSSqlContextCredentials'
            'DbContext' = $MSSqlContextCredentials
        }
        $SqlLiteContext.GetType().Name | Should -Be 'PoshContextInteractions'
        $SqlLiteContextCredentials.GetType().Name | Should -Be 'PoshContextInteractions'
        $MSSqlContext.GetType().Name | Should -Be 'PoshContextInteractions'
        $MSSqlContextCredentials.GetType().Name | Should -Be 'PoshContextInteractions'
    }
}