# Getting Started

SeedLang supports a number of scripting languages.

For example, SeedCalc is a lightweight sub-language to parse and execute
arithmetic expressions. With SeedCalc, calculator applications can visualize
every single step of arithmetic calculations.

You can run script code in one of the SeedLang scripting languages with
SeedLang.Shell. You can also embed the SeedLang compiler/interpreter/runtime
into your own hosting applications.

Note: the following example commands should be executed in the `csharp` dir:

```shell
cd csharp
```

## Run SeedLang scripts with SeedLang.Shell

### SeedCalc

A typical SeedCalc script is an arithmetic expression. For example:

```python
3.1415926 * (10 + -2.71828) / 7e-3
```

You can start the interactive mode of SeedLang.Shell to input and execute
SeedCalc expressions:

```shell
dotnet run --project src/SeedLang.Shell -- -l SeedCalc
```

You can also run a script file directly:

```shell
dotnet run --project src/SeedLang.Shell -- -l SeedCalc -f ../examples/seedcalc/scripts/arithmetic.calc
```

Or, run the script file with all the shell-based visualizers on:

```shell
dotnet run --project src/SeedLang.Shell -- -l SeedCalc -f ../examples/seedcalc/scripts/arithmetic.calc -v All
```

Shell-based visualizers are the example visualizers predefined by
SeedLang.Shell. By default, only the final `Eval` step is shown to the console.
With command-line option `-v All`, every calculation step of an expression will
be printed to the console. See SeedLang.Shell's
[VisualizerManager.cs](../csharp/src/SeedLang.Shell/VisualizerManager.cs) for
more details.

See also the dir of [SeedCalc Example Scripts](../examples/seedcalc/scripts/).

### SeedPython

Start the interactive mode of SeedLang.Shell to parse and execute SeedPython
statements:

```shell
dotnet run --project src/SeedLang.Shell -- -l SeedPython
```

Run the fibonacci example:

```shell
dotnet run --project src/SeedLang.Shell -- -l SeedPython -f ../examples/seedpython/scripts/fibonacci.py
```

Run the fibonacci example with all the shell-based visualizers on:

```shell
dotnet run --project src/SeedLang.Shell -- -l SeedPython -f ../examples/seedpython/scripts/fibonacci.py -v All
```

See also the dir of [SeedPython Example
Scripts](../examples/seedpython/scripts/).

## Embed SeedLang into .Net console applications

The [Apples](../examples/seedcalc/dotnet/apples) application shows the way how
to embed SeedLang into a .Net console application. It also defines a customized
visualizer to shows the integer numbers ranging from 1 to 20 as corresponding
number of red apples.

Here is an example run of Apples:

```shell
dotnet run --project ../examples/seedcalc/dotnet/apples
] 3+4*(5-3)-4
STEP 1: 🍎🍎🍎🍎🍎 - 🍎🍎🍎 = 🍎🍎
STEP 2: 🍎🍎🍎🍎 * 🍎🍎 = 🍎🍎🍎🍎🍎🍎🍎🍎
STEP 3: 🍎🍎🍎 + 🍎🍎🍎🍎🍎🍎🍎🍎 = 🍎🍎🍎🍎🍎🍎🍎🍎🍎🍎🍎
STEP 4: 🍎🍎🍎🍎🍎🍎🍎🍎🍎🍎🍎 - 🍎🍎🍎🍎 = 🍎🍎🍎🍎🍎🍎🍎
Result: 🍎🍎🍎🍎🍎🍎🍎
```

See the [Source code of Apples](../examples/seedcalc/dotnet/apples/Apples.cs)
for more details.
