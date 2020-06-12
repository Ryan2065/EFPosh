Describe 'Validate Adding and removing works' {
    #1_CreateDbs.tests.ps1 will create the Global:DbContexts variable
    It 'Correctly adds to <Name>' -TestCases $Global:DbContexts {
        Param(
            [string]$Name,
            [EFPosh.PoshContextInteractions]$DbContext
        )
        $newObj = ([EFPosh.PoshContextInteractions]$DbContext).TableOne.New()
        $newObj.Name = 'TestName'
        $DbContext.Add($newObj)
        # Id is managed by the database after save changes is run
        $newObj.Id | Should -BeLessOrEqual 0
        { $DbContext.SaveChanges() } | Should -not -Throw
        # Now Id should have value
        $newObj.Id | Should -BeGreaterThan 0
    }
    It 'Correctly adds range to <Name>' -TestCases $Global:DbContexts {
        Param(
            [string]$Name,
            [EFPosh.PoshContextInteractions]$DbContext
        )
        $NumberOfObjects = 100
        $newObjs = @()
        do{
            $count++
            $newObj = ([EFPosh.PoshContextInteractions]$DbContext).TableOne.New()
            $newObj.Name = "TestName$($count)"
            $newObjs += $newObj
        }while($count -lt $NumberOfObjects)

        foreach($instance in $newObjs) {
            $instance.Id | Should -BeLessOrEqual 0
        }

        $DbContext.AddRange($newObjs)
        $CurrentCount = ([EFPosh.PoshContextInteractions]$DbContext).TableOne.ToList().Count
        { $DbContext.SaveChanges() } | Should -not -Throw
        $NewCount = ([EFPosh.PoshContextInteractions]$DbContext).TableOne.ToList().Count
        $NewCountShouldBe = $CurrentCount + $NumberOfObjects
        $NewCount | Should -be $NewCountShouldBe

        foreach($instance in $newObjs) {
            $instance.Id | Should -BeGreaterThan 0
        }
    }
}