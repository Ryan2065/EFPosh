$Global:TestSettings = Get-Content "$PSScriptRoot\bin\TestSettings.json" | ConvertFrom-JSON
$Global:DbContexts = @()
Import-Module "C:\Users\Ryan2\OneDrive\Code\EFPosh\src\Module\EFPosh" -Force
Describe 'Build DbContexts' {
    It 'Should return Context objects' {
        $SqlLiteContext = . "$PSScriptRoot\TestContext.ps1" -DbType 'SQLITE'
        $MSSqlContext = . "$PSScriptRoot\TestContext.ps1" -DbType 'MSSQL'
        $Global:DbContexts += @{
            'Name' = 'SqlLiteContext'
            'DbContext' = $SqlLiteContext
        }
        $Global:DbContexts += @{
            'Name' = 'MSSqlContext'
            'DbContext' = $MSSqlContext
        }
        $SqlLiteContext.GetType().Name | Should -Be 'PoshContextInteractions'
        $MSSqlContext.GetType().Name | Should -Be 'PoshContextInteractions'
    }
}
