// Copyright 2021-2022 The SeedV Lab.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using SeedLang.Ast;
using SeedLang.Common;
using SeedLang.Interpreter;
using SeedLang.Runtime;
using SeedLang.Visualization;
using SeedLang.X;

namespace SeedLang {
  /// <summary>
  /// Engine is the facade class of the SeedLang core library. It provides interfaces of compiling
  /// and executing SeedX programs, registering visualizers, and inspecting the execution status of
  /// programs.
  /// </summary>
  public class Engine {
    private readonly SeedXLanguage _language;
    private readonly RunMode _runMode;
    private readonly VisualizerCenter _visualizerCenter;
    private readonly VM _vm;

    /// <summary>
    /// The flag to indicate if variable tracking is enabled. The SeedVM will track information of
    /// all global and local variables if It is set to true explicitly or any variable related
    /// visualizers like "VariableDefined", "VariableDeleted" etc. are registered.
    /// </summary>
    public bool IsVariableTrackingEnabled {
      get {
        return _visualizerCenter.IsVariableTrackingEnabled;
      }
      set {
        _visualizerCenter.IsVariableTrackingEnabled = value;
      }
    }

    /// <summary>
    /// If a program is compiled by the engine.
    /// </summary>
    public bool IsProgramCompiled => !(_func is null);
    /// <summary>
    /// If the engine is running.
    /// </summary>
    public bool IsRunning => _vm.IsRunning;
    /// <summary>
    /// If the engine is paused.
    /// </summary>
    public bool IsPaused => _vm.IsPaused;
    /// <summary>
    /// If the engine is stopped.
    /// </summary>
    public bool IsStopped => _vm.IsStopped;


    /// <summary>
    /// The semantic tokens of the source code. It falls back to syntax tokens when there are 
    /// parsing or compiling errors.
    /// </summary>
    public IReadOnlyList<TokenInfo> SemanticTokens => _semanticTokens;

    // The AST tree of the program.
    private Statement _astTree;
    // The semantic tokens of the source code.
    private IReadOnlyList<TokenInfo> _semanticTokens;
    // The module of the source code.
    private Module _module;
    // The compiled bytecode function of the source code.
    private Function _func;

    /// <summary>
    /// Initializes a new instance of the Engine class.
    /// </summary>
    /// <param name="language">The SeedLang language.</param>
    /// <param name="runMode">Running mode.</param>
    public Engine(SeedXLanguage language, RunMode runMode) {
      _language = language;
      _runMode = runMode;
      _visualizerCenter = new VisualizerCenter(MakeVMProxy);
      _vm = new VM(_visualizerCenter);
    }

    /// <summary>
    /// Redirects standard output to the specified text writer.
    /// </summary>
    /// <param name="writer">The text writer to redirect for standard output.</param>
    public void RedirectStdout(TextWriter writer) {
      _vm.RedirectStdout(writer);
    }

    // Registers a visualizer into the visualizer center.
    public void Register<Visualizer>(Visualizer visualizer) {
      _visualizerCenter.Register(visualizer);
    }

    // Un-registers a visualizer from the visualizer center.
    public void Unregister<Visualizer>(Visualizer visualizer) {
      _visualizerCenter.Unregister(visualizer);
    }

    // Parses SeedX source code into a list of syntax tokens. Incomplete or invalid source code can
    // also be parsed with this method, where illegal tokens will be marked as Unknown or other
    // relevant types. It can be used to get the syntax tokens quickly without parsing and compiling
    // the source code.
    public IReadOnlyList<TokenInfo> ParseSyntaxTokens(string source, string module) {
      if (string.IsNullOrEmpty(source) || module is null) {
        return new List<TokenInfo>();
      }
      return MakeParser(_language).ParseSyntaxTokens(source);
    }

    // Parses and compiles valid SeedX source code into AST node, semantic tokens and bytecode
    // function. Returns false and sets them to null if the source code is not valid.
    public bool Compile(string source, string moduleName, DiagnosticCollection collection = null) {
      _semanticTokens = null;
      _astTree = null;
      _func = null;
      if (string.IsNullOrEmpty(source) || moduleName is null) {
        return false;
      }
      try {
        BaseParser parser = MakeParser(_language);
        if (!parser.Parse(source, moduleName, collection ?? new DiagnosticCollection(),
                          out _astTree, out _semanticTokens)) {
          return false;
        }
        if (_module == null || _runMode == RunMode.Script) {
          _module = Module.Create(moduleName);
        }
        _func = new Compiler().Compile(_astTree, _module, _visualizerCenter, _runMode);
        return true;
      } catch (DiagnosticException exception) {
        collection?.Report(exception.Diagnostic);
        return false;
      }
    }

    // Dumps the AST tree of the source code.
    public bool DumpAst(out string result, DiagnosticCollection collection = null) {
      if (_astTree is null) {
        result = null;
        collection?.Report(SystemReporters.SeedLang, Severity.Error, "", null,
                           Message.EngineProgramNotCompiled);
        return false;
      }
      result = _astTree.ToString();
      return true;
    }

    // Disassembles the compiled bytecode of the source code.
    public bool Disassemble(out string result, DiagnosticCollection collection = null) {
      if (!IsProgramCompiled) {
        result = null;
        collection?.Report(SystemReporters.SeedLang, Severity.Error, "", null,
                           Message.EngineProgramNotCompiled);
        return false;
      }
      result = new Disassembler(_func).ToString();
      return true;
    }

    // Runs the compiled bytecode of the source code.
    public bool Run(DiagnosticCollection collection = null) {
      if (!IsProgramCompiled) {
        collection?.Report(SystemReporters.SeedLang, Severity.Error, "", null,
                           Message.EngineProgramNotCompiled);
        return false;
      }
      try {
        if (!_vm.IsStopped) {
          _vm.Stop();
        }
        _vm.Run(_module, _func);
        return true;
      } catch (DiagnosticException exception) {
        collection?.Report(exception.Diagnostic);
        return false;
      }
    }

    // Continues execution of current program.
    //
    // The execution must be paused from the callback function of visualization events before
    // calling this function.
    public bool Continue(DiagnosticCollection collection = null) {
      if (!IsPaused) {
        collection?.Report(SystemReporters.SeedLang, Severity.Error, "", null,
                           Message.EngineNotPaused);
        return false;
      }
      try {
        _vm.Continue();
        return true;
      } catch (DiagnosticException exception) {
        collection?.Report(exception.Diagnostic);
        return false;
      }
    }

    // Stops execution of current program.
    //
    // Multiple-thread is not support now. This function can only be called from the same thread
    // when execution is paused.
    // TODO: Add multiple-thread support.
    public bool Stop() {
      _vm.Stop();
      return true;
    }

    internal static BaseParser MakeParser(SeedXLanguage language) {
      return language switch {
        SeedXLanguage.SeedCalc => new SeedCalc(),
        SeedXLanguage.SeedPython => new SeedPython(),
        _ => throw new NotImplementedException($"Unsupported SeedX language: {language}."),
      };
    }

    private IVMProxy MakeVMProxy() {
      return new VMProxy(_language, _vm);
    }
  }
}
