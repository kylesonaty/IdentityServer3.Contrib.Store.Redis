// include Fake lib
#r "packages/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Testing
open System

RestorePackages()

// Properties
let buildDir = "./build/"
let testDir  = "./test/"
let packagingRoot = "./publish"
let packagingDir = packagingRoot @@ "identityserver3.contrib.store.redis"
let authors = ["Kyle Sonaty"]
let projectName = "IdentityServer3.Contrib.Store.Redis"
let projectDescription = "Redis Store for Identity Server 3"
let projectSummary = projectDescription

let releaseNotes =
    ReadFile "ReleaseNotes.md"
    |> ReleaseNotesHelper.parseReleaseNotes

let topLevelNugetDependencies = Set.ofArray [| "StackExchange.Redis"; "IdentityServer3"; "Newtonsoft.Json" |]

let filterNugetDependencies(dependencies:seq<string * string>) =
    dependencies |> Seq.where(fun (n,v) -> topLevelNugetDependencies.Contains n) |> List.ofSeq

MSBuildDefaults <- {
  MSBuildDefaults with
    ToolsVersion = Some "14.0"
    Verbosity = Some MSBuildVerbosity.Minimal
}

// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir; testDir]
)

Target "BuildApp" (fun _ ->
   !! "src/IdentityServer3.Contrib.Store.Redis/*.csproj"
     |> MSBuildRelease buildDir "Build"
     |> Log "AppBuild-Output: "
)

Target "BuildTest" (fun _ ->
    !! "src/IdentityServer3.Contrib.Store.Redis.Tests/*.csproj"
      |> MSBuildDebug testDir "Build"
      |> Log "TestBuild-Output: "
)

let testDlls = !! (testDir + "/*Tests.dll")

Target "xUnitTest" (fun _ ->
    testDlls
      |> xUnit2 (fun p ->
          { p with
              TimeOut = TimeSpan.MaxValue
              HtmlOutputPath = Some(testDir + "xunit.html") })
)

Target "Default" (fun _ ->
    trace "Hello World from FAKE"
)

Target "Nuget" (fun _ ->
    // Copy all the package files into a package folder
    let net45Dir = packagingDir @@ "lib/net45"
    CleanDirs [net45Dir]

    CopyFile net45Dir (buildDir @@ "IdentityServer3.Contrib.Store.Redis.dll")
    CopyFile net45Dir (buildDir @@ "IdentityServer3.Contrib.Store.Redis.xml")
    CopyFile net45Dir (buildDir @@ "IdentityServer3.Contrib.Store.Redis.pdb")
    CopyFiles packagingDir ["ReleaseNotes.md"]

    NuGet (fun p ->
        {p with
            Authors = authors
            Project = projectName
            Description = projectDescription
            OutputPath = packagingRoot
            Summary = projectSummary
            WorkingDir = packagingDir
            Version = releaseNotes.NugetVersion
            ReleaseNotes = toLines releaseNotes.Notes
            Dependencies = getDependencies "src/IdentityServer3.Contrib.Store.Redis/packages.config" |> filterNugetDependencies
            AccessKey = getBuildParamOrDefault "nugetaccesskey" ""
            Publish = hasBuildParam "nugetaccesskey" })
            "IdentityServer3.Contrib.Store.Redis.nuspec"
)

// Dependencies
"Clean"
  ==> "BuildApp"
  ==> "BuildTest"
  ==> "xUnitTest"
  ==> "Default"
  ==> "Nuget"

// start build
RunTargetOrDefault "Default"
