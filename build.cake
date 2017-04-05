#tool "nuget:?package=NUnit.Runners&version=2.6.4"
#addin "Cake.FileHelpers"
#addin "Cake.Incubator"

var target = Argument("target", "Build");
var buildConfiguration = Argument("buildConfiguration", "Release");
var version = Argument("buildVersion", "1.0.0-build1");
var apiKey = Argument("nugetKey", "");

var formatAssemblyVersion = new Func<string, string>(version =>
{
    var result = string.Empty;
    var versionType = string.Empty;

    if (version.Contains("build"))
    {
        versionType = "build";
    }
    else if (version.Contains("rc"))
    {
        versionType = "rc";
    }

    if (!string.IsNullOrEmpty(versionType))
    {
        var versionComponents = version.Split('-');
        var majorVersion = versionComponents[0];
        var buildVersion = versionComponents[1];
        var buildNumberStr = buildVersion.Substring(
                                    versionType.Length,
                                    buildVersion.Length - versionType.Length);

        var buildNumber = int.Parse(buildNumberStr);
        var buildNumberPrefix = string.Empty;
        if (buildNumber < 10)
        {
            buildNumberPrefix = "000";
        }
        else if (buildNumber < 100)
        {
            buildNumberPrefix = "00";
        }
        else if (buildNumber < 1000)
        {
            buildNumberPrefix = "0";
        }
        else if (buildNumber > 60000)
        {
            buildNumberPrefix = "9";
        }

        var versionTypeNumber = (versionType == "build") ? "1" : "2";

        result = majorVersion + "." + versionTypeNumber + buildNumberPrefix + buildNumberStr;
    }
    else
    {
        result = version + ".4";
    }

    return result;
});

var formatNugetVersion = new Func<string, string>(version =>
{
    var result = version;
    var versionType = string.Empty;

    if (version.Contains("build"))
    {
        versionType = "build";
    }
    else if (version.Contains("rc"))
    {
        versionType = "rc";
    }

    if (!string.IsNullOrEmpty(versionType))
    {
        var versionComponents = version.Split('-');
        var majorVersion = versionComponents[0];
        var buildVersion = versionComponents[1];
        var buildNumberStr = buildVersion.Substring(
                                    versionType.Length,
                                    buildVersion.Length - versionType.Length);

        var buildNumber = int.Parse(buildNumberStr);
        var buildNumberPrefix = string.Empty;
        if (buildNumber < 10)
        {
            buildNumberPrefix = "00";
        }
        else if (buildNumber < 100)
        {
            buildNumberPrefix = "0";
        }
        else if (buildNumber > 1000)
        {
            buildNumberPrefix = string.Empty;
        }

        result = majorVersion + "-" + versionType + "v" + buildNumberPrefix + buildNumberStr;
    }

    return result;
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

        var msBuildSettings = new MSBuildSettings();
        msBuildSettings.SetConfiguration(buildConfiguration)
        .WithTarget("Clean");

        MSBuild("ReactiveXComponent.sln", msBuildSettings);
    });

Task("RestoreNugetPackages")
    .Does(() =>
    {
        NuGetRestore("ReactiveXComponent.sln", new NuGetRestoreSettings { NoCache = true });
    });

Task("Build")
    .Does(() =>
    {
        var formattedAssemblyVersion = formatAssemblyVersion(version);
        var msBuildSettings = new MSBuildSettings() { Configuration = buildConfiguration };
        msBuildSettings.WithTarget("Rebuild");
        msBuildSettings.WithProperty("AssemblyVersion", formattedAssemblyVersion);
        
        MSBuild(@"./ReactiveXComponent.sln", msBuildSettings);
    });

Task("Test")
    .Does(() =>{
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

    var formattedNugetVersion = formatNugetVersion(version);

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
        var formattedNugetVersion = formatNugetVersion(version);
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