Describe 'Can query with functions' {
    #1_CreateDbs.tests.ps1 will create the Global:DbContexts variable
    It 'Correctly does FirstOrDefault in <Name>' -TestCases $Global:DbContexts {
        Param(
            [string]$Name,
            [EFPosh.PoshContextInteractions]$DbContext
        )
        New-EFPoshQuery -DBContext $DbContext -Entity 'TableOne'
        (Start-EFposhQuery -FirstOrDefault).Count | Should -Be 1
        New-EFPoshQuery -DBContext $DbContext -Entity 'TableTwo'
        (Start-EFposhQuery -FirstOrDefault) | Should -BeNullOrEmpty
    }
    It 'Correctly does ToList in <Name>' -TestCases $Global:DbContexts {
        Param(
            [string]$Name,
            [EFPosh.PoshContextInteractions]$DbContext
        )
        New-EFPoshQuery -DBContext $DbContext -Entity 'TableOne'
        (Start-EFposhQuery -ToList).Count | Should -BeGreaterThan 1
    }
    It 'Queries with Equals in <Name>' -TestCases $Global:DbContexts {
        Param(
            [string]$Name,
            [EFPosh.PoshContextInteractions]$DbContext
        )
        New-EFPoshQuery -DBContext $DbContext -Entity 'TableOne'
        Add-EFPoshQuery -Property Id -Equals 1
        $Result = Start-EFposhQuery -ToList
        $Result.Count | Should -be 1
        $Result.Id | Should -be 1
    }
    It 'Queries with NotEquals in <Name>' -TestCases $Global:DbContexts {
        Param(
            [string]$Name,
            [EFPosh.PoshContextInteractions]$DbContext
        )
        New-EFPoshQuery -DBContext $DbContext -Entity 'TableOne'
        $All = Start-EFposhQuery -ToList

        New-EFPoshQuery -DBContext $DbContext -Entity 'TableOne'
        Add-EFPoshQuery -Property Id -NotEquals 1
        $Results = Start-EFposhQuery -ToList
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
        New-EFPoshQuery -DBContext $DbContext -Entity 'TableOne'
        Add-EFPoshQuery -Property Name -Contains 'TestName1'
        $Results = Start-EFposhQuery -ToList
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
        New-EFPoshQuery -DBContext $DbContext -Entity 'TableOne'
        Add-EFPoshQuery -Property Name -Contains @('TestName1')
        $Results = Start-EFposhQuery -ToList
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
        New-EFPoshQuery -DBContext $DbContext -Entity 'TableOne'
        Add-EFPoshQuery -Property Name -NotContains 'TestName1'
        $Results = Start-EFposhQuery -ToList
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
        New-EFPoshQuery -DBContext $DbContext -Entity 'TableOne'
        $AllResults = Start-EFposhQuery -ToList
        New-EFPoshQuery -DBContext $DbContext -Entity 'TableOne'
        Add-EFPoshQuery -Property Name -NotContains @('TestName1')
        $Results = Start-EFposhQuery -ToList
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
        New-EFPoshQuery -DBContext $DbContext -Entity 'TableOne'
        Add-EFPoshQuery -Property Name -StartsWith 'TestName1'
        $Results = Start-EFposhQuery -ToList
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
        New-EFPoshQuery -DBContext $DbContext -Entity 'TableOne'
        Add-EFPoshQuery -Property Name -EndsWith '0'
        $Results = Start-EFposhQuery -ToList
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
        New-EFPoshQuery -DBContext $DbContext -Entity 'TableOne'
        Add-EFPoshQuery -Property Id -GreaterThan 50
        $Results = Start-EFposhQuery -ToList
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
        New-EFPoshQuery -DBContext $DbContext -Entity 'TableOne'
        Add-EFPoshQuery -Property Id -GreaterThanOrEqualTo 50
        $Results = Start-EFposhQuery -ToList
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
        New-EFPoshQuery -DBContext $DbContext -Entity 'TableOne'
        Add-EFPoshQuery -Property Id -LessThan 50
        $Results = Start-EFposhQuery -ToList
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
        New-EFPoshQuery -DBContext $DbContext -Entity 'TableOne'
        Add-EFPoshQuery -Property Id -LessThanOrEqualTo 50
        $Results = Start-EFposhQuery -ToList
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
        New-EFPoshQuery -DBContext $DbContext -Entity 'TableOne'
        Add-EFPoshQuery -Property Id -Equals 1 -And
        Add-EFPoshQuery -Property Name -Contains 'Test'
        $Results = Start-EFposhQuery -ToList
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
        New-EFPoshQuery -DBContext $DbContext -Entity 'TableOne'
        Add-EFPoshQuery -Property Id -Equals 1 -Or
        Add-EFPoshQuery -Property Id -Equals 2 -Or
        Add-EFPoshQuery -Property Id -Equals 3
        $Results = Start-EFposhQuery -ToList
        $Results.Count | Should -Be 3
    }
}