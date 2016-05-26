// include Fake lib
#r @"packages/FAKE/tools/FakeLib.dll"
#r "System.Xml.Linq"

open Fake
open Fake.XMLHelper

let buildReleaseDir = "./XCClientLib/obj/Release/"

Target "Clean" (fun _ ->    
    trace ("Cleaning")
    CleanDir buildReleaseDir
)

Target "Compile" (fun _ ->    
    trace ("Compiling XCClientLib project")
    !! "./XCClientLib/**/*.csproj"
    |> MSBuildRelease buildReleaseDir "Build"
    |> Log "Compiling Output: "
)
  

Target "All" DoNothing

// Dependencies
"Clean"  
  ==> "Compile"    
  ==> "All"

  
// start build
RunTargetOrDefault "All"