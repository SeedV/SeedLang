---
layout: page
title: Building and Testing
nav_order: 3
---

## SeedLang for C# and Unity

### Pre-requisites

Install [Microsoft .NET](https://dotnet.microsoft.com/download).

Note: the following example commands should be executed in the `csharp` dir:

```shell
cd csharp
```

### Building

```shell
dotnet build
```

To make a release build:

```shell
dotnet build -c Release
```

### Testing

To run the unit tests:

```shell
dotnet test
```

To start a simple interpreter of SeedLang:

```shell
dotnet run --project src/SeedLang.Shell
```

To print the usage info of SeedLang.Shell:

```shell
dotnet run --project src/SeedLang.Shell -- --help
```
