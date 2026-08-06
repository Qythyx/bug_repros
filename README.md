# Repro: no-change Android builds repack the APK when `EmbedAssembliesIntoApk=true`

Minimal reproduction for a .NET Android incremental-build bug.

An Android app with **at least one `ProjectReference`**, built with `EmbedAssembliesIntoApk=true`,
never builds incrementally. Every build — including one run immediately after a successful build,
with nothing touched — re-runs `_BuildApkEmbed`, then re-zipaligns and re-signs the APK.

## How to see it

```bash
dotnet build ReproApp/ReproApp.csproj -f net10.0-android   # first build
dotnet build ReproApp/ReproApp.csproj -f net10.0-android   # no changes — still ~32s
```

To see which target is running and why:

```bash
dotnet build ReproApp/ReproApp.csproj -f net10.0-android -clp:PerformanceSummary -v d
```

Look for `Building target "_BuildApkEmbed" completely.` followed by MSBuild's reason line.

## Measurements

Second (no-change) build, macOS 26.6 arm64, .NET SDK 10.0.101, android workload 36.1.53:

| Configuration                                                           | Wall time | `_BuildApkEmbed`         |
| ----------------------------------------------------------------------- | --------- | ------------------------ |
| As committed (`ProjectReference` + `EmbedAssembliesIntoApk=true`)       | 32.5s     | 29.7s                    |
| With the `ProjectReference` removed from `ReproApp.csproj`              | 1.0s      | 2ms                      |
| With `-p:EmbedAssembliesIntoApk=false` (fast deploy, the Debug default) | 3.2s      | `_BuildApkFastDev` 202ms |

## What is happening

`ReproLib` is compiled twice per app build, by two MSBuild project configurations. Their two `csc`
command lines have 362 arguments each and differ in exactly one — `/define:` — carrying the same 75
constants in a different order:

```
compile 1: /define:TRACE;DEBUG;__XAMARIN_ANDROID_v1_0__;__MOBILE__;__ANDROID__;...;NET;NET10_0;...
compile 2: /define:TRACE;DEBUG;NET;NET10_0;NETCOREAPP;ANDROID;...;__XAMARIN_ANDROID_v1_0__;...
```

`_GenerateCompileDependencyCache` hashes `$(DefineConstants)` as a string, so the two configurations
compute different hashes and overwrite each other's `ReproLib.csproj.CoreCompileInputs.cache` every
build. The last write lands after csc wrote the compile outputs, so the next build sees the cache as
newer than the outputs and recompiles. The file content is byte-identical between builds — only the
mtime moves — so it never converges.

That recompile then invalidates everything downstream:

```
ReproLib.csproj.CoreCompileInputs.cache  newer than  ReproLib.xml
  → ReproLib.dll                         newer than  stamp/_ResolveLibraryProjectImports.stamp
  → android/assets/arm64-v8a/ReproLib.dll newer than stamp/_GenerateJavaStubs.stamp
  → android/assets/arm64-v8a/ReproLib.dll newer than stamp/_GeneratePackageManagerJava.stamp
  → android/assets/arm64-v8a/ReproLib.dll newer than android/bin/com.companyname.reproapp.apk
      → _BuildApkEmbed runs completely, then re-zipalign and re-sign
```

Building `ReproLib` on its own is perfectly incremental — `CoreCompile` is skipped and the cache and
compile outputs share a timestamp. The second configuration only appears when the library is built
as a reference of the Android app.

## Not required to reproduce

- Multi-targeting the library — a single-TFM `net10.0-android` library reproduces (as committed).
- `GenerateDocumentationFile` — reproduces with or without it.
- Multiple RIDs — `RuntimeIdentifiers` is pinned to `android-arm64` here.
