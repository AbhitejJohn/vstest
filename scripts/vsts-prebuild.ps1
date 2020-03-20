# Sets variables which are used across the build tasks.

$buildSuffix = $args[0]
$IsRtmBuild = $args[1]

$TP_ROOT_DIR = (Get-Item (Split-Path $MyInvocation.MyCommand.Path)).Parent.FullName

# Set Version from scripts/build/TestPlatform.Settings.targets
$TpVersion = [string](([xml](Get-Content $TP_ROOT_DIR\scripts\build\TestPlatform.Settings.targets)).Project.PropertyGroup.TPVersionPrefix)
$buildPrefix = $TpVersion.Trim()

if ($IsRtmBuild.ToLower() -eq "false") 
{ 
  $packageVersion = $buildPrefix+"-"+$buildSuffix
} 
else 
{
  $packageVersion = $buildPrefix
  $buildSuffix = [string]::Empty
}

Write-Host "##vso[task.setvariable variable=BuildVersionPrefix;]$buildPrefix"
Write-Host "##vso[task.setvariable variable=BuildVersionSuffix;]$buildSuffix"
Write-Host "##vso[task.setvariable variable=PackageVersion;]$packageVersion"

# Set Newtonsoft.Json version to consume in  CI build "Package: TestPlatform SDK" task.
# "Nuget.exe pack" required JsonNetVersion property for creating nuget package.

$JsonNetVersion = ([xml](Get-Content $TP_ROOT_DIR\scripts\build\TestPlatform.Dependencies.props)).Project.PropertyGroup.JsonNetVersion
Write-Host "##vso[task.setvariable variable=JsonNetVersion;]$JsonNetVersion"

$microsoftFakesVersion = ([xml](Get-Content $env:TP_ROOT_DIR\scripts\build\TestPlatform.Dependencies.props)).Project.PropertyGroup.MicrosoftFakesVersion
$FakesPackageDir = Join-Path $env:TP_PACKAGES_DIR "Microsoft.VisualStudio.TestPlatform.Fakes\$microsoftFakesVersion\lib"
Write-Host "##vso[task.setvariable variable=JsonNetVersion;]$FakesPackageDir"