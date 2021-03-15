module Tools

open System
open System.IO
open Fake.Core

module CreateProcess =
    /// Creates a cross platfrom command from the given program and arguments.
    ///
    /// For example:
    ///
    /// ```fsharp
    /// CreateProcess.xplatCommand "npm" [ "install" ]
    /// ```
    ///
    /// Will be the following on windows
    ///
    /// ```fsharp
    /// CreateProcess.fromRawCommand "cmd" [ "/C"; "npm"; "install" ]
    /// ```
    /// And the following otherwise
    ///
    /// ```fsharp
    /// CreateProcess.fromRawCommand "npm" [ "install" ]
    /// ```
    let xplatCommand program args =
        let program', args' =
            if Environment.isWindows
            then "cmd", List.concat [ [ "/C"; program ]; args ]
            else program, args

        CreateProcess.fromRawCommand program' args'
    
/// <summary>Finds the executable path from the name of the tool</summary>
let executablePath (tool: string) = 
    let locator = 
        if Environment.isWindows
        then "C:\Windows\System32\where.exe"
        else "/usr/bin/which"
    
    let locatorOutput =
        CreateProcess.xplatCommand locator [ tool ]
        |> CreateProcess.redirectOutput
        |> Proc.run

    if locatorOutput.ExitCode <> 0 then failwithf "Could not determine the executable path of '%s'" tool

    locatorOutput.Result.Output
    |> String.splitStr Environment.NewLine
    |> List.filter (fun path -> (Environment.isWindows && Path.HasExtension(path)) || Environment.isUnix)
    |> List.tryFind File.Exists
    |> function 
        | Some executable -> executable
        | None -> failwithf "The executable paht '%s' was not found" tool

let dotnet = executablePath "dotnet"
let npm = executablePath "npm"
let node = executablePath "node"

/// <summary>Build mode</summary>
type Mode = 
    | Debug
    | Release
    with 
        member self.Format() = 
            match self with 
            | Debug -> "Debug"
            | Release -> "Release"

/// <summary>The different runtime targets for which the application can be packaged</summary>
[<RequireQualifiedAccess>]
type Runtime = 
    /// Portable windows
    | Win_x86
    /// Portable windows
    | Win_x64 
    /// Portable windows
    | Win_Arm
    /// Portable windows
    | Win_Arm64
    /// Windows 7 / Windows Server 2008 R2
    | Win7_x64
    /// Windows 7 / Windows Server 2008 R2 
    | Win7_x86
    /// Windows 10 / Windows Server 2016
    | Win10_x64
    /// Windows 10 / Windows Server 2016
    | Win10_x86
    /// Windows 10 / Windows Server 2016
    | Win10_Arm
    /// Windows 10 / Windows Server 2016
    | Win10_Arm64
    /// Most desktop distributions like CentOS, Debian, Fedora, Ubuntu, and derivatives
    | Linux_x64
    /// Lightweight distributions using musl like Alpine Linux
    | Linux_Musl_x64
    /// Linux distributions running on ARM like Raspbian on Raspberry Pi Model 2+
    | Linux_Arm
    /// Linux distributions running on 64-bit ARM like Ubuntu Server 64-bit on Raspberry Pi Model 3+
    | Linux_Arm_x64
    /// Portable Minimum OS version is macOS 10.12 Sierra
    | Osx_x64
    /// macOS 10.10 Yosemite
    | Osx_10_10_x64
    /// macOS 10.11 El Capitan
    | Osx_10_11_x64
    /// macOS 10.12 Sierra
    | Osx_10_12_x64
    /// macOS 10.13 High Sierra
    | Osx_10_13_x64
    /// macOS 10.14 Mojave
    | Osx_10_14_x64
    /// macOS 10.15 Catalina
    | Osx_10_15_x64
    /// macOS 11.01 Big Sur
    | Osx_11_0_x64
    /// macOS 11.01 Big Sur
    | Osx_11_0_Arm64
    with 
        member self.Format() = 
            match self with 
            | Runtime.Win_x86 -> "win-x86"
            | Runtime.Win_x64 -> "win-x64"
            | Runtime.Win_Arm -> "win-arm"
            | Runtime.Win_Arm64 -> "win-arm64"
            | Runtime.Win7_x64 -> "win7-x64"
            | Runtime.Win7_x86 -> "win7-x86"
            | Runtime.Win10_x64 -> "win10-x64"
            | Runtime.Win10_x86 -> "win10-x86"
            | Runtime.Win10_Arm -> "win10-arm"
            | Runtime.Win10_Arm64 -> "win10-arm64"
            | Runtime.Linux_x64 -> "linux-x64"
            | Runtime.Linux_Musl_x64 -> "linux-musl-x64"
            | Runtime.Linux_Arm -> "linux-arm"
            | Runtime.Linux_Arm_x64 -> "linux-arm64"
            | Runtime.Osx_x64 -> "osx-x64"
            | Runtime.Osx_10_10_x64 -> "osx.10.10-x64"
            | Runtime.Osx_10_11_x64 -> "osx.10.11-x64"
            | Runtime.Osx_10_12_x64 -> "osx.10.12-x64"
            | Runtime.Osx_10_13_x64 -> "osx.10.13-x64"
            | Runtime.Osx_10_14_x64 -> "osx.10.14-x64"
            | Runtime.Osx_10_15_x64 -> "osx.10.15-x64"
            | Runtime.Osx_11_0_x64 -> "osx.11.0-x64"
            | Runtime.Osx_11_0_Arm64 -> "osx.11.0-arm64"

type Npm  = 
    static member Run(script: string, cwd: string) = 
        let exitCode = Shell.Exec(npm, $"run {script}", cwd)
        if exitCode <> 0 then failwithf "Could not execute npm run %s" script

    static member Install(cwd: string) = 
        let exitCode = Shell.Exec(npm, $"install", cwd)
        if exitCode <> 0 then failwith "Could not execute npm install"

type Dotnet = 
    static member Build(cwd: string, arguments: string) = 
        let exitCode = Shell.Exec(dotnet, $"build {arguments}", cwd)
        if exitCode <> 0 then failwithf "Could not execute dotnet build %s at '%s'" arguments cwd 
         
    static member Run(cwd: string, ?arguments: string) = 
        let args = 
            match arguments with
            | None -> ""
            | Some arg -> $" -- {arg}"

        let exitCode = Shell.Exec(dotnet, $"run{args}", cwd)
        if exitCode <> 0 then failwithf "Could not execute dotnet run%s at '%s'" args cwd 

    static member Configuration(mode: Mode) = 
        match mode with 
        | Debug -> "--configuration Debug"
        | Release -> "--configuration Release"

    static member SelfContained() = "--self-contained true" 

    static member SelfContained(value: bool) = 
        if value 
        then "--self-contained true" 
        else "--self-contained false"

    static member Runtime(runtime: Runtime) = $"--runtime {runtime.Format()}"

    static member PublishTrimmed() = "-p:PublishTrimmed=true" 

    static member PublishTrimmed(value: bool) = 
        if value 
        then "-p:PublishTrimmed=true" 
        else "" 

    static member PublishSingleFile() = "-p:PublishTrimmed=true"

    static member PublishSingleFile(value: bool) = 
        if value 
        then "-p:PublishTrimmed=true" 
        else "" 

    static member Publish(cwd: string, args: string list) = 
        let formattedArgs = 
            args
            |> List.filter (fun arg -> arg <> "")
            |> String.concat " "

        let exitCode = Shell.Exec(dotnet, $"publish {formattedArgs}", cwd)
        if exitCode <> 0 
        then failwithf "Could not execute dotnet publish at '%s'" cwd 

type PasteMethods = 
    abstract To : string -> unit 

type Copy = 
    static member DirectoryFrom(source: string) = 
        { 
            new PasteMethods with 
               member self.To(target) = 
                   Fake.IO.Shell.copyDir target source (fun fileToCopy -> true)
        }
