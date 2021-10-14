# Building and Testing

## SeedLang for C# and Unity

### Pre-requisites

Install [Microsoft .NET](https://dotnet.microsoft.com/download).

### Building

```shell
cd csharp
dotnet build
```

To make a release build:

```shell
cd csharp
dotnet build -c Release
```

### Testing

To run the unit tests:

```shell
cd csharp
dotnet test
```

To start a simple interpreter of SeedLang:

```shell
cd csharp
dotnet run -p src/SeedLang.Shell
```

With the interpreter, you can input and execute SeedX code:

```python
pi = 3.14
r = 10
a = pi * r * r
```
