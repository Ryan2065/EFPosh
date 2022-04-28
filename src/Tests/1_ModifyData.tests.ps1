Param(
    [string]$DbType, [object]$DbContext
)

Describe "Modify <DbType>" {
    Context "Add data"{
        It 'Should add data to table one and three'{

            #Creates a new entity
            $NewTableEntity = New-EFPoshEntity -DbContext $DbContext -Entity 'TestTableOne'
            $NewTableEntity.Name = 'My test name'
            #Adds entity to context but doesn't update DB yet (-SaveChanges needs to be specified for that)
            {Add-EFPoshEntity -DbContext $DbContext -Entity $NewTableEntity } | SHould -not -Throw
            
            # Verifies it is not in the DB yet
            $foundItem = Search-EFPosh -Entity $DbContext.TestTableOne -Expression { $_.Name -eq 'My test name' } -FirstOrDefault
            $foundItem | Should -BeNullOrEmpty

            #Adds another entity
            $NewTableEntity = New-EFPoshEntity -DbContext $DbContext -Entity 'TestTableThree'
            $NewTableEntity.Name = 'My test table 3 name'
            {Add-EFPoshEntity -DbContext $DbContext -Entity $NewTableEntity } | SHould -not -Throw
            
            # Verifies it is not in the DB yet
            $foundItem = Search-EFPosh -Entity $DbContext.TestTableThree -Expression { $_.Name -eq 'My test table 3 name' } -FirstOrDefault
            $foundItem | Should -BeNullOrEmpty

            #Save changes will save both
            Save-EFPoshChanges -DbContext $DbContext

            # Validates it is there now
            $foundItem = Search-EFPosh -Entity $DbContext.TestTableOne -Expression { $_.Name -eq 'My test name' } -FirstOrDefault
            $foundItem | Should -Not -BeNullOrEmpty
            
            $foundItem = Search-EFPosh -Entity $DbContext.TestTableThree -Expression { $_.Name -eq 'My test table 3 name' } -FirstOrDefault
            $foundItem | Should -Not -BeNullOrEmpty
        }
        It 'Should be able to add data to table two with FK reference to table one'{
            $NewTableEntity = New-EFPoshEntity -DbContext $DbContext -Entity 'TestTableTwo'
            $NewTableEntity.Name = 'My test name for table two'
            $NewTableEntity.TableOneId = ( Search-EFPosh -Entity $DbContext.TestTableOne -FirstOrDefault ).Id
            $NewTableEntity.TableThreeId = ( Search-EFPosh -Entity $DbContext.TestTableThree -FirstOrDefault ).Id
            {Add-EFPoshEntity -DbContext $DbContext -Entity $NewTableEntity -SaveChanges } | SHould -not -Throw
        }
    }
    Context "Remove data"{
        It 'Should be able to remove data'{
            $item = ( Search-EFPosh -Entity $DbContext.TestTableOne -FirstOrDefault )
            $itemId = $item.Id
            Remove-EFPoshEntity -DbContext $DbContext -Entity $item -SaveChanges
            ( Search-EFPosh -Entity $DbContext.TestTableOne -Expression { $_.Id -eq $itemId } -FirstOrDefault ) | Should -BeNullOrEmpty
        }
    }
}