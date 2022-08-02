---
layout: page
title: Getting Started
nav_order: 1
---

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

### SeedCalc scripts

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
dotnet run --project src/SeedLang.Shell -- \
-l SeedCalc -f ../example_scripts/seedcalc/arithmetic.calc
```

Or, run the script file with all the shell-based visualizers on:

```shell
dotnet run --project src/SeedLang.Shell -- \
-l SeedCalc -f ../example_scripts/seedcalc/arithmetic.calc -v "*"
```

Shell-based visualizers are the example visualizers predefined by
SeedLang.Shell. With the command-line option `-v "*"`, every calculation step of
an expression will be printed to the console. See SeedLang.Shell's usage info
for more details:

```shell
dotnet run --project src/SeedLang.Shell -- -h
```

See also the dir of [SeedCalc Example Scripts](../example_scripts/seedcalc/).

### SeedPython scripts

Start the interactive mode of SeedLang.Shell to parse and execute SeedPython
statements:

```shell
dotnet run --project src/SeedLang.Shell -- -l SeedPython
```

Run the fibonacci example:

```shell
dotnet run --project src/SeedLang.Shell -- \
-l SeedPython -f ../example_scripts/seedpython/function.py
```

Run the fibonacci example with all the shell-based visualizers on:

```shell
dotnet run --project src/SeedLang.Shell -- \
-l SeedPython -f ../example_scripts/seedpython/function.py -v "*"
```

See also the dir of [SeedPython Example
Scripts](../example_scripts/seedpython/).

## Example applications that embeds the SeedLang engine

The SeedLang engine can be embedded in .Net applications or Unity games. We put
all the example applications and games in the following git repo:

- [https://github.com/SeedV/SeedLangExamples](https://github.com/SeedV/SeedLangExamples)

Please check the `README.md` file of the `SeedLangExamples` for more info.
