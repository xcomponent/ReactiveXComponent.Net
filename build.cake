#tool "nuget:?package=NUnit.ConsoleRunner&version=3.9.0"
#addin "Cake.FileHelpers&version=3.1.0"
#addin "Cake.XComponent&version=6.0.0"
#addin "Cake.Incubator&version=3.0.0"
#load "cake.scripts/utilities.cake"

var target = Argument("target", "Build");
var buildConfiguration = Argument("buildConfiguration", "Debug");
var version = Argument("buildVersion", "1.0.0-build1");
var vsVersion = Argument("vsVersion", "VS2017");
var apiKey = Argument("nugetKey", "");
var setAssemblyVersion = Argument<bool>("setAssemblyVersion", false);

Setup (context => {
    var formattedAssemblyVersion = FormatAssemblyVersion(version);
    Information("Resolving value {0} for tag {1} in source code", formattedAssemblyVersion, "[PRODUCT_VERSION]");

    ReplaceTextInFiles("./ReactiveXComponent/Properties/AssemblyInfo.cs", "[PRODUCT_VERSION]", formattedAssemblyVersion);
});

Teardown (context => {
    
    var formattedAssemblyVersion = FormatAssemblyVersion(version);
    Information("Reversing value {0} for tag {1} in source code", formattedAssemblyVersion, "[PRODUCT_VERSION]");

    ReplaceTextInFiles("./ReactiveXComponent/Properties/AssemblyInfo.cs", formattedAssemblyVersion, "[PRODUCT_VERSION]");
});

Task("Clean")
    .Does(() =>
    {
        CleanDirectory("nuget");
        CleanDirectory("packages");
        CleanDirectory("packaging");
        CleanDirectory("./ReactiveXComponent/bin");
        CleanDirectory("./ReactiveXComponent/obj");
        CleanDirectory("./ReactiveXComponentTest/bin");
        CleanDirectory("./ReactiveXComponenttest/obj");

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
        var formattedNugetVersion = FormatNugetVersion(version);
        DotNetCorePack(
            "ReactiveXComponent/ReactiveXComponent.csproj",
            new DotNetCorePackSettings  {
                Configuration = buildConfiguration,
                OutputDirectory = @"nuget",
                VersionSuffix = formattedNugetVersion,
                MSBuildSettings = new DotNetCoreMSBuildSettings{}.SetVersion(formattedNugetVersion),
            }
        );
    });

Task("PushPackage")
    .IsDependentOn("All")
    .Does(() =>
    {
        var formattedNugetVersion = FormatNugetVersion(version);
        if (!string.IsNullOrEmpty(apiKey))
        {
            var package = "./nuget/ReactiveXComponent.Net." + formattedNugetVersion + ".nupkg";
            DotNetCoreNuGetPush(package, new DotNetCoreNuGetPushSettings 
            {
                Source = "https://www.nuget.org/api/v2/package",
                ApiKey = apiKey
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
        NuGetRestore("./docker/integration_tests/XCProjects/HelloWorldV5/CreateInstancesReactiveApi/CreateInstances.sln", new NuGetRestoreSettings { NoCache = true });
        var helloWorldProjectPathParam = " --project=\"./docker/integration_tests/XCProjects/HelloWorldV5/HelloWorldV5_Model.xcml\"";
    
        XcBuildBuild(helloWorldProjectPathParam, buildConfiguration, "Dev", vsVersion, GetXCBuildExtraParam());
    });

Task("BuildIntegrationTests")
  .IsDependentOn("BuildHelloWorld")
  .Does(() =>
  {
    var rxcAssembliesPatterns = new string[]
    {
        "./packaging/ReactiveXComponent.dll"
    };

    var pathrxcAssembliesDirectory = "./docker/integration_tests/XCProjects/HelloWorldV5/rxcAssemblies";
    var rxcAssemblies = GetFiles(rxcAssembliesPatterns);

    CreateDirectory(pathrxcAssembliesDirectory);
    CopyFiles(rxcAssemblies, pathrxcAssembliesDirectory);
    var buildSettings = new Settings { Configuration = buildConfiguration, VSVersion = vsVersion };

    CrossPlatformBuild(@"./docker/integration_tests/XCProjects/HelloWorldV5/CreateInstancesReactiveApi/CreateInstances.sln", buildSettings);
  });

Task("PackageDockerIntegrationTests")
  .Does(() =>
 {
    Zip( GetXcRuntimePath().Replace("xcruntime.exe", ""), "./docker/integration_tests/dockerScripts/XCContainer/XCRuntime.zip");
    Zip("./docker/integration_tests/XCProjects/HelloWorldV5/xcr/xcassemblies", "./docker/integration_tests/dockerScripts/XCContainer/HelloWorldV5XCassemblies.zip");
    Zip("./docker/integration_tests/XCProjects/HelloWorldV5/CreateInstancesReactiveApi/CreateInstances/bin/" + buildConfiguration, "./docker/integration_tests/dockerScripts/AppsContainer/CreateInstanceReactiveApi.zip");
	
});


RunTarget(target);