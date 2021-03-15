open System
open System.IO
open Fake.IO
open Tools

let path xs = Path.Combine(Array.ofList xs)

let solutionRoot = Files.findParent __SOURCE_DIRECTORY__ "App.sln";

let desktop = path [ solutionRoot; "Desktop" ]

let clientDist = path [ solutionRoot; "dist" ]

let buildFor(runtime: Runtime) = 
    let releaseMode = Release
    // build the desktop app in release mode
    Dotnet.Publish(desktop, [
        Dotnet.Configuration(releaseMode)
        Dotnet.Runtime(runtime)
        Dotnet.SelfContained()
        Dotnet.PublishSingleFile()
    ])
    // build the frontend 
    Npm.Install(solutionRoot)
    Npm.Run("build", solutionRoot)
    // copy built client artifacts to the output
    // the built application expects the static files to be at wwwroot
    // relative to the executable directory
    let clientTarget = path [ desktop; "bin"; releaseMode.Format(); "net5.0"; runtime.Format(); "publish"; "wwwroot" ]
    Copy.DirectoryFrom(clientDist).To(clientTarget)

[<EntryPoint>]
let main argv =
    match argv with
    | [| "build-win64"   |] -> buildFor(Runtime.Win10_x64)
    | [| "build-linux64" |] -> buildFor(Runtime.Linux_x64)
    | [| "build-osx64"   |] -> buildFor(Runtime.Osx_x64)
    | otheriwse -> printfn "Build project"
    0