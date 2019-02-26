#addin "Cake.Incubator&version=3.0.0"

Func<bool> IsRunningOnOsx = () => 
{
    return DirectoryExists("/Applications");
};

Func<bool> IsRunningOnLinux = () => 
{
    return IsRunningOnUnix() && !IsRunningOnOsx();
};

Func<string> GetXCBuildExtraParam = () => {
    if (IsRunningOnLinux()) 
    {
        return " --monoPath=\"/usr/lib/mono/4.5/Facades/\"";
    }
    if (IsRunningOnOsx()) 
    {
        return " --monoPath=\"/Library/Frameworks/Mono.framework/Versions/5.2.0/lib/mono/4.5/Facades/\"";
    }

    return "";
};