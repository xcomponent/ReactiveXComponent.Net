// include Fake lib
#r @"packages/FAKE/tools/FakeLib.dll"
#r "System.Xml.Linq"

open Fake
open Fake.RestorePackageHelper
open Fake.TaskRunnerHelper
open Fake.XMLHelper
open System
open System.Text.RegularExpressions
open System.IO
open System.Xml.Linq

// parameters and constants definition
let product = "ReactiveXComponent.Net"
let authors = [ "XComponent team" ]
let copyright = "Copyright XComponent"
let company = "XComponent"
let description = ".Net Reactive Client Api to interact with XComponent microservices"
let tags = ["xcomponent";"reactive";"api"]

let releaseConfig = "release"
let defaultVersion = "1.0.0-build1"
let configuration = getBuildParamOrDefault "config" releaseConfig
let version = getBuildParamOrDefault "version" defaultVersion

let reactiveNuspecFile = "./ReactiveXComponent/ReactiveXComponent.Net.nuspec"

let nugetExe = FullName @"./Tools/NuGet.exe"
let buildDir = "./build/"
let nugetDir = buildDir @@ "nuget"
let libDir = nugetDir @@ @"lib/net45/"

let defaultPushSource = "https://www.nuget.org/api/v2/package"

// helper functions
        
let formatAssemblyVersion (strVersion:string) =        
    let typeVersionMatch = Regex.Match(strVersion, "build|rc")    
    match typeVersionMatch.Success with
    | true ->
        let typeVersion = typeVersionMatch.Value
        let splitVersion = strVersion.Split('-')   
    
        let majorVersion = splitVersion.[0]        
        let buildVersion = splitVersion.[1]                           

        let extVersion = buildVersion.Substring(typeVersion.Length, buildVersion.Length - typeVersion.Length)
        
        let finalExtVersion = 
            match (Convert.ToInt32(extVersion)) with
            | x when x < 10 ->
                "000" + extVersion
            | x when x < 100 ->
                "00" + extVersion        
            | x when x < 1000 ->            
                "0" + extVersion         
            | x when x > 60000 ->
                "9"
            | _ -> extVersion
          
        let finalTypeVersion = 
            match (typeVersion) with
            | "build" -> "1"
            | "rc" -> "2" 
            | _ -> "3"           
        majorVersion + "." + finalTypeVersion + finalExtVersion
    | false -> strVersion + ".4" // specific release number

let formatNugetVersion (strVersion:string) =        
    let typeVersionMatch = Regex.Match(strVersion, "build|rc")    
    match typeVersionMatch.Success with
    | true ->
        let typeVersion = typeVersionMatch.Value
        let splitVersion = strVersion.Split('-')   
    
        let majorVersion = splitVersion.[0]        
        let buildVersion = splitVersion.[1]                           

        let extVersion = buildVersion.Substring(typeVersion.Length, buildVersion.Length - typeVersion.Length)
        
        let finalExtVersion = 
            match (Convert.ToInt32(extVersion)) with
            | x when x < 10 ->
                "00" + extVersion
            | x when x < 100 ->
                "0" + extVersion        
            | x when x > 1000 ->            
                extVersion                     
            | _ -> extVersion
        
        majorVersion + "-" + typeVersion + "v" + finalExtVersion
    | false -> strVersion

let formattedAssemblyVersion = formatAssemblyVersion version

let formattedNugetVersion = formatNugetVersion  version

let getMSBuildFn =
    let properties = [("Configuration", configuration);("AssemblyCompany", company);("AssemblyCopyright", copyright); ("VersionNumber", formattedAssemblyVersion)]
    fun (outputPath:string) targets (projects:FileIncludes) ->              
            MSBuild outputPath targets properties projects


// Targets
Target "RestorePackages" (fun _ -> 
     "ReactiveXComponent.sln"
     |> RestoreMSSolutionPackages (fun p ->
         { p with
             OutputPath = "./packages"
             Retries = 4 })
)

Target "Clean" (fun _ ->    
    trace ("Cleaning...")
    CleanDir buildDir
    !! "./ReactiveXComponent.sln"
    |> getMSBuildFn "" "Clean"
    |> Log "MSBuild Clean Output: "
)

Target "Compile" (fun _ ->    
    trace ("Compiling ReactiveXComponent Solution")
    !! "./ReactiveXComponent.sln"
    |> getMSBuildFn "" "Rebuild"
    |> Log "MSBuild build Output: "
)

Target "RunTests" (fun _ ->
    trace ("Running tests...")
    !! ("./ReactiveXComponentTest/**/bin/"+configuration+"/*ReactiveXComponentTest.dll")
    |> NUnit (fun p ->
          {p with
             Framework = "v4.5";
             StopOnError = true;
             DisableShadowCopy = true;
             OutputFile = "./TestResults.xml" })
)

Target "CreatePackage" (fun _ ->       
    let package nuspecFile csprojFile packageDescription packageTags libFiles =
        let removeDir dir = 
            let del _ = 
                DeleteDir dir
                not (directoryExists dir)
            runWithRetries del 3 |> ignore

        ensureDirectory nugetDir
        printfn "Creating nuget packages..."

        CleanDir nugetDir
        
        let projectDir = Path.GetDirectoryName nuspecFile  
        let project = Path.GetFileNameWithoutExtension nuspecFile      
        let compileDir = projectDir @@ @"bin" @@ configuration
        let packages = projectDir @@ "packages.config"
        let packageDependencies = if (fileExists packages) then (getDependencies packages) else []                

        let getGacReferences (csprojFile:string) =
            let document = XDocument.Load (new StreamReader(csprojFile,true))
            document.Descendants()
            |> Seq.filter(fun t -> t.Name.LocalName = "Reference")
            |> Seq.filter(fun t -> not (t.Descendants() |> Seq.exists(fun r -> r.Name.LocalName = "HintPath")))
            |> Seq.filter(fun t -> not(t.Descendants() |> Seq.exists(fun r -> r.Name.LocalName = "Private") || t.Descendants() |> Seq.exists(fun r -> r.Name.LocalName = "Private" && r.Value.ToLower() = "true" )))
            |> Seq.map(fun t -> { NugetFrameworkAssemblyReferences.FrameworkVersions  = ["net45"]; NugetFrameworkAssemblyReferences.AssemblyName = t.Attribute(XName.Get("Include")).Value})
            |> Seq.distinct
            |> Seq.toList

        let pack outputDir =
            NuGetHelper.NuGet
                (fun p ->
                    { p with
                        Description = packageDescription
                        Authors = authors
                        Copyright = copyright
                        Project =  project
                        Properties = ["Configuration", configuration]                        
                        Version = formattedNugetVersion
                        Tags = packageTags |> String.concat " "
                        FrameworkAssemblies = getGacReferences csprojFile
                        OutputPath = outputDir
                        WorkingDir = nugetDir                                                                   
                        Dependencies = packageDependencies })
                nuspecFile        

        // Copy dll, pdb and xml to libdir
        ensureDirectory libDir
        libFiles
        |> Seq.iter (fun f ->            
                CopyFileWithSubfolder compileDir libDir f)

        CopyFiles nugetDir ["LICENSE"; "README.md"]

        // Create both normal nuget package. 
        // Uses the files we copied to libDir and outputs to buildDir
        pack buildDir            
        
        removeDir nugetDir

    !! ("./ReactiveXComponent/bin/"+ configuration + "/*.dll")
    ++ ("./ReactiveXComponent/bin/"+ configuration + "/*.pdb")
    ++ ("./ReactiveXComponent/bin/"+ configuration + "/*.xml")  
    -- ("./ReactiveXComponent/bin/"+ configuration + "/*CodeAnalysisLog*.xml")
    |> package reactiveNuspecFile "ReactiveXComponent/ReactiveXComponent.csproj" description tags
)

Target "PublishPackage" (fun _ ->    
    let publishNugetPackages _ = 
        let rec publishPackage accessKey trialsLeft packageFile =
            let tracing = enableProcessTracing
            enableProcessTracing <- false
            let args pack key = sprintf "push \"%s\" %s -Source %s" pack key defaultPushSource                

            tracefn "Pushing %s Attempts left: %d" (FullName packageFile) trialsLeft
            try 
                let result = ExecProcess (fun info -> 
                        info.FileName <- nugetExe
                        info.WorkingDirectory <- (Path.GetDirectoryName (FullName packageFile))
                        info.Arguments <- (args packageFile accessKey)) (System.TimeSpan.FromMinutes 1.0)
                enableProcessTracing <- tracing
                if result <> 0 then failwithf "Error during NuGet symbol push. %s %s" nugetExe (args packageFile accessKey)
            with exn -> 
                if (trialsLeft > 0) then (publishPackage accessKey (trialsLeft-1) packageFile)
                else raise exn
        printfn "Pushing nuget packages"
        let normalPackages= 
            !! (buildDir @@ "*.nupkg") 
            |> Seq.sortBy(fun x -> x.ToLower())
        for package in normalPackages do
            publishPackage (getBuildParam "nugetkey") 3 package
            
    publishNugetPackages()
)


Target "All" DoNothing

// Dependencies
"RestorePackages"
  ==>"Clean"  
  ==> "Compile" 
  ==> "RunTests"
  ==> "CreatePackage"
  ==> "All"

"All"
  ==> "PublishPackage"
  
// start build
RunTargetOrDefault "All"