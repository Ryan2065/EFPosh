$RebuildBinaries = $true

$ModuleLocation = [System.IO.Path]::Combine($PSScriptRoot, "src", "Module", "EFPosh")
$EFPoshProjLocations = [System.IO.Path]::Combine($PSScriptRoot, "src", "EFPosh")
$ErrorActionPreference = 'Stop'
#region Publish

$Dependencies = @('EFPosh', 'EFPosh.EFInteractions' )
$Frameworks = @('net6.0', 'netstandard2.0')

$FilesToCheck = @()
foreach($dep in $Dependencies){
  $FilesToCheck += Get-ChildItem ([System.IO.Path]::Combine($EFPoshProjLocations, $dep)) -File -Filter '*.cs' -Recurse -Depth 2 -ErrorAction SilentlyContinue
}

Function FixDependencyFolder{
  Param([string[]]$FolderPath)
  $copyExtensions = @('.dll', '.pdb')
  $EFPoshFolder = [System.IO.Path]::Combine($ModuleLocation, "Dependencies", "EFPosh")
  $EFPoshInteractionsFolder = [System.IO.Path]::Combine($ModuleLocation, "Dependencies", "EFPosh.EFInteractions")
  foreach($folderp in $FolderPath){
    $EFPoshFolder = [System.IO.Path]::Combine($EFPoshFolder, $folderp)
    $EFPoshInteractionsFolder = [System.IO.Path]::Combine($EFPoshInteractionsFolder,$folderp)
  }

  foreach($file in ( Get-ChildItem  $EFPoshInteractionsFolder -File)){
    if($File.Extension -notin $CopyExtensions){
      $null = Remove-Item $file.FullName -Force
      $null = Remove-Item ( [System.IO.Path]::Combine($EFPoshFolder, $file.Name )) -Force -ErrorAction SilentlyContinue
    }
    elseif( Test-Path ( [System.IO.Path]::Combine($EFPoshFolder, $file.Name )) -ErrorAction SilentlyContinue  ){
      $null = Remove-Item ( [System.IO.Path]::Combine($EFPoshFolder, $file.Name )) -Force
    }
  }
  foreach($file in ( Get-ChildItem  $EFPoshFolder -File -ErrorAction SilentlyContinue)){
    if($File.Extension -notin $CopyExtensions){
      $null = Remove-Item $file.FullName -Force
    }
  }
  foreach($folder in ( Get-ChildItem  $EFPoshInteractionsFolder -Directory)){
    $newFolderPathArray = @($FolderPath)
    $newFolderPathArray += $folder.Name
    FixDependencyFolder -FolderPath $newFolderPathArray
  }
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
    $pushToLocation = [System.IO.Path]::Combine($EFPoshProjLocations, $dep)
    Push-Location $pushToLocation
    dotnet restore
    foreach($Framework in $Frameworks){

      dotnet publish --self-contained false --configuration release --framework $Framework --no-restore
     
      $copyToFolder = [System.IO.Path]::Combine($ModuleDependencyFolder, $dep, $framework)
      
      $null = New-Item $copyToFolder -ItemType Directory -Force
      $copyFromFolder = [System.IO.Path]::Combine($EFPoshProjLocations, $dep, 'bin', 'Release', $framework, 'publish')
      
      $null = Copy-Item -Path "$copyFromFolder\*" -Destination "$copyToFolder\"  -Force -Recurse
    }
    Pop-Location
  }
  FixDependencyFolder -FolderPath 'net6.0'
  
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