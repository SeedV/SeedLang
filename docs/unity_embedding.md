---
layout: home
nav_order: 6
---

# Embedding in Unity

## Create Unity project then introduce SeedLang DLLs

First, create an empty 2D or 3D (Built-in, URP and HDRP pipelines are supported)
project with Unity Hub.

Second, introduce SeedLang into your Unity game. There are several ways to add
SeedLang DLLs to Unity project:

### Method 1: Use a pre-built Unity package

Download the newest pre-built SeedLang `.unitypackage` file at [SeedLang's
release page](https://github.com/SeedV/SeedLang/releases).

From Unity's menu, select `Assets / Import Package / Custom Package`, open the
downloaded `.unitypackage` file, then import everything in the package.

### Method 2: Add SeedLang DLLs to the Plugins dir

In Unity, create a folder under the `Assets` folder: `Assets/Plugins`.

Build the SeedLang DLLs yourself from the
[SeedLang](https://github.com/SeedV/SeedLang) repo, or reuse the pre-built DLLs
at
[SeedLangUnityCommon/Runtime/Plugins](https://github.com/SeedV/SeedLangExamples/tree/main/SeedLangUnityCommon/Runtime/Plugins).

A SeedLang build include the main SeedLang DLL and its dependencies:

```shell
SeedLang.dll
Antlr4.Runtime.Standard.dll
System.Collections.Immutable.dll
```

Drag those DLL files into your Unity project's `Assets/Plugins` folder.

### Method 3: Import SeedLang via its NuGet package

SeedLang publishes its NuGet package at
[https://www.nuget.org/packages/SeedLang/](https://www.nuget.org/packages/SeedLang/).

Unity does not support importing NuGet packages directly, but you can use a
third-party plug-in,
[NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity), to achieve this.

See [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity)'s
documentations for more details.

## Use SeedLang in Unity

Create a source code folder in Unity, such as: `Assets/Scripts`.

Add a C# script in the `Assets/Scripts` folder, e.g.: `SeedLangTest.cs`.

Unlike console applications, Unity has no `stdout` pipe to support SeedPython's
`print()` function. For running a SeedLang script in Unity and capturing the
script's outputs, we need to redirect `stdout` to a custom C# class:

```csharp
class ConsoleWriter : TextWriter {
  private readonly StringBuilder _buffer = new StringBuilder();
  public override System.Text.Encoding Encoding => Encoding.Default;
  public override void Write(char c) {
    if (c == '\n') {
      if (_buffer.Length > 0) {
        Debug.Log(_buffer.ToString());
        _buffer.Clear();
      }
    } else {
      _buffer.Append(c);
    }
  }
}
```

The full example code `SeedLangTest.cs`:

```csharp
using System.IO;
using System.Text;
using UnityEngine;

using SeedLang;
using SeedLang.Common;

public class SeedLangTest : MonoBehaviour {
  class ConsoleWriter : TextWriter {
    private readonly StringBuilder _buffer = new StringBuilder();
    public override System.Text.Encoding Encoding => Encoding.Default;
    public override void Write(char c) {
      if (c == '\n') {
        if (_buffer.Length > 0) {
          Debug.Log(_buffer.ToString());
          _buffer.Clear();
        }
      } else {
        _buffer.Append(c);
      }
    }
  }

  const string _script = @"
msg = 'HelloWorld'
for i in range(len(msg)):
    print(msg[:i+1])
";

  void Start() {
    var engine = new Engine(SeedXLanguage.SeedPython, RunMode.Script);
    var consoleWriter = new ConsoleWriter();
    engine.RedirectStdout(consoleWriter);
    engine.Compile(_script, "main");
    engine.Run();
  }
}
```

In Unity, remember to drag `SeedLangTest.cs` onto an object (e.g., a 3D cube or
an empty object such as `GameManager` in your scene), otherwise, Unity will
never run your C# code.

Start the game and open Unity's Console window, where you can find all the
output lines of the "Hello, World" script.

## Register visualizers and use them in Unity games

Once you have registered visualizers to a SeedLang engine, you can trigger or
control Unity animations in your game scene. In order to do that, you can run
the SeedLang engine either in a separate thread or in a Unity coroutine.

See [SeedLangExample:
XyzWaler](https://github.com/SeedV/SeedLangExamples/tree/main/XyzWalker)'s
[CodeExecutor.cs](https://github.com/SeedV/SeedLangExamples/blob/main/XyzWalker/Assets/Src/Scripts/CodeExecutor.cs)
for a detailed demo.
