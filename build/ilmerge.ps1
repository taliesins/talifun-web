# 
# Copyright (c) 2012, Toji Project Contributors
# 
# Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL).
# See the file LICENSE.txt for details.
# 

properties {
  Write-Output "Loading ilmerge properties"

  $ilmerge = @{}
  $ilmerge.file = (Resolve-Ilmerge)

  $ilmerge.key = "$($source.dir)\$($solution.name)\$($solution.name).snk"
  if(!(Test-Path($ilmerge.key)))
  {
    $ilmerge.key = ""
  }  
  else
  {
    $ilmerge.key = "/keyfile:`"$($ilmerge.key)`""
  }

  $ilmerge.directory = $build.dir
  $ilmerge.targets = @{}
}

Task Create-IlmergePackage {
Write-Output "Create-IlmergePackage"
  Assert (![string]::IsNullOrEmpty($ilmerge.file) -and (Test-Path($ilmerge.file))) "The location of the ilmerge exe must be specified."
  Assert (Test-Path($ilmerge.file)) "Could not find ilmerge exe"

  if ($ilmerge.targets.length -lt 1)
  {
    $talifunweb = ($solution.name -replace "-", ".") + ".dll"
    $talifunwebAssemblies = @("Talifun.Web.dll", "Talifun.FileWatcher.dll","AjaxMin.dll", "dotless.ClientOnly.dll", "EcmaScript.NET.dll", "Iesi.Collections.dll", "Yahoo.Yui.Compressor.dll")
    $ilmerge.targets["$talifunweb"] = @(Get-ChildItem -path "$($build.dir)\*.dll" | Where-Object {$talifunwebAssemblies -contains $_.Name} | Select-Object $_.FullName)

    #$talifunwebmsbuild = "Talifun.Crusher.MsBuild.dll"
    #$talifunwebmsbuildAssemblies = @("Talifun.Crusher.MsBuild.dll","Talifun.Web.dll", "Talifun.FileWatcher.dll","AjaxMin.dll", "dotless.ClientOnly.dll", "EcmaScript.NET.dll", "Iesi.Collections.dll", "Yahoo.Yui.Compressor.dll")
    #$ilmerge.targets["$talifunwebmsbuild"] = @(Get-ChildItem -path "$($build.dir)\*.dll" | Where-Object {$talifunwebmsbuildAssemblies -contains $_.Name} | Select-Object $_.FullName)    
  }

  $temp_directory = $ilmerge.directory+"\merged"

  if(!(Test-Path($temp_directory))) 
  { 
    Write-Output "Creating merge directory $temp_directory"
    new-item $temp_directory -itemType directory | Out-Null 
  }

  $ilmerge_targetPath = $temp_directory
  Write-Output "Moving into $ilmerge_targetPath"
  Push-Location $ilmerge_targetPath
  try {
    Foreach ($ilmerge_target in $ilmerge.targets.keys)
    {
      $ilmerge_name = $ilmerge_target
      $assemblies = $ilmerge.targets[$ilmerge_target]
      $targetPlatform = ""
      Write-Output "$ilmerge_name = $assemblies"
      if($framework -eq "4.0")
      {
        $targetPlatform = "/targetplatform:'v4,C:\Windows\Microsoft.NET\Framework\v4.0.30319'"
      } 
      else
      {
        $targetPlatform = "/targetplatform:'v2,C:\Windows\Microsoft.NET\Framework\v2.0.50727'"
      }

      $ilmerge.command = "& $($ilmerge.file) $($ilmerge.key) /ver:$($build.version) /out:`"$($temp_directory)\$($ilmerge_name)`" $($assemblies) $($targetPlatform)"

      $message = "Error executing command: {0}"
      $command = "Invoke-Expression $($ilmerge.command)"
      $errorMessage = $message -f $command
      exec { Invoke-Expression $ilmerge.command } $errorMessage   
    }
  } finally { Pop-Location }
}

Task Clean-IlmergePackage {
  Remove-Item "$($ilmerge.directory)\temp_merge" -Recurse -ErrorAction SilentlyContinue
}