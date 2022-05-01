$DebugPreference = 'Continue'
$VerbosePreference = 'Continue'
$InformationPreference = 'Continue'

$env:EFPoshDependencyFolder = "C:\Users\Ryan2\OneDrive\Code\EFPosh\src\EFPosh\EFPosh\bin\Debug\net6.0"

#. C:\Users\Ryan2\OneDrive\Code\EFPosh\src\Tests\_TestOrchestrator.ps1 -SkipBuild
Import-Module C:\users\Ryan2\OneDrive\Code\EFPosh\src\Module\EFPosh -Force
$sqliteFile = "C:\users\Ryan2\OneDrive\Code\EFPosh\src\Tests\bin\EFPoshTestDb.sqlite"
$Global:DebugPreference = 'Continue'
$Global:VerbosePreference = 'Continue'
#Start-EFPoshModel -SqliteFile $sqliteFile -AllEntities -FilePath C:\users\Ryan2\OneDrive\Code\EFPosh\src\Tests\bin\EFPoshTestDbModel.ps1 -Overwrite

$newcontext = . C:\users\Ryan2\OneDrive\Code\EFPosh\src\Tests\bin\EFPoshTestDbModel.ps1

$newcontext.GetEntities()

