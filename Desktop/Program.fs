module Program

open System
open System.Threading

open Suave
open Shared
open Fable.Remoting.Server
open Fable.Remoting.Suave
open PhotinoNET
open System.IO
open System.Reflection

let serverApi : IServerApi = {
    Counter = fun () -> async {
        return { Value = 10 }
    }

    SystemInfo = fun () -> async {
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

let random = Random()
// TODO: find a better way for choosing a free port
let randomPort = random.Next(9000, 10000)

let webApp = choose [
    webApi
    Files.browseHome
]

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
    else $"http://localhost:{randomPort}/index.html"

[<EntryPoint; STAThread>]
let main args =
    let cts = new CancellationTokenSource()
    let currentAssembly = Assembly.GetExecutingAssembly()
    let executableDirectory = Directory.GetParent(currentAssembly.Location).FullName
    let frontendAssets = Path.Combine(executableDirectory, "wwwroot")
    let suaveConfig =
        { defaultConfig with
            homeFolder = Some frontendAssets
            bindings   = [ HttpBinding.createSimple HTTP "127.0.0.1" suavePort ]
            bufferSize = 2048
            cancellationToken = cts.Token }

    let listening, server = startWebServerAsync suaveConfig webApp
    Async.Start server

    listening
    |> Async.RunSynchronously
    |> ignore

    if isDevelopment then
        printfn "Suave server started"
        printfn "A Phontino window should pop up"
        printfn "That window is running the user interface hosted by webpack at http://localhost:8080"
        printfn "Meanwhile the (Suave) backend is running on http://localhost:5000"
        printfn "Webpack dev server will proxy all HTTP calls to Suave during the development session"

    let window = new PhotinoWindow(title="Full Stack F# on Desktop (Using Photino)")
    window.Center().Load(Uri(desktopUrl)).WaitForClose()
    0