param(
  [Parameter(Position=0,Mandatory=0)]
  [string]$buildFile = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)\build\build.ps1",
  [Parameter(Position=1,Mandatory=0)]
  [string[]]$taskList = @(),
  [Parameter(Position=2,Mandatory=0)]
  [string]$framework = '4.0',
  [Parameter(Position=3,Mandatory=0)]
  [switch]$docs = $false,
  [Parameter(Position=4,Mandatory=0)]
  [System.Collections.Hashtable]$parameters = @{},
  [Parameter(Position=5, Mandatory=0)]
  [System.Collections.Hashtable]$properties = @{}
)

$rootPath = (Split-Path -parent $MyInvocation.MyCommand.Definition)
$buildPath = (Resolve-Path $rootPath\build)

$psakeLocations = @(Get-ChildItem $rootPath\build\* -recurse -include psake.ps1)
$psakeModule = $psakeLocations[0].FullName
$psakeModuleDirectory = $psakeLocations[0].DirectoryName

. $buildPath\bootstrap.ps1 $buildPath
. $psakeModule -buildFile $buildFile -taskList $taskList -framework '4.5' -docs:$docs -parameters $parameters -properties $properties -scriptPath $psakeModuleDirectory
. $psakeModule -buildFile $buildFile -taskList $taskList -framework '4.0' -docs:$docs -parameters $parameters -properties $properties -scriptPath $psakeModuleDirectory
. $psakeModule -buildFile $buildFile -taskList $taskList -framework '3.5' -docs:$docs -parameters $parameters -properties $properties -scriptPath $psakeModuleDirectory
. $psakeModule -buildFile $buildFile -taskList Package -framework $framework -docs:$docs -parameters $parameters -properties $properties -scriptPath $psakeModuleDirectory


if($env:BUILD_NUMBER) {
  [Environment]::Exit($lastexitcode)
} else {
  exit $lastexitcode
}