// include Fake lib
#r @"packages/FAKE/tools/FakeLib.dll"
#r "System.Xml.Linq"

open Fake
open Fake.XMLHelper

let buildRelease = "./ReactiveXComponent/obj/Release/"
let releaseConfig = "release"
let configuration = getBuildParamOrDefault "config" releaseConfig



// Targets
Target "RestorePackages" (fun _ -> 
     "ReactiveXcomponent"
     |> RestoreMSSolutionPackages (fun p ->
         { p with
             OutputPath = "./packages"
             Retries = 4 })
)

Target "Clean" (fun _ ->    
    trace ("Cleaning")
    CleanDir buildRelease
)

Target "Compile" (fun _ ->    
    trace ("Compiling ReactiveXComponent Solution")
    !! "./ReactiveXComponent.sln"
    |> MSBuildRelease "" "Build"
    |> Log "MSBuild build Output: "
)

Target "RunTests" (fun _ ->
    trace ("Running tests...")
    !! ("./ReactiveXComponentTest/**/bin/"+configuration+"/*ReactiveXComponentTest.dll")
    |> NUnit (fun p ->
          {p with
             Framework = "v4.5.2";
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