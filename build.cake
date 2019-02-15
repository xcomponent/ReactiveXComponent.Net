#tool "nuget:?package=NUnit.ConsoleRunner&version=3.9.0"
#tool "nuget:?package=ILRepack"
#addin "Cake.FileHelpers&version=3.1.0"
#addin "Cake.Incubator&version=3.0.0"
#addin "Cake.DoInDirectory&version=3.2.0"
#load "cake.scripts/utilities.cake"

var target = Argument("target", "Build");
var buildConfiguration = Argument("buildConfiguration", "Debug");
var configuration = Argument("configuration", "Release");
var version = Argument("buildVersion", "1.0.0-build1");
var vsVersion = Argument("vsVersion", "VS2017");
var apiKey = Argument("nugetKey", "");
var setAssemblyVersion = Argument<bool>("setAssemblyVersion", false);

var XComponentVersion = "6.0.3";

Setup(context=> {
    DoInDirectory(@"tools", () => {
        NuGetInstall("XComponent.Build.Community", new NuGetInstallSettings{ Version=XComponentVersion, ExcludeVersion=true });
        NuGetInstall("XComponent.Studio.Community", new NuGetInstallSettings{ Version=XComponentVersion, ExcludeVersion=true });
    });
});

Task("Clean")
    .Does(() =>
    {
        if (DirectoryExists("nuget"))
        {
            CleanDirectory("nuget");
        }

        if (DirectoryExists("packages"))
        {
            CleanDirectory("packages");
        }

        // CleanSolution("ReactiveXComponent.sln", buildConfiguration);

        var pathHelloWorldIntegrationTest = "./docker/integration_tests/XCProjects/HelloWorldV5/";
        if (DirectoryExists(pathHelloWorldIntegrationTest + "xcr"))
        {
            CleanDirectory(pathHelloWorldIntegrationTest + "xcr");
        }
        if (DirectoryExists(pathHelloWorldIntegrationTest + "generated"))
        {
            CleanDirectory(pathHelloWorldIntegrationTest + "generated");
        }
        if (DirectoryExists(pathHelloWorldIntegrationTest + "rxcAssemblies"))
        {
            CleanDirectory(pathHelloWorldIntegrationTest + "rxcAssemblies");
        }
        if (DirectoryExists(pathHelloWorldIntegrationTest + "CreateInstancesReactiveApi/CreateInstances/xcassemblies"))
        {
            CleanDirectory(pathHelloWorldIntegrationTest + "CreateInstancesReactiveApi/CreateInstances/xcassemblies");
        }
        // CleanSolution(pathHelloWorldIntegrationTest + "CreateInstancesReactiveApi/CreateInstances.sln", buildConfiguration);
    });

Task("Build")
    .Does(() =>
    {
            NuGetRestore("ReactiveXComponent.sln", new NuGetRestoreSettings { NoCache = true });        DotNetCoreBuild(
            @"./ReactiveXComponent.sln",
            new DotNetCoreBuildSettings {
                Configuration = configuration,
            }
        );
    });

Task("Test")
    .Does(() =>
    {
        var testAssembliesPatterns = new string[]
        {
            "./ReactiveXComponentTest/bin/" + buildConfiguration + "/netstandard2.0/ReactiveXComponentTest.dll"
        };
        
        var testAssemblies = GetFiles(testAssembliesPatterns);
        var nunitSettings = new NUnit3Settings(){ Results = new[] { new NUnit3Result { FileName = "TestResults.xml" } } };
        NUnit3(testAssemblies, nunitSettings);
    });

Task("Merge")
    .Does(() =>
    {
        EnsureDirectoryExists("packaging");

        var filesToMerge = GetFiles("./ReactiveXComponent/bin/"+ buildConfiguration + "/netstandard2.0/ReactiveXComponent.dll");

        // var ilRepackSettings = new ILRepackSettings { Parallel = true, Internalize = true };

        // ILRepack(
        //     "./packaging/ReactiveXComponent.dll",
        //     "./ReactiveXComponent/bin/"+ buildConfiguration + "/netstandard2.0/ReactiveXComponent.dll",
        //     filesToMerge,
        //     ilRepackSettings
		// );

        var pdbFiles = GetFiles("./ReactiveXComponent/bin/"+ buildConfiguration + "/netstandard2.0/ReactiveXComponent.pdb");
        CopyFiles(filesToMerge, "./packaging");
        CopyFiles(pdbFiles, "./packaging");
    });

Task("CreatePackage")
    .IsDependentOn("Merge")
    .Does(() =>
    {
        EnsureDirectoryExists("nuget");

        var formattedNugetVersion = FormatNugetVersion(version);

        var filesToPackPatterns = new string[]
        {
            "./packaging/*.dll",
            "./packaging/*.pdb"
        };

        var filesToPack = GetFiles(filesToPackPatterns);

        var nuSpecContents = filesToPack.Select(file => new NuSpecContent {Source = file.FullPath, Target = @"lib\net451"}).ToList();

        var nugetPackSettings = new NuGetPackSettings()
        { 
            OutputDirectory = @"./nuget",
            Files = nuSpecContents,
            Version = formattedNugetVersion,
            IncludeReferencedProjects = true
        };

        NuGetPack("ReactiveXComponent.Net.nuspec", nugetPackSettings);
    });

Task("PushPackage")
    .IsDependentOn("All")
    .Does(() =>
    {
        var formattedNugetVersion = FormatNugetVersion(version);
        if (FileExists("./nuget/ReactiveXComponent.Net." + formattedNugetVersion + ".nupkg")
            && !string.IsNullOrEmpty(apiKey))
        {
            var package = "./nuget/ReactiveXComponent.Net." + formattedNugetVersion + ".nupkg";
            var nugetPushSettings = new NuGetPushSettings 
            {
                Source = "https://www.nuget.org/api/v2/package",
                ApiKey = apiKey
            };

            NuGetPush(package, nugetPushSettings);
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
        var exitCode = 0;
        var helloWorldProjectPathParam = " --project=\"./docker/integration_tests/XCProjects/HelloWorldV5/HelloWorldV5_Model.xcml\"";
    
        var mono = "mono";
        var xcbuild = "./tools/XComponent.Build.Community/tools/XCBuild/xcbuild.exe";
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
    Zip("./tools/XComponent.Build.Community/tools/XCBuild/XCRuntime", "./docker/integration_tests/dockerScripts/XCContainer/XCRuntime.zip");
    Zip("./docker/integration_tests/XCProjects/HelloWorldV5/xcr/xcassemblies", "./docker/integration_tests/dockerScripts/XCContainer/HelloWorldV5XCassemblies.zip");
    Zip("./docker/integration_tests/XCProjects/HelloWorldV5/CreateInstancesReactiveApi/CreateInstances/bin/Debug", "./docker/integration_tests/dockerScripts/AppsContainer/CreateInstanceReactiveApi.zip");
	
});


RunTarget(target);