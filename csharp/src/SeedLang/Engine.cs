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
using System.Diagnostics;
using System.IO;
using SeedLang.Ast;
using SeedLang.Common;
using SeedLang.Interpreter;
using SeedLang.X;

namespace SeedLang {
  // Engine is the facade class of the SeedLang core library. It provides interfaces of compiling
  // and executing SeedX programs, registering visualizers, and inspecting the execution status of
  // programs.
  public class Engine {
    // The semantic tokens of the source code. It falls back to syntax tokens when there are parsing
    // or compiling errors.
    public IReadOnlyList<TokenInfo> SemanticTokens => _semanticTokens;

    private readonly SeedXLanguage _language;
    private readonly RunMode _runMode;
    private readonly VM _vm = new VM();

    // The AST tree of the source code.
    private AstNode _astNode;
    // The semantic tokens of the source code.
    private IReadOnlyList<TokenInfo> _semanticTokens;
    // The compiled bytecode function of the source code.
    private Function _func;

    public Engine(SeedXLanguage language, RunMode runMode) {
      _language = language;
      _runMode = runMode;
    }

    // Redirects standard output to the given text writer.
    public void RedirectStdout(TextWriter stdout) {
      _vm.RedirectStdout(stdout);
    }

    // Registers a visualizer into the visualizer center.
    public void Register<Visualizer>(Visualizer visualizer) {
      _vm.VisualizerCenter.Register(visualizer);
    }

    // Un-registers a visualizer from the visualizer center.
    public void Unregister<Visualizer>(Visualizer visualizer) {
      _vm.VisualizerCenter.Unregister(visualizer);
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
    public bool Compile(string source, string module, DiagnosticCollection collection = null) {
      _semanticTokens = null;
      _astNode = null;
      _func = null;
      if (string.IsNullOrEmpty(source) || module is null) {
        return false;
      }
      try {
        BaseParser parser = MakeParser(_language);
        var localCollection = collection ?? new DiagnosticCollection();
        if (!parser.Parse(source, module, localCollection, out _astNode, out _semanticTokens)) {
          return false;
        }
        _func = new Compiler().Compile(_astNode, _vm.Env, _vm.VisualizerCenter, _runMode);
        return true;
      } catch (DiagnosticException exception) {
        collection?.Report(exception.Diagnostic);
        return false;
      }
    }

    // Dumps the AST tree of the source code.
    public string DumpAst() {
      Debug.Assert(!(_astNode is null));
      return _astNode.ToString();
    }

    // Disassembles the compiled bytecode of the source code.
    public string Disassemble() {
      Debug.Assert(!(_func is null));
      return new Disassembler(_func).ToString();
    }

    // Runs the compiled bytecode of the source code.
    public bool Run(DiagnosticCollection collection = null) {
      Debug.Assert(!(_func is null));
      try {
        _vm.Run(_func);
        return true;
      } catch (DiagnosticException exception) {
        collection?.Report(exception.Diagnostic);
        return false;
      }
    }

    private static BaseParser MakeParser(SeedXLanguage language) {
      switch (language) {
        case SeedXLanguage.SeedBlockInlineText:
          return new SeedBlockInlineText();
        case SeedXLanguage.SeedCalc:
          return new SeedCalc();
        case SeedXLanguage.SeedPython:
          return new SeedPython();
        default:
          throw new NotImplementedException($"Unsupported SeedX language: {language}.");
      }
    }
  }
}
