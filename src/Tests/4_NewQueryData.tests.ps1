Describe 'Can query with functions' {
    #1_CreateDbs.tests.ps1 will create the Global:DbContexts variable
    It 'Correctly does FirstOrDefault in <Name>' -TestCases $Global:DbContexts {
        Param(
            [string]$Name,
            [EFPosh.PoshContextInteractions]$DbContext
        )
        (Search-EFPosh -Entity $DbContext.TableOne -FirstOrDefault).count | Should -be 1
        Search-EFPosh -Entity $DbContext.TableTwo -FirstOrDefault | Should -BeNullOrEmpty
    }
    It 'Correctly does ToList in <Name>' -TestCases $Global:DbContexts {
        Param(
            [string]$Name,
            [EFPosh.PoshContextInteractions]$DbContext
        )
        (Search-EFPosh -Entity $DbContext.TableOne).Count | Should -BeGreaterThan 1
    }
    It 'Queries with Equals in <Name>' -TestCases $Global:DbContexts {
        Param(
            [string]$Name,
            [EFPosh.PoshContextInteractions]$DbContext
        )
        $Result = Search-EFPosh -Entity $DbContext.TableOne -Expression { $_.Id -eq 1 }
        $Result.Count | Should -be 1
        $Result.Id | Should -be 1
    }
    It 'Queries with NotEquals in <Name>' -TestCases $Global:DbContexts {
        Param(
            [string]$Name,
            [EFPosh.PoshContextInteractions]$DbContext
        )
        $All = Search-EFPosh -Entity $DbContext.TableOne
        $Results = Search-EFPosh -Entity $DbContext.TableOne -Expression { $_.Id -ne 1 }
        $Results.Count | Should -be ( $All.Count - 1 )
        foreach($result in $Results) {
            $Result.Id | Should -Not -Be 1
        }
    }
    It 'Queries with Contains in <Name>' -TestCases $Global:DbContexts {
        Param(
            [string]$Name,
            [EFPosh.PoshContextInteractions]$DbContext
        )
        $Results = Search-EFPosh -Entity $DbContext.TableOne -Expression { $_.Name -contains 'TestName1' }
        $Results.Count | Should -Be 12
        foreach($result in $Results) {
            $Result.Name.ToLower().Contains('testname1') | Should -BeTrue
        }
    }
    It 'Queries with Array Contains in <Name>' -TestCases $Global:DbContexts {
        Param(
            [string]$Name,
            [EFPosh.PoshContextInteractions]$DbContext
        )
        $Results = Search-EFPosh -Entity $DbContext.TableOne -Expression { @('TestName1') -contains $_.Name }
        $Results.Count | Should -Be 1
        foreach($result in $Results) {
            $Result.Name | Should -Be 'TestName1'
        }
    }
    It 'Queries with NotContains in <Name>' -TestCases $Global:DbContexts {
        Param(
            [string]$Name,
            [EFPosh.PoshContextInteractions]$DbContext
        )
        $Results = Search-EFPosh -Entity $DbContext.TableOne -Expression { $_.Name -notcontains 'TestName1' } 
        $Results.Count | Should -Be 89
        foreach($result in $Results) {
            $Result.Name.ToLower().Contains('testname1') | Should -BeFalse
        }
    }
    It 'Queries with Array NotContains in <Name>' -TestCases $Global:DbContexts {
        Param(
            [string]$Name,
            [EFPosh.PoshContextInteractions]$DbContext
        )
        $AllResults = Search-EFPosh -Entity $DbContext.TableOne
        $Results = Search-EFPosh -Entity $DbContext.TableOne -Expression { @('TestName1') -notcontains $_.Name }
        $Results.Count | Should -Be ( $AllResults.Count - 1 )
        foreach($result in $Results) {
            $Result.Name | Should -Not -Be 'TestName1'
        }
    }
    It 'Queries with StartsWith in <Name>' -TestCases $Global:DbContexts {
        Param(
            [string]$Name,
            [EFPosh.PoshContextInteractions]$DbContext
        )
        $Results = Search-EFPosh -Entity $DbContext.TableOne -Expression { $_.Name -like "TestName1%" }
        $Results.Count | Should -Be 12
        foreach($result in $Results) {
            $Result.Name.ToLower().Contains('testname1') | Should -BeTrue
        }
    }
    It 'Queries with EndsWith in <Name>' -TestCases $Global:DbContexts {
        Param(
            [string]$Name,
            [EFPosh.PoshContextInteractions]$DbContext
        )
        $Results = Search-EFPosh -Entity $DbContext.TableOne -Expression { $_.Name -like "%0" }
        $Results.Count | Should -Be 10
        foreach($result in $Results) {
            $Result.Name.ToLower().EndsWith('0') | Should -BeTrue
        }
    }
    It 'Queries with GreaterThan in <Name>' -TestCases $Global:DbContexts {
        Param(
            [string]$Name,
            [EFPosh.PoshContextInteractions]$DbContext
        )
        $Results = Search-EFPosh -Entity $DbContext.TableOne -Expression { $_.Id -gt 50 }
        $Results.Count | Should -Be 51
        foreach($result in $Results) {
            $Result.Id | Should -BeGreaterThan 50
        }
    }
    It 'Queries with GreaterThanOrEqualTo in <Name>' -TestCases $Global:DbContexts {
        Param(
            [string]$Name,
            [EFPosh.PoshContextInteractions]$DbContext
        )
        $Results = Search-EFPosh -Entity $DbContext.TableOne -Expression { $_.Id -ge 50 }
        $Results.Count | Should -Be 52
        foreach($result in $Results) {
            $Result.Id | Should -BeGreaterThan 49
        }
    }
    It 'Queries with LessThan in <Name>' -TestCases $Global:DbContexts {
        Param(
            [string]$Name,
            [EFPosh.PoshContextInteractions]$DbContext
        )
        $Results = Search-EFPosh -Entity $DbContext.TableOne -Expression { $_.Id -lt 50 }
        $Results.Count | Should -Be 49
        foreach($result in $Results) {
            $Result.Id | Should -BeLessThan 50
        }
    }
    It 'Queries with LessThanOrEqualTo in <Name>' -TestCases $Global:DbContexts {
        Param(
            [string]$Name,
            [EFPosh.PoshContextInteractions]$DbContext
        )
        $Results = Search-EFPosh -Entity $DbContext.TableOne -Expression { $_.Id -le 50 }
        $Results.Count | Should -Be 50
        foreach($result in $Results) {
            $Result.Id | Should -BeLessOrEqual 50
        }
    }
    It 'Queries with And in <Name>' -TestCases $Global:DbContexts {
        Param(
            [string]$Name,
            [EFPosh.PoshContextInteractions]$DbContext
        )
        $Results = Search-EFPosh -Entity $DbContext.TableOne -Expression { $_.Id -eq 1 -and $_.Name -contains 'Test' }
        $Results.Count | Should -Be 1
        foreach($result in $Results) {
            $Result.Id | Should -Be 1
        }
    }
    It 'Queries with Or in <Name>' -TestCases $Global:DbContexts {
        Param(
            [string]$Name,
            [EFPosh.PoshContextInteractions]$DbContext
        )
        $Results = Search-EFPosh -Entity $DbContext.TableOne -Expression { $_.Id -eq 1 -or $_.Id -eq 2 -or $_.Id -eq 3 }
        $Results.Count | Should -Be 3
    }
    It 'Honors select list in <Name>' -TestCases $Global:DbContexts {
        Param(
            [string]$Name,
            [EFPosh.PoshContextInteractions]$DbContext
        )
        $Results = Search-EFPosh -Entity $DbContext.TableOne -Expression { $_.Id -eq 1 } -Select 'Id'
        $Results.Name | Should -Be $null
        $Results.Id | Should -Be 1
        @($Results).Count | Should -Be 1
    }
    It 'Honors select list in <Name>' -TestCases $Global:DbContexts {
        Param(
            [string]$Name,
            [EFPosh.PoshContextInteractions]$DbContext
        )
        $Results = Search-EFPosh -Entity $DbContext.TableOne -Expression { $_.Id -eq 1 } -Select 'Id' -FirstOrDefault
        $Results[0].Name | Should -Be $null
        $Results[0].Id | Should -Be 1
    }
}