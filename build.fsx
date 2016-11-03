// include Fake lib
#r @"packages/FAKE/tools/FakeLib.dll"
#r "System.Xml.Linq"

open Fake
open Fake.XMLHelper

let releaseConfig = "release"
let configuration = getBuildParamOrDefault "config" releaseConfig

let getMSBuildFn =
    let properties = [("Configuration", configuration)]
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
    trace ("Cleaning")
    !! "./ReactiveXComponent.sln"
    |> getMSBuildFn "" "Clean"
    |> Log "MSBuild Clean Output: "
)

Target "Compile" (fun _ ->    
    trace ("Compiling ReactiveXComponent Solution")
    !! "./ReactiveXComponent.sln"
    |> getMSBuildFn "" "Build"
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


Target "All" DoNothing

// Dependencies
"RestorePackages"
  ==>"Clean"  
  ==> "Compile" 
  ==> "RunTests"
  ==> "All"

  
// start build
RunTargetOrDefault "All"