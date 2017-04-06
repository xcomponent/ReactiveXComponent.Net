#tool "nuget:?package=NUnit.Runners&version=2.6.4"
#addin "Cake.FileHelpers"
#addin "Cake.Incubator"
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
        BuildSolution(@"./ReactiveXComponent.sln", buildConfiguration, setAssemblyVersion);
    });

Task("Test")
    .Does(() =>
    {
        var testAssembliesPatterns = new string[]
        {
            "./ReactiveXComponentTest/bin/" + buildConfiguration + "/ReactiveXComponentTest.dll"
        };
        
        var testAssemblies = GetFiles(testAssembliesPatterns);
        var nunitSettings = new NUnitSettings(){ ResultsFile = "TestResults.xml" };
        NUnit(testAssemblies, nunitSettings);
    });

Task("CreatePackage")
    .Does(() =>
    {
        EnsureDirectoryExists("nuget");

        var formattedNugetVersion = FormatNugetVersion(version);

        var filesToPackPatterns = new string[]
            {
                "./ReactiveXComponent/bin/"+ buildConfiguration + "/*.dll",
                "./ReactiveXComponent/bin/"+ buildConfiguration + "/*.pdb",
                "./ReactiveXComponent/bin/"+ buildConfiguration + "/*.xml"
            };

        var filesToPack = GetFiles(filesToPackPatterns);

        var nuSpecContents = new List<NuSpecContent>();

        foreach (var file in filesToPack)
        {
            if (!file.FullPath.Contains("CodeAnalysisLog.xml"))
            {
                nuSpecContents.Add(new NuSpecContent {Source = file.FullPath, Target = @"lib\net451"});
            }
        }

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