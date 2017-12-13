#tool "nuget:?package=NUnit.Runners&version=3.7.0&include=./**/*"
#tool "nuget:?package=ILRepack"
#addin "Cake.FileHelpers&version=2.0.0"
#addin "Cake.Incubator&version=1.6.0"
#load "cake.scripts/utilities.cake"

var target = Argument("target", "Build");
var buildConfiguration = Argument("buildConfiguration", "Release");
var version = Argument("buildVersion", "1.0.0-build1");
var apiKey = Argument("nugetKey", "");
var setAssemblyVersion = Argument<bool>("setAssemblyVersion", false);

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

        CleanSolution("ReactiveXComponent.sln", buildConfiguration);
    });

Task("RestoreNugetPackages")
    .Does(() =>
    {
        NuGetRestore("ReactiveXComponent.sln", new NuGetRestoreSettings { NoCache = true });
    });

Task("Build")
    .Does(() =>
    {
        BuildSolution(@"./ReactiveXComponent.sln", buildConfiguration, setAssemblyVersion, version);
    });

Task("Test")
    .Does(() =>
    {
        var testAssembliesPatterns = new string[]
        {
            "./ReactiveXComponentTest/bin/" + buildConfiguration + "/ReactiveXComponentTest.dll"
        };
        
        var testAssemblies = GetFiles(testAssembliesPatterns);
        var nunitSettings = new NUnit3Settings(){ Results = new[] { new NUnit3Result { FileName = "TestResults.xml" } } };
        NUnit3(testAssemblies, nunitSettings);
    });

Task("Merge")
    .Does(() =>
    {
        EnsureDirectoryExists("packaging");

        var filesToMerge = GetFiles("./ReactiveXComponent/bin/"+ buildConfiguration + "/*.dll");

        var ilRepackSettings = new ILRepackSettings { Parallel = true, Internalize = true };

        ILRepack(
            "./packaging/ReactiveXComponent.dll",
            "./ReactiveXComponent/bin/"+ buildConfiguration + "/ReactiveXComponent.dll",
            filesToMerge,
            ilRepackSettings
		);

        var pdbFiles = GetFiles("./ReactiveXComponent/bin/"+ buildConfiguration + "/ReactiveXComponent.pdb");
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
  .IsDependentOn("RestoreNugetPackages")
  .IsDependentOn("Build")
  .IsDependentOn("Test")
  .IsDependentOn("CreatePackage")
  .Does(() =>
  {
  });

RunTarget(target);