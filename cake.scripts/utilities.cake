var FormatAssemblyVersion = new Func<string, string>(currentVersion =>
{
    var result = string.Empty;
    var versionType = string.Empty;

    if (currentVersion.Contains("build"))
    {
        versionType = "build";
    }
    else if (currentVersion.Contains("rc"))
    {
        versionType = "rc";
    }
    else if (currentVersion.Contains("release"))
    {
        versionType = "release";
    }

    if (!string.IsNullOrEmpty(versionType))
    {
        var versionComponents = currentVersion.Split('-');
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

        var versionTypeNumber = string.Empty;

        switch (versionType)
        {
            case "build":
                versionTypeNumber = "1";
                break;
            case "rc":
                versionTypeNumber = "2";
                break;
            case "release":
                versionTypeNumber = "3";
                break;
            default:
                versionTypeNumber = "0";
                break;
        }

        result = majorVersion + "." + versionTypeNumber + buildNumberPrefix + buildNumberStr;
    }
    else
    {
        result = currentVersion + ".4";
    }

    return result;
});

var FormatNugetVersion = new Func<string, string>(currentVersion =>
{
    var result = currentVersion;
    var versionType = string.Empty;

    if (currentVersion.Contains("build"))
    {
        versionType = "build";
    }
    else if (currentVersion.Contains("rc"))
    {
        versionType = "rc";
    }
    else if (currentVersion.Contains("release"))
    {
        versionType = "release";
    }

    if (!string.IsNullOrEmpty(versionType))
    {
        var versionComponents = currentVersion.Split('-');
        var majorVersion = versionComponents[0];

        if (versionType == "release")
        {
            result = majorVersion;
        }
        else
        {
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
    }

    return result;
});

var CleanSolution = new Action<string, string>((solutionPath, configuration) =>
{
    if (IsRunningOnUnix())
    {
        var xBuildSettings = new XBuildSettings();
        xBuildSettings.SetConfiguration(configuration)
        .WithTarget("Clean");

        XBuild(solutionPath, xBuildSettings);
    }
    else
    {
        var msBuildSettings = new MSBuildSettings();
        msBuildSettings.SetConfiguration(configuration)
        .WithTarget("Clean");

        MSBuild(solutionPath, msBuildSettings);
    }
});

var BuildSolution = new Action<string, string, bool, string>((solutionPath, configuration, hasSetAssemblyVersion, targetVersion) =>
{
    var formattedAssemblyVersion = FormatAssemblyVersion(targetVersion);

    if (IsRunningOnUnix())
    {
        var xBuildSettings = new XBuildSettings() { Configuration = configuration };
        xBuildSettings.WithTarget("Rebuild");
        xBuildSettings.WithProperty("AssemblyVersion", formattedAssemblyVersion);
        xBuildSettings.WithProperty("SetAssemblyVersion", hasSetAssemblyVersion.ToString());
        
        XBuild(solutionPath, xBuildSettings);
    }
    else
    {
        var msBuildSettings = new MSBuildSettings() { Configuration = configuration };
        msBuildSettings.WithTarget("Rebuild");
        msBuildSettings.WithProperty("AssemblyVersion", formattedAssemblyVersion);
        msBuildSettings.WithProperty("SetAssemblyVersion", hasSetAssemblyVersion.ToString());
        
        MSBuild(solutionPath, msBuildSettings);
    }
});