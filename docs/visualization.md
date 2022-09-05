---
layout: home
nav_order: 4
---

# Visualization API

SeedLang supports full-stack visualization with a set of event-based .Net APIs.

## Engine

Engine is the facade class of the SeedLang core library. It provides interfaces of compiling and executing SeedX programs, registering visualizers, and inspecting the execution status of programs.

### Constructor(SeedLang.SeedXLanguage,SeedLang.RunMode)

Initializes a new instance of the Engine class.

### RedirectStdout(System.IO.TextWriter)

Redirects standard output to the specified text writer.

### IsVariableTrackingEnabled

The flag to indicate if variable tracking is enabled. The SeedVM will track information of all global and local variables if It is set to true explicitly or any variable related visualizers like "VariableDefined", "VariableDeleted" etc. are registered.

### IsProgramCompiled

If a program is compiled by the engine.

### IsRunning

If the engine is running.

### IsPaused

If the engine is paused.

### IsStopped

If the engine is stopped.

### SemanticTokens

The semantic tokens of the source code. It falls back to syntax tokens when there are parsing or compiling errors.

## SeedXLanguage

Supported SeedX languages.

### SeedCalc

A lightweight sub-language to parse and execute arithmetic expressions.

### SeedPython

A minimal subset of the Python programming language.
