# Releasing

## SeedLang for C# and Unity

### The NuGet package of SeedLang

[https://www.nuget.org/packages/SeedLang/](https://www.nuget.org/packages/SeedLang/)

### Publishing a pre-release build

To publish a pre-release build of the NuGet package:

```shell
cd csharp
dotnet clean
rm src/SeedLang/bin/Release/SeedLang.*.nupkg
dotnet pack src/SeedLang -c Release -p:ReleaseTag=preview
dotnet nuget push src/SeedLang/bin/Release/SeedLang.*.nupkg --api-key <apikey> \
  --source https://api.nuget.org/v3/index.json
```

`ReleaseTag` can be an arbitrary string tag to indicate the build type.
Recommended tags include `alpha`, `beta` and `preview`.

The `<apikey>` to access NuGet.org is maintained by the administrator of the dev
team.

The [semantic](https://semver.org/) version number `<major>.<minor>.<patch>`
will not change when publishing pre-release builds, while the pre-release tag
will be updated, composed of the release tag and an auto-generated timestamp.
For example:

```text
0.1.2-preview20211013074315
```

### Publishing a formal release build

To publish a formal release build of the NuGet package:

Manually assign a new version number `<major>.<minor>.<patch>` to the release
build.

Prepare a versioning PR to update the `VersionPrefix` property in the following
two project files:

* [SeedLang.csproj](./csharp/src/SeedLang/SeedLang.csproj)
* [SeedLang.Shell.csproj](./csharp/src/SeedLang.Shell/SeedLang.Shell.csproj)

Get the versioning PR approved by the dev team and merged into the main branch.

In GitHub, [create a new release for
SeedLang](https://github.com/SeedV/SeedLang/releases/new) and assign a release
tag to the main branch. The release tag is typically formatted as
`v<major>.<minor>.<patch>`.

Build the SeedLang package and publish it to NuGet:

```shell
cd csharp
dotnet clean
rm src/SeedLang/bin/Release/SeedLang.*.nupkg
dotnet pack src/SeedLang -c Release -p:ReleaseTag=release
dotnet nuget push src/SeedLang/bin/Release/SeedLang.*.nupkg --api-key <apikey> \
 --source https://api.nuget.org/v3/index.json
```

Once `ReleaseTag` is set to `release` as in the above command line, an
auto-generated timestamp will be attached to the [semantic](https://semver.org/)
version number as its build metadata. For example:

```text
0.1.2+20211013074315
```
