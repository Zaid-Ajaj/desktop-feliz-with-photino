module Program

open System
open System.Threading

open Suave
open Suave.Filters
open Suave.Successful
open Suave.Operators
open Shared
open Fable.Remoting.Server
open Fable.Remoting.Suave
open PhotinoNET

let serverApi : IServerApi = { 
    Counter = fun () -> async {
        return { Value = 10 }
    }

    SystemInfo = fun () -> async {
        do! Async.Sleep 1500
        return {
            Platform = Environment.OSVersion.Platform.ToString()
            Version = Environment.OSVersion.VersionString
        }
    }
}
 
let webApi =
    Remoting.createApi()
    |> Remoting.fromValue serverApi
    |> Remoting.withRouteBuilder routerPaths
    |> Remoting.buildWebPart

let rnd = System.Random()

let randomPort = rnd.Next(9000, 10000)

let webApp = choose [ webApi; GET >=> OK "Welcome to full stack F#" ]

let isDevelopment = 
    #if DEBUG
    true
    #else 
    false
    #endif

let suavePort = 
    if isDevelopment 
    // Suave web server has to run on port 5000
    // during development because webpack-dev-server proxies
    // requests to the backend here
    then 5000 
    else randomPort

let desktopUrl = 
    if isDevelopment 
    // during development assume webpack dev server is running
    then "http://localhost:8080" 
    // in release mode we run Suave on a random port 
    // which will host the static files generated
    else $"http://localhost:{randomPort}" 

[<EntryPoint; STAThread>]
let main args = 

    let cts = new CancellationTokenSource()

    let suaveConfig =
        { defaultConfig with
            homeFolder = Some "./wwwwroot"
            bindings   = [ HttpBinding.createSimple HTTP "127.0.0.1" suavePort ]
            bufferSize = 2048
            cancellationToken = cts.Token }
    
    let listening, server = startWebServerAsync suaveConfig webApp
    
    Async.Start server

    listening
    |> Async.RunSynchronously
    |> ignore

    printfn "Suave server started"
    printfn ""

    let window = new PhotinoWindow(title="Full Stack F# on Desktop (Using Photino)")
    window.Center().Load(Uri(desktopUrl)).WaitForClose()
    0