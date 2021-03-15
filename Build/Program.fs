open System
open System.IO
open Fake.IO
open Tools

let path xs = Path.Combine(Array.ofList xs)

let solutionRoot = Files.findParent __SOURCE_DIRECTORY__ "App.sln";

let desktop = path [ solutionRoot; "Desktop" ]

let clientDist = path [ solutionRoot; "dist" ]

let buildFor(runtime: Runtime) = 
    // build the desktop app in release mode
    Dotnet.Publish(desktop, [
        Dotnet.Configuration(Release)
        Dotnet.Runtime(runtime)
        Dotnet.SelfContained()
        Dotnet.PublishSingleFile()
    ])
    // build the frontend 
    Npm.Run("build", solutionRoot)
    // copy client artifacts to the output
    let appDist = path [ desktop; "bin"; "Release"; "net5.0"; runtime.Format(); "publish" ]
    let clientTarget = path [ appDist; "wwwroot" ]
    Shell.copyDir clientTarget clientDist (fun fileToCopy -> true)

[<EntryPoint>]
let main argv =
    match argv with
    | [| "build-win64"   |] -> buildFor(Runtime.Win10_x64)
    | [| "build-linux64" |] -> buildFor(Runtime.Linux_x64)
    | [| "build-osx64"   |] -> buildFor(Runtime.Osx_x64)
    | otheriwse -> printfn "Build project"
    0