# 
# Copyright (c) 2011-2012, Toji Project Contributors
# 
# Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL).
# See the file LICENSE.txt for details.
# 

# This file is where you can override any settings specified in the other scripts.
# These settings can also be used to override conventions such as solution name and nuspec file.

properties {
  Write-Output "Loading override settings"

  $solution.name = "Talifun-Web"


    $talifunweb = ($solution.name -replace "-", ".") + ".dll"
    $talifunwebAssemblies = @("Talifun.Web.dll", "Talifun.FileWatcher.dll","AjaxMin.dll", "dotless.ClientOnly.dll", "EcmaScript.NET.dll", "Iesi.Collections.dll", "Yahoo.Yui.Compressor.dll")
    $ilmerge.targets["$talifunweb"] = @(Get-ChildItem -path "$($build.dir)\*.dll" | Where-Object {$talifunwebAssemblies -contains $_.Name} | Select-Object $_.FullName)

    #$talifunwebmsbuild = "Talifun.Crusher.MsBuild.dll"
    #$talifunwebmsbuildAssemblies = @("Talifun.Crusher.MsBuild.dll","Talifun.Web.dll", "Talifun.FileWatcher.dll","AjaxMin.dll", "dotless.ClientOnly.dll", "EcmaScript.NET.dll", "Iesi.Collections.dll", "Yahoo.Yui.Compressor.dll")
    #$ilmerge.targets["$talifunwebmsbuild"] = @(Get-ChildItem -path "$($build.dir)\*.dll" | Where-Object {$talifunwebmsbuildAssemblies -contains $_.Name} | Select-Object $_.FullName)    

}