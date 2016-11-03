@echo off

pushd %~dp0

Tools\NuGet.exe update -self

Tools\NuGet.exe install FAKE -ConfigFile Tools\Nuget.Config -ExcludeVersion -OutputDirectory packages -Version 4.10.3
Tools\NuGet.exe install nunit.runners -ConfigFile Tools\Nuget.Config -OutputDirectory .\packages\FAKE -ExcludeVersion -Version 2.6.4

set encoding=utf-8
packages\FAKE\tools\FAKE.exe build.fsx %*

popd