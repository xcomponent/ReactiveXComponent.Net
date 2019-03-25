#tool "nuget:?package=NUnit.ConsoleRunner&version=3.9.0"
#addin "nuget:?package=Cake.DoInDirectory&version=3.2.0"
#addin "Cake.FileHelpers&version=3.1.0"
#addin "Cake.XComponent&version=6.0.1"
#addin "Cake.Incubator&version=3.0.0"
#load "cake.scripts/utilities.cake"

var target = Argument("target", "Build");
var buildConfiguration = Argument("buildConfiguration", "Debug");
var version = Argument("buildVersion", "1.0.0");
var vsVersion = Argument("vsVersion", "VS2017");
var apiKey = Argument("nugetKey", "");
var setAssemblyVersion = Argument<bool>("setAssemblyVersion", false);

Task("Clean")
    .Does(() =>
    {
        CleanDirectory("nuget");
        CleanDirectory("packaging");
        CleanDirectory("./ReactiveXComponent/bin");
        CleanDirectory("./ReactiveXComponent/obj");
        CleanDirectory("./ReactiveXComponentTest/bin");
        CleanDirectory("./ReactiveXComponentTest/obj");

        var pathHelloWorldIntegrationTest = "./docker/integration_tests/XCProjects/HelloWorldV5/";
        CleanDirectory(pathHelloWorldIntegrationTest + "xcr");
        CleanDirectory(pathHelloWorldIntegrationTest + "generated");
        CleanDirectory(pathHelloWorldIntegrationTest + "rxcAssemblies");
        CleanDirectory(pathHelloWorldIntegrationTest + "CreateInstancesReactiveApi/CreateInstances/xcassemblies");
        CleanDirectory(pathHelloWorldIntegrationTest + "CreateInstancesReactiveApi/CreateInstances/bin");
        CleanDirectory(pathHelloWorldIntegrationTest + "CreateInstancesReactiveApi/CreateInstances/obj");
    });

Task("Build")
    .Does(() =>
    {
        DotNetCoreRestore("ReactiveXComponent.sln");
        DotNetCoreBuild(
        "ReactiveXComponent.sln",
        new DotNetCoreBuildSettings {
            Configuration = buildConfiguration,
            VersionSuffix = version,
            MSBuildSettings = new DotNetCoreMSBuildSettings{}.SetVersion(version),
        });
    });

Task("Test")
    .Does(() =>
    {
        var settings = new DotNetCoreTestSettings
        {
            Configuration = buildConfiguration
        };

        var projectFiles = GetFiles("./**/*Test.csproj");
        foreach(var file in projectFiles)
        {
            DotNetCoreTest(file.FullPath, settings);
        }
    });

Task("CreatePackage")
    .Does(() =>
    {
        DotNetCorePack(
            "ReactiveXComponent/ReactiveXComponent.csproj",
            new DotNetCorePackSettings  {
                Configuration = buildConfiguration,
                IncludeSymbols = true,
                NoBuild = true,
                OutputDirectory = @"nuget",
                VersionSuffix = version,
                MSBuildSettings = new DotNetCoreMSBuildSettings{}.SetVersion(version),
            }
        );
    });

Task("PushPackage")
    .Does(() =>
    {
        if (!string.IsNullOrEmpty(apiKey))
        {
            DoInDirectory("./nuget", () =>
            {
                var package = "./ReactiveXComponent.Net." + version + ".nupkg";
                DotNetCoreNuGetPush(package, new DotNetCoreNuGetPushSettings
                {
                    Source = "https://api.nuget.org/v3/index.json",
                    ApiKey = apiKey
                });
            });
        }
        else
        {
            Error("No Api Key provided. Can't deploy package to Nuget.");
        }
    });

Task("All")
  .IsDependentOn("Clean")
  .IsDependentOn("Build")
  .IsDependentOn("Test")
  .IsDependentOn("CreatePackage")
  .Does(() =>
  {
  });

Task("BuildHelloWorld")
    .Does(() =>
    {
        var exitCode = 0;
        var helloWorldProjectPathParam = " --project=\"./docker/integration_tests/XCProjects/HelloWorldV5/HelloWorldV5_Model.xcml\"";
    
        var mono = "mono";
        var xcbuild = GetXcBuildPath();
        var cleanArgs = " --compilationmode=Debug --clean --env=Dev --vs=";
        var buildArgs = " --compilationmode=Debug --build --env=Dev --vs=";

        if (IsRunningOnUnix()) {
            exitCode = StartProcess(mono, xcbuild + cleanArgs + vsVersion + helloWorldProjectPathParam + GetXCBuildExtraParam());
            StartProcess(mono, xcbuild + buildArgs + vsVersion + helloWorldProjectPathParam + GetXCBuildExtraParam());
        } else {
            exitCode =  StartProcess(xcbuild, cleanArgs + vsVersion + helloWorldProjectPathParam + GetXCBuildExtraParam());
            StartProcess(xcbuild, buildArgs + vsVersion + helloWorldProjectPathParam + GetXCBuildExtraParam());
        }
        if (exitCode != 0) {
            throw new Exception();
        }
    });

Task("BuildIntegrationTests")
  .IsDependentOn("BuildHelloWorld")
  .Does(() =>
  {
    var rxcAssembliesPatterns = new string[]
    {
        "./packaging/ReactiveXComponent.dll"
    };

    var pathTIDirectory = "./docker/integration_tests/XCProjects/HelloWorldV5/CreateInstancesReactiveApi";
    var pathrxcAssembliesDirectory = "./docker/integration_tests/XCProjects/HelloWorldV5/rxcAssemblies";
    var rxcAssemblies = GetFiles(rxcAssembliesPatterns);

    CreateDirectory(pathrxcAssembliesDirectory);
    CopyFiles(rxcAssemblies, pathrxcAssembliesDirectory);

    DotNetCoreRestore(pathTIDirectory + "/CreateInstances.sln");
    DotNetCoreBuild(
    pathTIDirectory + "/CreateInstances.sln",
    new DotNetCoreBuildSettings {
        Configuration = buildConfiguration,
    });
  });

Task("PackageDockerIntegrationTests")
  .Does(() =>
 {
    Zip( GetXcRuntimeDirectory(), "./docker/integration_tests/dockerScripts/XCContainer/XCRuntime.zip");
    Zip("./docker/integration_tests/XCProjects/HelloWorldV5/xcr/xcassemblies", "./docker/integration_tests/dockerScripts/XCContainer/HelloWorldV5XCassemblies.zip");
    Zip("./docker/integration_tests/XCProjects/HelloWorldV5/CreateInstancesReactiveApi/CreateInstances/bin/" + buildConfiguration + "/netcoreapp2.1", "./docker/integration_tests/dockerScripts/AppsContainer/CreateInstanceReactiveApi.zip");	
});


RunTarget(target);