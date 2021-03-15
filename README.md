# Feliz on Desktop with Photino (Highly Experimental)

A full stack F# application on desktop with the help of [Photino](https://www.tryphotino.io).

Phontino hosts a lightweight chromioum instance in a desktop window and this application embeds a full web application in it.

Using [Suave](https://github.com/SuaveIO/suave) as an extremely lightweight web server for the backend and [Feliz](https://github.com/Zaid-Ajaj/Feliz)/React on the frontend.

Of course with fully type-safe data transport between client and server using [Fable.Remoting](https://github.com/Zaid-Ajaj/Fable.Remoting)

![image](photino-feliz.gif)

## Dependencies
The dependencies need are the .NET SDK and Node.js

On Windows 10, the Edge Dev Channel version needs to be installed from [Microsoft Edge Insider](https://www.microsoftedgeinsider.com/en-us/download) to be able to view the browser

## Development

Before doing anything, start with installing npm dependencies into the client using `npm install`.

Then to start development mode with hot module reloading, run:
```bash
npm start
```
This will start the development server after compiling the project, once it is finished, navigate to http://localhost:8080 to view the application .

While the frontend is running, go the `./Desktop` and run the application in debug mode using
```
dotnet restore
dotnet run
```
This will do two things:
 - Runs a Suave backend in the background on port 5000 (API for the frontend)
 - Opens a Photino window navigating to localhost:8080 which is where the frontend is being hosted

## Build and package the application 

The `Build` project includes a couple of build targets to package up the application. 

```bash
cd ./Build
# windows
dotnet run -- build-win64
# linux
dotnet run -- build-linux64
# macOS
dotnet run -- build-osx64
```
You will find the application packaged inside the directory
```
Windows -> {root}/Desktop/bin/Release/net5.0/win10-x64/publish
Linux -> {root}/Desktop/bin/Release/net5.0/linux-x64/publish
MacOS -> {root}/Desktop/bin/Release/net5.0/osx-x64/publish
```
Feel free to extend the packaging function yourself:
```fs
let buildFor(runtime: Runtime) = 
    // build the desktop app in release mode
    Dotnet.Publish(desktop, [
        Dotnet.Configuration(Release)
        Dotnet.Runtime(runtime)
        Dotnet.SelfContained()
        Dotnet.PublishSingleFile()
    ])
    // build the frontend 
    Npm.Install(solutionRoot)
    Npm.Run("build", solutionRoot)
    // copy client artifacts to the output
    let appDist = path [ desktop; "bin"; "Release"; "net5.0"; runtime.Format(); "publish" ]
    let clientTarget = path [ appDist; "wwwroot" ]
    Shell.copyDir clientTarget clientDist (fun fileToCopy -> true)

// Windows
buildFor(Runtime.Win10_x64)
// Linux
buildFor(Runtime.Linux_x64)
// macOS
buildFor(Runtime.Osx_x64)
```