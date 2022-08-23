---
layout: page
nav_order: 2
---

# Getting Started

## Supported Scripting Languages

As of v0.3, SeedLang supports two scripting languages, SeedCalc and SeedPython.
More scripting languages such as SeedLua, SeedJS are on the way.

### SeedCalc

SeedCalc is a lightweight sub-language to parse and execute arithmetic
expressions. With SeedCalc, calculator applications can visualize every single
step of arithmetic calculations.

An example of SeedCalc script:

```python
3.1415926 * (10 + -2.71828) / 7e-3
```

See [SeedCalc Example
Scripts](https://github.com/SeedV/SeedLang/tree/main/example_scripts/seedcalc).

### SeedPython

SeedPython is a minimal subset of the Python programming language.

An example of SeedPython script:

```python
# Tower of Hanoi.
def move(n, source, target, auxiliary):
    if n <= 0:
        return
    move(n - 1, source, auxiliary, target)
    print('Tower ' + source + ' -> Tower ' + target)
    move(n - 1, auxiliary, target, source)

num = 3
move(num, 'A', 'B', 'C')
```

See [SeedPython Example
Scripts](https://github.com/SeedV/SeedLang/tree/main/example_scripts/seedpython).

See [SeedPython]({{ site.baseurl }}{% link seedpython.md %}) for more details of
the language.

## Install Microsoft .NET

Before scripting with SeedLang, please install [Microsoft
.NET](https://dotnet.microsoft.com/download) first. The LTS version of .NET 6.0
is recommended.

## Run SeedLang.Shell

SeedLang.Shell is an interactive shell program to test and debug SeedLang
scripting programs.

### Run scripts with SeedLang.Shell

To run SeedLang.Shell, please clone the source code of SeedLang then enter its
top level dir:

```shell
git clone https://github.com/SeedV/SeedLang.git
cd SeedLang
```

To start SeedLang.Shell:

```shell
dotnet run --project csharp/src/SeedLang.Shell
```

The default scripting language of SeedLang.Shell is SeedPython. With
SeedLang.Shell running, you can input python code lines and execute them:

```shell
>>> import math
---------- Source ----------
1     import math

---------- Run ----------
>>> print(math.pow(math.e, math.pi))
---------- Source ----------
1     print(math.pow(math.e, math.pi))

---------- Run ----------
23.140692632779263
```

In the shell instance, type "quit" to exit:

```shell
>>> quit
```

To print the usage info of SeedLang.Shell:

```shell
dotnet run --project csharp/src/SeedLang.Shell -- --help
```

### Specify the scripting language

You can use the `-l` argument to specify the scripting language when running
SeedLang.Shell. For example, the following command starts a SeedLang.Shell
instance to run SeedCalc scripts:

```shell
dotnet run --project csharp/src/SeedLang.Shell -- -l SeedCalc
```

In the shell, you may try arithmetic calculations:

```shell
>>> 3.1415926 * (10 + -2.71828) / 7e-3
---------- Source ----------
1     3.1415926 * (10 + -2.71828) / 7e-3

---------- Run ----------
3268.028238181714
```

### Specify the script file to run

You can also run a SeedCalc script file directly, without entering the
interactive shell mode:

```shell
dotnet run --project csharp/src/SeedLang.Shell -- \
-l SeedCalc -f ./example_scripts/seedcalc/arithmetic.calc
```

### Visualize scripts with SeedLang.Shell

One of the key features of SeedLang is full-stack visualization. SeedLang.Shell
demonstrates how to register visualizers to the SeedLang runtime and retrieve
the state updates via the event listener callbacks.

The following command runs SeedLang.Shell to visualize SeedPython scripts, and
enables the `Assignment` and `FuncCalled` Visualizers:

```shell
dotnet run --project csharp/src/SeedLang.Shell -- \
-l SeedPython -v "Assignment,FuncCalled"
```

The output of the command:

```shell
>>> def foo(x):
...   return x * 2
...
---------- Source ----------
1     def foo(x):
2       return x * 2
3

---------- Run ----------
>>> a = foo(3)
---------- Source ----------
1     a = foo(3)

---------- Run ----------
FuncCalled: foo 3
Assign: a:Global = 6
```

The command-line argument `-v` accepts a list of visualizer names. Supported
visualizer names include:

- **Assignment**: triggered when a variable is assigned with a new value.
- **Binary**: triggered when a binary operation is executed.
- **Comparison**: triggered when a comparison is executed.
- **FuncCalled**: triggered when a function is called.
- **FuncReturned**: triggered when a function call is returned.
- **SingleStep**: triggered for each single source code line.
- **VariableDefined**: triggered when a variable is defined.
- **VariableDeleted**: triggered when a variable is deleted from the current
  scope.
- **VTagEntered**: triggered when a V-Tag scope is entered.
- **VTagExited**: triggered when a V-Tag scope is exited.

See [SeedLang Visualization]({{ site.baseurl }}{% link visualization.md %}) for
more info about the visualization API and the definition of V-Tags.

You can use the wildcard `*` to turn on multiple visualizers like below:

```shell
dotnet run --project csharp/src/SeedLang.Shell -- \
-l SeedCalc -f ./example_scripts/seedcalc/arithmetic.calc -v "*"
```

Or, the following command turns on the `FuncCalled` and `FuncReturned`
visualizers when running the SeedPython script `hanoi.py`:

```shell
dotnet run --project csharp/src/SeedLang.Shell -- \
-l SeedPython -f ./example_scripts/seedpython/hanoi.py -v "Func*"
```

The output of the above command clearly shows the function call stack of the
"Tower of Hanoi" program:

```shell
Enabled Visualizers: FuncCalled, FuncReturned

---------- Source ----------
1     def move(n, source, target, auxiliary):
2         if n <= 0:
3             return
4         move(n - 1, source, auxiliary, target)
5         print('Tower ' + source + ' -> Tower ' + target)
6         move(n - 1, auxiliary, target, source)
7
8
9     num = 3
10    move(num, 'A', 'B', 'C')

---------- Run ----------
FuncCalled: move 3, 'A', 'B', 'C'
  FuncCalled: move 2, 'A', 'C', 'B'
    FuncCalled: move 1, 'A', 'B', 'C'
      FuncCalled: move 0, 'A', 'C', 'B'
      FuncReturned: move None
      FuncCalled: print 'Tower A -> Tower B'
Tower A -> Tower B
      FuncReturned: print None
      FuncCalled: move 0, 'C', 'B', 'A'
      FuncReturned: move None
    FuncReturned: move None
    FuncCalled: print 'Tower A -> Tower C'
Tower A -> Tower C
    FuncReturned: print None
    FuncCalled: move 1, 'B', 'C', 'A'
      FuncCalled: move 0, 'B', 'A', 'C'
      FuncReturned: move None
      FuncCalled: print 'Tower B -> Tower C'
Tower B -> Tower C
      FuncReturned: print None
      FuncCalled: move 0, 'A', 'C', 'B'
      FuncReturned: move None
    FuncReturned: move None
  FuncReturned: move None
  FuncCalled: print 'Tower A -> Tower B'
Tower A -> Tower B
  FuncReturned: print None
  FuncCalled: move 2, 'C', 'B', 'A'
    FuncCalled: move 1, 'C', 'A', 'B'
      FuncCalled: move 0, 'C', 'B', 'A'
      FuncReturned: move None
      FuncCalled: print 'Tower C -> Tower A'
Tower C -> Tower A
      FuncReturned: print None
      FuncCalled: move 0, 'B', 'A', 'C'
      FuncReturned: move None
    FuncReturned: move None
    FuncCalled: print 'Tower C -> Tower B'
Tower C -> Tower B
    FuncReturned: print None
    FuncCalled: move 1, 'A', 'B', 'C'
      FuncCalled: move 0, 'A', 'C', 'B'
      FuncReturned: move None
      FuncCalled: print 'Tower A -> Tower B'
Tower A -> Tower B
      FuncReturned: print None
      FuncCalled: move 0, 'C', 'B', 'A'
      FuncReturned: move None
    FuncReturned: move None
  FuncReturned: move None
FuncReturned: move None
```

## Embedding the SeedLang Engine

### Example applications

The SeedLang engine can be embedded in .Net applications or Unity games. We put
all the example applications and games in the following git repo:

- [https://github.com/SeedV/SeedLangExamples](https://github.com/SeedV/SeedLangExamples)

Please check the `README.md` file of the `SeedLangExamples` for more info.

### Embedding SeedLang in .Net applications

Please read [Embedding SeedLang in .Net]({{ site.baseurl }}{% link
dotnet_embedding.md %}).

### Embedding SeedLang in Unity games

Please read [Embedding SeedLang in Unity]({{ site.baseurl }}{% link
unity_embedding.md %}).
