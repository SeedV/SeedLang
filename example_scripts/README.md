# Example Scripts of SeedLang

SeedLang supports a few script languages, including:

- SeedCalc: a simple script language for arithmetic calculators.
- SeedPython: a simplified Python language.
- SeedLua: not implemented yet,
- SeedJS: not implemented yet.

In this dir, there are examples script programs that can be run by the SeedLang
Shell. For example, the following command line runs the quick sort program in
SeedPython:

```shell
dotnet run --project csharp/src/SeedLang.Shell -- \
-f example_scripts/seedpython/sorting/quick_sort.py
```
