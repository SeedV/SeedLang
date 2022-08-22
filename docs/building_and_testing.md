---
layout: page
nav_order: 7
---

# Building and Testing

The current implementation of SeedLang runs in the environment of .Net or Unity.

## Pre-requisites

1. Install [Microsoft .NET](https://dotnet.microsoft.com/download).

1. If you want to build and verify the documentation locally, please install
   [Jekyll](https://jekyllrb.com/) as well.

1. Clone the [SeedLang](https://github.com/SeedV/SeedLang) source code then
   enter the top level dir:

```shell
git clone https://github.com/SeedV/SeedLang.git
cd SeedLang
```

## Build SeedLang

```shell
dotnet build csharp
```

To make a release build:

```shell
dotnet build -c Release csharp
```

## Unit Tests

To run the unit tests of SeedLang:

```shell
dotnet test csharp
```

## Run SeedLang.Shell

SeedLang.Shell is a simple interpreter of SeedLang. To start it:

```shell
dotnet run --project csharp/src/SeedLang.Shell
```

To print the usage info of SeedLang.Shell:

```shell
dotnet run --project csharp/src/SeedLang.Shell -- --help
```

## Build and Verify the Documentation

The source files of the SeedLang documentation is located at
[https://github.com/SeedV/SeedLang/tree/main/docs](https://github.com/SeedV/SeedLang/tree/main/docs).

The documentation is automatically built with GitHub Pages/Jekyll and released
at [https://seedv.github.io/SeedLang/](https://seedv.github.io/SeedLang/).

Before submitting any content update of the documentation, please verify the
update by serving it locally with Jekyll:

```shell
pushd docs
bundle exec jekyll serve
popd
```

## Benchmarks

There are a couple of pre-defined benchmarks to verify the performance of
SeedLang.

```shell
dotnet run --project csharp/benchmark/SeedLang.Benchmark
```
