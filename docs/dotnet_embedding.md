---
layout: home
nav_order: 5
---

# Embedding in .Net

The following example steps are based on command-line commands. It's also
recommended to use an IDE (e.g., Visual Studio) to complete the same task.

To embed SeedLang in a .net application:

## Install the .Net SDK

Download and install [Microsoft .Net
SDK](https://dotnet.microsoft.com/en-us/download/visual-studio-sdks).

The 6.0 LTS version of .Net is recommended.

## Create your .Net project

```shell
> mkdir seedlang_helloworld
> cd seedlang_helloworld
> dotnet new console
```

## Add the SeedLang NuGet package

```shell
> dotnet add package SeedLang
```

Your .Net project file `seedlang_helloworld.csproj` will look like below after
adding the SeedLang package:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net6.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="SeedLang" Version="0.3.0" />
  </ItemGroup>

</Project>
```

## "Hello, World"

Open the C# code `Program.cs` with our favorite code editor or IDE, and input
the following "Hello, World" program:

```csharp
using SeedLang;

class Program {
  const string _script = @"
msg = 'HelloWorld'
for i in range(len(msg)):
    print(msg[:i+1])
";

  static void Main(string[] args) {
    var engine = new Engine(SeedXLanguage.SeedPython, RunMode.Script);
    engine.Compile(_script, "main");
    engine.Run();
  }
}
```

Run the project:

```shell
> dotnet run
```

The C# code `Program.cs` creates a SeedLang engine then executes the "Hello,
World!" script in the SeedPython language and outputs the following lines to the
console:

```shell
H
He
Hel
Hell
Hello
HelloW
HelloWo
HelloWor
HelloWorl
HelloWorld
```

## Get the diagnosis info when running a script

The above code `Program.cs` simply `engine.Compile` then `engine.Run` a script
string, without any error check.

You can pass in a diagnosis collection to get warnings or errors if there is
anything wrong in the compile-time or the run-time:

```csharp
using SeedLang;
using SeedLang.Common;

class Program {
  const string _script = @"
msg = 'HelloWorld'
for i in range(len(msg)):
    print(msg[:i+1])
";

  static void Main(string[] args) {
    var engine = new Engine(SeedXLanguage.SeedPython, RunMode.Script);
    var collection = new DiagnosticCollection();
    if (!engine.Compile(_script, "main", collection) ||
        !engine.Run(collection)) {
      foreach (var diagnostic in collection.Diagnostics) {
        Console.WriteLine(diagnostic.ToString());
      }
    }
  }
}
```

## Visualization Example

For a full example of SeedLang's visualization features, please see the
following project:

[SeedLangExample:
AppleCalc](https://github.com/SeedV/SeedLangExamples/tree/main/AppleCalc)
