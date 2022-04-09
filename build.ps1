Function Join-MultiplePaths{
    Param(
        [string[]]$Paths
        )
    if($paths.Count -lt 2){
      return $Paths
    }
    $newPath = Join-Path $Paths[0] $Paths[1]
    if($paths.count -gt 2){
      $newPathArray = @($newPath)
      for($i = 2; $i -lt $paths.count; $i++){
        $newPathArray += $paths[$i]
      }
      Join-MultiplePaths $newPathArray
    }
    else{
      $newPath
    }
}

$ModuleLocation = Join-MultiplePaths @($PSScriptRoot, "src", "Module", "EFPosh")
$EFPoshProjLocation = Join-MultiplePaths @($PSScriptRoot, "src", "EFPosh", "EFPosh")

#region Publish

Push-Location $EFPoshProjLocation

dotnet restore

dotnet publish --self-contained false --configuration release --framework net472 --no-restore
dotnet publish --self-contained false --configuration release --framework net6.0 --no-restore 

$ModuleDependencyFolder = Join-Path $ModuleLocation "Dependencies"

if(Test-Path $ModuleDependencyFolder){
    Remove-Item $ModuleDependencyFolder -Force -Recurse
}

$Net47Folder = Join-Path $ModuleDependencyFolder "net472"
$Net6Folder = Join-Path $ModuleDependencyFolder "net6.0"

$null = New-Item $ModuleDependencyFolder -ItemType Directory -Force
$null = New-Item $Net47Folder -ItemType Directory -Force
$null = New-Item $Net6Folder -ItemType Directory -Force

$Net47PublishFolder = Join-MultiplePaths @($EFPoshProjLocation, 'bin', 'Release', 'net472', 'publish')
$Net6PublishFolder = Join-MultiplePaths @($EFPoshProjLocation, 'bin', 'Release', 'net6.0', 'publish')
$null = Copy-Item -Path "$Net47PublishFolder\*" -Destination "$Net47Folder\"  -Force -Recurse
$null = Copy-Item -Path "$Net6PublishFolder\*" -Destination "$Net6Folder\"  -Force -Recurse

$refFolderPath = Join-Path $Net6Folder 'ref'
if(Test-Path $refFolderPath){
    Remove-Item $refFolderPath -Force -Recurse
}

$RuntimeFoldersToKeep = @(
    'win-x64',
    'win-x86',
    'linux-x64',
    'osx'
)
$RuntimeFolders = Get-ChildItem (Join-Path $Net6Folder 'runtimes') -Directory
foreach($RuntimeFolder in $RuntimeFolders){
    if($RuntimeFoldersToKeep -notcontains $RuntimeFolder.BaseName){
        Remove-Item $RuntimeFolder.FullName -Recurse -Force
    }
}
#endregion

#region handle increasing module version
if($null -eq $env:GITHUB_RUN_NUMBER){
    $env:GITHUB_RUN_NUMBER = 0
}

$ModulePSD = Join-Path $ModuleLocation 'EFPosh.psd1'

$CurrentVersion = (Get-Module $ModuleLocation -ListAvailable).Version
$NewVersion = "$($CurrentVersion.Major).$($CurrentVersion.Minor).$($env:GITHUB_RUN_NUMBER)"

(Get-Content $ModulePSD -Raw).Replace("'$CurrentVersion'", "'$($NewVersion)'") | Out-File $ModulePSD -Force
#endregion