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

$ConString = $env:AzureConString

Import-Module $ModuleLocation -Force

. "$PSScriptRoot\src\TestModule.ps1"