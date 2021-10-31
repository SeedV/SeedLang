// Copyright 2021 The Aha001 Team.
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
using SeedLang.Ast;
using SeedLang.Block;
using SeedLang.Common;
using SeedLang.Interpreter;
using SeedLang.X;

namespace SeedLang.Runtime {
  // An executor class to execute SeedBlock programs or SeedX source code. The information during
  // execution can be visualized by registered visualizers.
  public class Executor {
    // Syntax tokens of current parsed source code.
    public IReadOnlyList<SyntaxToken> SyntaxTokens;

    private readonly VisualizerCenter _visualizerCenter = new VisualizerCenter();
    private readonly Ast.Executor _executor;
    private readonly VM _vm;

    // The ast node of current parsed source code.
    private AstNode _node;

    public Executor() {
      _executor = new Ast.Executor(_visualizerCenter);
      _vm = new VM(_visualizerCenter);
    }

    public void Register<Visualizer>(Visualizer visualizer) {
      _visualizerCenter.Register(visualizer);
    }

    public void Unregister<Visualizer>(Visualizer visualizer) {
      _visualizerCenter.Unregister(visualizer);
    }

    // Runs a SeedBlock program.
    public bool Run(Program program, DiagnosticCollection collection = null) {
      if (program is null) {
        return false;
      }
      DiagnosticCollection localCollection = collection ?? new DiagnosticCollection();
      foreach (var node in Converter.Convert(program, localCollection)) {
        _executor.Run(node);
      }
      return true;
    }

    // Parses SeedX source code into an AST tree and a list of syntax tokens.
    public bool Parse(string source, string module, SeedXLanguage language,
                      DiagnosticCollection collection = null) {
      if (string.IsNullOrEmpty(source) || module is null) {
        return false;
      }
      DiagnosticCollection localCollection = collection ?? new DiagnosticCollection();
      BaseParser parser = MakeParser(language);
      return parser.Parse(source, module, localCollection, out _node, out SyntaxTokens);
    }

    // Runs current parsed AST tree or bytecode based on the run type.
    public bool Run(RunType runType, DiagnosticCollection collection = null) {
      if (_node is null) {
        return false;
      }
      try {
        switch (runType) {
          case RunType.Ast:
            _executor.Run(_node);
            return true;
          case RunType.Bytecode:
            var compiler = new Compiler();
            var chunk = compiler.Compile(_node);
            _vm.Run(chunk);
            return true;
          default:
            throw new NotImplementedException($"Unsupported run type: {runType}");
        }
      } catch (DiagnosticException exception) {
        collection?.Report(exception.Diagnostic);
        return false;
      }
    }

    private static BaseParser MakeParser(SeedXLanguage language) {
      switch (language) {
        case SeedXLanguage.Python:
          return new PythonParser();
        default:
          Debug.Assert(false, $"Not implemented SeedX language: {language}");
          return null;
      }
    }
  }
}
