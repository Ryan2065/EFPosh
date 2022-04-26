$RebuildBinaries = $true

$ModuleLocation = [System.IO.Path]::Combine($PSScriptRoot, "src", "Module", "EFPosh")
$EFPoshProjLocations = [System.IO.Path]::Combine($PSScriptRoot, "src", "EFPosh")
$ErrorActionPreference = 'Stop'
#region Publish

$Dependencies = @('EFPosh', 'EFPosh.EFInteractions')
$Frameworks = @('net6.0', 'netstandard2.0')

$FilesToCheck = @()
foreach($dep in $Dependencies){
  $FilesToCheck += Get-ChildItem ([System.IO.Path]::Combine($EFPoshProjLocations, $dep)) -File -Filter '*.cs' -Recurse -Depth 2 -ErrorAction SilentlyContinue
}


$LastBuildTimeFile = [System.IO.Path]::Combine($($env:temp), "EFPoshLastBuildTime.txt")
if(Test-Path $LastBuildTimeFile -ErrorAction SilentlyContinue){
  try{
    $LastBuildTime = Get-Content $LastBuildTimeFile | ConvertFrom-JSON
    $RebuildBinaries = $false
    foreach($file in $FilesToCheck){
      if($LastBuildTime -lt $file.LastWriteTime){
        $RebuildBinaries = $true
      }
    }
  }
  catch{}
  
}

if($RebuildBinaries){
  $ModuleDependencyFolder = [System.IO.Path]::Combine($ModuleLocation, "Dependencies")
  if(Test-Path $ModuleDependencyFolder){
    Remove-Item $ModuleDependencyFolder -Force -Recurse
  }

  $null = New-Item $ModuleDependencyFolder -ItemType Directory -Force

  foreach($dep in $Dependencies){
    Push-Location ([System.IO.Path]::Combine($EFPoshProjLocations, $dep))
    dotnet restore
    foreach($Framework in $Frameworks){
      Get-Location
      
      dotnet publish --self-contained false --configuration release --framework $Framework --no-restore
      $copyToFolder = [System.IO.Path]::Combine($ModuleDependencyFolder, $dep, $framework)
      
      $null = New-Item $copyToFolder -ItemType Directory -Force
      $copyFromFolder = [System.IO.Path]::Combine($EFPoshProjLocations, $dep, 'bin', 'Release', $framework, 'publish')
      $null = Copy-Item -Path "$copyFromFolder\*" -Destination "$copyToFolder\"  -Force -Recurse
      Pop-Location
    }
  }
  <#$refFolderPath = [System.IO.Path]::Combine($Net6Folder, 'ref')
  if(Test-Path $refFolderPath){
      Remove-Item $refFolderPath -Force -Recurse
  }
  
  $RuntimeFoldersToKeep = @(
      'win-x64',
      'win-x86',
      'linux-x64',
      'osx',
      'win'
  )
  $RuntimeFolders = Get-ChildItem ([System.IO.Path]::Combine($Net6Folder, 'runtimes')) -Directory
  foreach($RuntimeFolder in $RuntimeFolders){
      if($RuntimeFoldersToKeep -notcontains $RuntimeFolder.BaseName){
          Remove-Item $RuntimeFolder.FullName -Recurse -Force
      }
  }#>
  [DateTime]::Now | ConvertTo-JSON | Out-File $LastBuildTimeFile -Force
}

#endregion
return
#region handle increasing module version
$ModulePSD = [System.IO.Path]::Combine($ModuleLocation, 'EFPosh.psd1')
$CurrentVersion = (Get-Module $ModuleLocation -ListAvailable).Version

if($null -eq $env:GITHUB_RUN_NUMBER){
    $env:GITHUB_RUN_NUMBER = $CurrentVersion.Build + 1
}

$LastPublishedVersion = (Find-Module -Name EFPosh -AllowPrerelease).Version
$LastPublishedVersion = $LastPublishedVersion.Split('-')[0]

$NewBuild = ([Version]$LastPublishedVersion).Build + 1
$NewVersion = "$($CurrentVersion.Major).$($CurrentVersion.Minor).$($NewBuild)"

(Get-Content $ModulePSD -Raw).Trim().Replace("'$CurrentVersion'", "'$($NewVersion)'") | Out-File $ModulePSD -Force
#endregion

Pop-Location