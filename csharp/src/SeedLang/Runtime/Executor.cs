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
using SeedLang.Block;
using SeedLang.Common;
using SeedLang.Interpreter;
using SeedLang.X;

namespace SeedLang.Runtime {
  // An executor class to execute SeedBlock programs or SeedX source code. The information during
  // execution can be visualized by registered visualizers.
  public class Executor {
    private readonly VisualizerCenter _visualizerCenter = new VisualizerCenter();
    private readonly Ast.Executor _executor;
    private readonly VM _vm;

    public Executor() {
      _executor = new Ast.Executor(_visualizerCenter);
      _vm = new VM(_visualizerCenter);
    }

    public void RedirectStdout(TextWriter stdout) {
      _executor.RedirectStdout(stdout);
      _vm.RedirectStdout(stdout);
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
      var localCollection = collection ?? new DiagnosticCollection();
      foreach (AstNode node in Converter.Convert(program, localCollection)) {
        _executor.Run(node);
      }
      return true;
    }

    // Parses SeedX source code into a list of syntax tokens.
    public static IReadOnlyList<SyntaxToken> ParseSyntaxTokens(
        string source, string module, SeedXLanguage language,
        DiagnosticCollection collection = null) {
      if (string.IsNullOrEmpty(source) || module is null) {
        return new List<SyntaxToken>();
      }
      BaseParser parser = MakeParser(language);
      parser.Parse(source, module, collection ?? new DiagnosticCollection(),
                   out _, out IReadOnlyList<SyntaxToken> syntaxTokens);
      return syntaxTokens;
    }

    // Runs or dumps SeedX source code based on the language and run type. Returns string if the
    // runType is DumpAst or Disassemble, otherwise returns null.
    public string Run(string source, string module, SeedXLanguage language, RunType runType,
                      DiagnosticCollection collection = null) {
      if (string.IsNullOrEmpty(source) || module is null) {
        return null;
      }
      try {
        BaseParser parser = MakeParser(language);
        var localCollection = collection ?? new DiagnosticCollection();
        if (!parser.Parse(source, module, localCollection, out AstNode node, out _)) {
          return null;
        }
        switch (runType) {
          case RunType.Ast:
            _executor.Run(node);
            return null;
          case RunType.Bytecode: {
              var compiler = new Compiler();
              Interpreter.Function func = compiler.Compile(node, _vm.Env);
              _vm.Run(func);
              return null;
            }
          case RunType.DumpAst:
            return node.ToString();
          case RunType.Disassemble: {
              var compiler = new Compiler();
              Interpreter.Function func = compiler.Compile(node, _vm.Env);
              return new Disassembler(func).ToString();
            }
          default:
            throw new NotImplementedException($"Unsupported run type: {runType}");
        }
      } catch (DiagnosticException exception) {
        collection?.Report(exception.Diagnostic);
        return null;
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
