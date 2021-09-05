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

using System.Diagnostics;
using SeedLang.Ast;
using SeedLang.Block;
using SeedLang.Common;
using SeedLang.X;

namespace SeedLang.Runtime {
  // An executor class to execute block programs or SeedX source code. The information during
  // execution can be visualized by registered visualizers.
  public class Executor {
    private readonly VisualizerCenter _visualizerCenter = new VisualizerCenter();

    private readonly Ast.Executor _executor;

    public Executor() {
      _executor = new Ast.Executor(_visualizerCenter);
    }

    public void Register<Visualizer>(Visualizer visualizer) {
      _visualizerCenter.Register(visualizer);
    }

    public void Unregister<Visualizer>(Visualizer visualizer) {
      _visualizerCenter.Unregister(visualizer);
    }

    // Runs a block program.
    public bool Run(Program program, DiagnosticCollection collection = null) {
      if (program is null) {
        return false;
      }
      DiagnosticCollection localCollection = collection ?? new DiagnosticCollection();
      foreach (var node in Parser.TryParse(program, localCollection)) {
        _executor.Run(node);
      }
      return true;
    }

    // Runs text-based SeedBlock or SeedX source code based on the given programming language and
    // parse rule.
    public bool Run(string source, string module, ProgrammingLanguage language, ParseRule rule,
                    RunType runType, DiagnosticCollection collection = null) {
      if (string.IsNullOrEmpty(source) || module is null) {
        return false;
      }
      DiagnosticCollection localCollection = collection ?? new DiagnosticCollection();
      BaseParser parser = MakeParser(language);
      if (parser.TryParse(source, module, rule, localCollection, out AstNode node)) {
        switch (runType) {
          case RunType.Ast:
            _executor.Run(node);
            return true;
        }
      }
      return false;
    }

    private static BaseParser MakeParser(ProgrammingLanguage language) {
      switch (language) {
        case ProgrammingLanguage.TextBlock:
          return new BlockTextParser();
        case ProgrammingLanguage.Python:
          return new PythonParser();
        default:
          Debug.Assert(false, $"Not implemented SeedX language: {language}");
          return null;
      }
    }
  }
}
