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
  public class Engine {
    public IReadOnlyList<TokenInfo> SemanticTokens => _semanticTokens;

    private readonly SeedXLanguage _language;
    private readonly RunMode _runMode;
    private readonly VM _vm = new VM();

    private IReadOnlyList<TokenInfo> _semanticTokens;
    private AstNode _program;
    private Function _func;

    public Engine(SeedXLanguage language, RunMode runMode) {
      _language = language;
      _runMode = runMode;
    }

    public void RedirectStdout(TextWriter stdout) {
      _vm.RedirectStdout(stdout);
    }

    public void Register<Visualizer>(Visualizer visualizer) {
      _vm.VisualizerCenter.Register(visualizer);
    }

    public void Unregister<Visualizer>(Visualizer visualizer) {
      _vm.VisualizerCenter.Unregister(visualizer);
    }

    // Parses SeedX source code into a list of syntax tokens. Incomplete or invalid source code can
    // also be parsed with this method, where illegal tokens will be marked as Unknown or other
    // relevant types.
    public IReadOnlyList<TokenInfo> ParseSyntaxTokens(string source, string module) {
      if (string.IsNullOrEmpty(source) || module is null) {
        return new List<TokenInfo>();
      }
      BaseParser parser = MakeParser(_language);
      return parser.ParseSyntaxTokens(source);
    }

    // Tries to parse valid SeedX source code into a list of semantic tokens. Returns false and sets
    // semanticTokens to null if the source code is not valid.
    public bool Parse(string source, string module, DiagnosticCollection collection = null) {
      _semanticTokens = null;
      _program = null;
      _func = null;
      if (string.IsNullOrEmpty(source) || module is null) {
        return false;
      }
      try {
        BaseParser parser = MakeParser(_language);
        var localCollection = collection ?? new DiagnosticCollection();
        if (!parser.Parse(source, module, localCollection, out _program, out _semanticTokens)) {
          _semanticTokens = parser.ParseSyntaxTokens(source);
          return false;
        }
        return true;
      } catch (DiagnosticException exception) {
        collection?.Report(exception.Diagnostic);
        return false;
      }
    }

    public bool Compile(DiagnosticCollection collection = null) {
      Debug.Assert(!(_program is null));
      try {
        _func = new Compiler().Compile(_program, _vm.Env, _vm.VisualizerCenter, _runMode);
        return true;
      } catch (DiagnosticException exception) {
        collection?.Report(exception.Diagnostic);
        return false;
      }
    }

    public string DumpAst() {
      Debug.Assert(!(_program is null));
      return _program.ToString();
    }

    public string Disassemble() {
      Debug.Assert(!(_func is null));
      return new Disassembler(_func).ToString();
    }

    // Runs or dumps SeedX source code based on the language and run type. Returns string if the
    // runType is DumpAst or Disassemble, otherwise returns null.
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
