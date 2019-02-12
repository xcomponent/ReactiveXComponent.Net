#addin "Cake.Incubator&version=1.6.0"
#addin "Cake.VsixSignTool&version=1.1.0"
#tool "nuget:?package=WiX&version=3.11.0"

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

public class Settings {
  public string Configuration { get; set; }
  public string Target { get; set; }
  public string VersionNumber { get; set; }
  public bool? IsCommunityEdition { get; set; }
  public string VSVersion {get; set; }
}

Func<bool> IsRunningOnOsx = () => 
{
    return DirectoryExists("/Applications");
};

Func<bool> IsRunningOnLinux = () => 
{
    return IsRunningOnUnix() && !IsRunningOnOsx();
};

public MSBuildSettings GetDefaultMSBuildSettings() 
{
    if (IsRunningOnLinux()){
        return new MSBuildSettings { ToolPath = new FilePath("/usr/bin/msbuild")};
    }
    return new MSBuildSettings();
}

public void CrossPlatformBuild(string filePath, Settings settings) 
{  
	if (settings == null)
    {      
		MSBuild(filePath, GetDefaultMSBuildSettings());
    }
    else
    {
		var msbuildSettings = GetDefaultMSBuildSettings();

		if(!string.IsNullOrEmpty(settings.Target))
		{
			msbuildSettings.WithTarget(settings.Target);
		}

		if(!string.IsNullOrEmpty(settings.Configuration))
		{
			msbuildSettings.SetConfiguration(settings.Configuration);
		}

		if (settings.IsCommunityEdition.HasValue)
		{
			string assemblyProduct = settings.IsCommunityEdition.Value ? "\"XComponent Community Edition\"" : "\"XComponent Workgroup Edition\"";
			msbuildSettings.WithProperty("AssemblyProduct", assemblyProduct);
			if (settings.IsCommunityEdition.Value)
			{
				msbuildSettings.WithProperty("DefineConstants", "CommunityEdition");
			}
		}
		
		if (!string.IsNullOrEmpty(settings.VersionNumber)) 
		{
			msbuildSettings.WithProperty("VersionNumber", settings.VersionNumber);
		}
		
		if (!string.IsNullOrEmpty(settings.VSVersion)) 
		{
			msbuildSettings.WithProperty("VSVersion", settings.VSVersion);
		}
		
		MSBuild(filePath, msbuildSettings);
    }
}

public string PadBuildPart(int buildPart) 
{
	string paddedBuildPart = buildPart.ToString();
	if (buildPart < 10)
	{
		paddedBuildPart = "000" + paddedBuildPart;
	}
	else if (buildPart < 100)
	{
		paddedBuildPart = "00" + paddedBuildPart;
	}
	else if (buildPart < 1000)
	{
		paddedBuildPart = "0" + paddedBuildPart;
	}
	else if (buildPart > 60000) 
	{
		paddedBuildPart = "9";
	}
	return paddedBuildPart;
}

// convert version (ex : 3.x.y-G15) to wix format (ex : 3.x.y.415)
public string FormatWixVersion(string buildVersion) 
{
	var versionMatch = System.Text.RegularExpressions.Regex.Match(buildVersion, @"(?<major>\d+\.\d+\.\d+)-(?<type>[ZDBCG])(?<build>\d+)");
	if (versionMatch.Success)
	{
		string typePart = versionMatch.Groups["type"].Value;
		string majorPart = versionMatch.Groups["major"].Value;
		int buildPart = Int32.Parse(versionMatch.Groups["build"].Value);
		string paddedBuildPart = PadBuildPart(buildPart);

		int versionTypeDigit = 0;
		switch (typePart)
		{
			case "Z":
				versionTypeDigit = 1;
				break;
			case "D":
				versionTypeDigit = 2;
				break;
			case "B":
				versionTypeDigit = 3;
				break;
			case "C":
				versionTypeDigit = 4;
				break;
			case "G": 
				versionTypeDigit = 5;
				break;
		}

		return string.Format("{0}.{1}{2}", majorPart, versionTypeDigit, paddedBuildPart);
	}

	throw new ArgumentException("Invalid build version", "buildVersion");
}