Param(
    [string]$DbType, [object]$DbContext, [hashtable]$ContextParams
)

BeforeAll{
    0..100 | ForEach-Object{
        $NewTableEntity = New-EFPoshEntity -DbContext $DbContext -Entity 'TestTableOne'
        $NewTableEntity.Name = "TestEntityForSearching$($_)"
        
        Add-EFPoshEntity -DbContext $DbContext -Entity $NewTableEntity -SaveChanges
        
        $NewTableThreeEntity = New-EFPoshEntity -DbContext $DbContext -Entity 'TestTableThree'
        $NewTableThreeEntity.Name = "TestTableThreeEntityForSearching$($_)"

        Add-EFPoshEntity -DbContext $DbContext -Entity $NewTableThreeEntity -SaveChanges
        
        $NewTableTwoEntity = New-EFPoshEntity -DbContext $DbContext -Entity 'TestTableTwo'
        $NewTableTwoEntity.Name = "TestTableTwoEntityForSearching$($_)"
        $NewTableTwoEntity.TableOneId = $NewTableEntity.Id
        $NewTableTwoEntity.TableThreeId = $NewTableThreeEntity.Id
        Add-EFPoshEntity -DbContext $DbContext -Entity $NewTableTwoEntity
    }
    Save-EFPoshChanges -DbContext $DbContext
}

Describe "Search <DbType>" {

    Context "Validate crazy search params"{
        BeforeEach{
            $DbContext = New-EFPoshContext -ConnectionString $DbContext.ConnectionString -DbType $DbType @ContextParams
        }
        It 'Should be able to query with arrays definied in query'{
            $results = Search-EFPosh -DbContext $DbContext -Entity TestTableOne -Expression { @('TestEntityForSearching1','TestEntityForSearching2') -Contains $_.Name } -ToList
            $results.Count | Should -Be 2
        }
        It 'Should be able to query with arrays defined outside the query'{
            $arr =  @('TestEntityForSearching1','TestEntityForSearching2')
            $results = Search-EFPosh -DbContext $DbContext -Entity TestTableOne -Expression { $arr -Contains $_.Name } -ToList
            $results.Count | Should -Be 2
        }
        It 'Should be able to query with methods on arrays defined in the query'{
            $results = Search-EFPosh -DbContext $DbContext -Entity TestTableOne -Expression {  @('TestEntityForSearching1','TestEntityForSearching2').Contains($_.Name) } -ToList
            $results.Count | Should -Be 2
        }
        It 'Should be able to query with methods on arrays defined in the query'{
            $arr =  @('TestEntityForSearching1','TestEntityForSearching2')
            $results = Search-EFPosh -DbContext $DbContext -Entity TestTableOne -Expression {  $arr.Contains($_.Name) } -ToList
            $results.Count | Should -Be 2
        }
        It 'Should be able to handle inputs in hashtables'{
            $param = @{
                'a' = 'b'
                'c' = 'TestEntityForSearching1'
            }
            $results = Search-EFPosh -DbContext $DbContext -Entity TestTableOne -Expression {  $_.Name -eq $param['c'] } -ToList
            $results.Count | Should -Be 1
        }
        It 'Should be able to handle inputs in hashtables indexed with a variable'{
            $param = @{
                'a' = 'b'
                'c' = 'TestEntityForSearching1'
            }
            $indexVar = 'c'
            $results = Search-EFPosh -DbContext $DbContext -Entity TestTableOne -Expression {  $_.Name -eq $param[$indexVar] } -ToList
            $results.Count | Should -Be 1
        }
    }
    Context "Validate standard searchs"{
        BeforeEach{
            $DbContext = New-EFPoshContext -ConnectionString $DbContext.ConnectionString -DbType $DbType @ContextParams
        }
        It 'Should be able to search using greater than with ands'{
            $results = Search-EFPosh -DbContext $DbContext -Entity TestTableOne -Expression { $_.Name -like "TestEntityForSearching99" -and $_.Id -gt 90 } -ToList
            $results.Count | Should -Be 1
        }
        It 'Should be able to search reference properties'{
            $results = Search-EFPosh -DbContext $DbContext -Entity TestTableOne -Expression { $_.TableTwo.Name -like 'TestTableTwoEntityForSearching5' } -ToList
            $results.Count | SHould -Be 1
        }
        It 'Should be able to get reference properties'{
            $result = Search-EFPosh -DbContext $DbContext -Entity TestTableOne -Expression { $_.Name -like "TestEntityForSearching99" } -FirstOrDefault
            $result.TableTwo.Name | Should -BeNullOrEmpty
            $result = Search-EFPosh -DbContext $DbContext -Entity TestTableOne -Expression { $_.Name -like "TestEntityForSearching99" } -FirstOrDefault -Include 'TableTwo'
            $result.TableTwo.Name | Should -Not -BeNullOrEmpty
        }
        It 'Should be able to only return specific properties'{
            $result = Search-EFPosh -DbContext $DbContext -Entity TestTableOne -Expression { $_.Name -like "TestEntityForSearching99" } -FirstOrDefault -Select 'Name'
            $result.Id | Should -be 0 # id is not nullable so 0 is default, meaning it was not returned
            $result.Name | Should -not -BeNullOrEmpty
        }
        It 'Should be able to get a reference properties reference property'{
            $result = Search-EFPosh -DbContext $DbContext -Entity TestTableOne -Expression { $_.Name -like "TestEntityForSearching99" } -FirstOrDefault
            $result.TableTwo.Name | Should -BeNullOrEmpty
            $result = Search-EFPosh -DbContext $DbContext -Entity TestTableOne -Expression { $_.Name -like "TestEntityForSearching99" } -FirstOrDefault -Include 'TableTwo' -ThenInclude 'TableThree'
            $result.TableTwo.Name | Should -Not -BeNullOrEmpty
            $result.TableTwo.TableThree.Name | Should -Not -BeNullOrEmpty
        }
    }
}