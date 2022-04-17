$RebuildBinaries = $true

$ModuleLocation = [System.IO.Path]::Combine($PSScriptRoot, "src", "Module", "EFPosh")
$EFPoshProjLocation = [System.IO.Path]::Combine($PSScriptRoot, "src", "EFPosh", "EFPosh")


#region Publish

$BinaryExpConvertProjLocation = [System.IO.Path]::Combine($PSScriptRoot, "src", "EFPosh", "BinaryExpressionConverter")

$LastBuildTimeFile = [System.IO.Path]::Combine($($env:temp), "EFPoshLastBuildTime.txt")
if(Test-Path $LastBuildTimeFile -ErrorAction SilentlyContinue){
  try{
    $LastBuildTime = Get-Content $LastBuildTimeFile | ConvertFrom-JSON
    $FilesToCheck = Get-ChildItem $EFPoshProjLocation -File
    $FilesToCheck += Get-ChildItem $BinaryExpConvertProjLocation -File
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
  Push-Location $EFPoshProjLocation

  dotnet restore
  
  dotnet publish --self-contained false --configuration release --framework net472 --no-restore
  dotnet publish --self-contained false --configuration release --framework net6.0 --no-restore 
  
  $ModuleDependencyFolder = [System.IO.Path]::Combine($ModuleLocation, "Dependencies")
  
  if(Test-Path $ModuleDependencyFolder){
      Remove-Item $ModuleDependencyFolder -Force -Recurse
  }
  
  $Net47Folder = [System.IO.Path]::Combine($ModuleDependencyFolder, "net472")
  $Net6Folder = [System.IO.Path]::Combine($ModuleDependencyFolder, "net6.0")
  
  $null = New-Item $ModuleDependencyFolder -ItemType Directory -Force
  $null = New-Item $Net47Folder -ItemType Directory -Force
  $null = New-Item $Net6Folder -ItemType Directory -Force
  
  $Net47PublishFolder = [System.IO.Path]::Combine($EFPoshProjLocation, 'bin', 'Release', 'net472', 'publish')
  $Net6PublishFolder = [System.IO.Path]::Combine($EFPoshProjLocation, 'bin', 'Release', 'net6.0', 'publish')
  $null = Copy-Item -Path "$Net47PublishFolder\*" -Destination "$Net47Folder\"  -Force -Recurse
  $null = Copy-Item -Path "$Net6PublishFolder\*" -Destination "$Net6Folder\"  -Force -Recurse
  
  $refFolderPath = [System.IO.Path]::Combine($Net6Folder, 'ref')
  if(Test-Path $refFolderPath){
      Remove-Item $refFolderPath -Force -Recurse
  }
  
  $RuntimeFoldersToKeep = @(
      'win-x64',
      'win-x86',
      'linux-x64',
      'osx'
  )
  $RuntimeFolders = Get-ChildItem ([System.IO.Path]::Combine($Net6Folder, 'runtimes')) -Directory
  foreach($RuntimeFolder in $RuntimeFolders){
      if($RuntimeFoldersToKeep -notcontains $RuntimeFolder.BaseName){
          Remove-Item $RuntimeFolder.FullName -Recurse -Force
      }
  }
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