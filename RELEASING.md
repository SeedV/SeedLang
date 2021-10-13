# Releasing

## SeedLang for C# and Unity

### The NuGet package of SeedLang

https://www.nuget.org/packages/SeedLang/

### Publishing a Preview Build

To publish a new preview build of the NuGet package:

```shell
cd csharp
dotnet clean
rm src/SeedLang/bin/Release/SeedLang.*.nupkg
dotnet pack -c Release -p:ReleaseTag=preview
dotnet nuget push src/SeedLang/bin/Release/SeedLang.*.nupkg --api-key <apikey> \
 --source https://api.nuget.org/v3/index.json
```

`ReleaseTag` can be an arbitrary string tag to indicate the build type.
Recommended tags include `alpha`, `beta` and `preview`.

The `<apikey>` to access NuGet.org is maintained by the administrator of the dev
team.

The [semantic](https://semver.org/) version number `<major>.<minor>.<patch>`
will not change when publishing preview builds, while the pre-release tag will
be updated, composed of the release tag and an auto-generated timestamp.

### Publishing a Formal Build

To publish a formal build of the NuGet package:

Manually assign a new version number `<major>.<minor>.<patch>` to the build and
update the `VersionPrefix` property accordingly at:

* [SeedLang.csproj](./csharp/src/SeedLang/SeedLang.csproj)
* [SeedLang.Shell.csproj](./csharp/src/SeedLang.Shell/SeedLang.Shell.csprojj)

Create a release branch and tag it with the new version number. In the release
branch:

```shell
cd csharp
dotnet clean
rm src/SeedLang/bin/Release/SeedLang.*.nupkg
dotnet pack -c Release -p:ReleaseTag=release
dotnet nuget push src/SeedLang/bin/Release/SeedLang.*.nupkg --api-key <apikey> \
 --source https://api.nuget.org/v3/index.json
```

An auto-generated timestamp will be attached to the
[semantic](https://semver.org/) version number as its build metadata.
