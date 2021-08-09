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
using SeedLang.Common;
using SeedLang.X;

namespace SeedLang {
  // A facade class to implement the SeedLang engine interface.
  //
  // This is a singleton class. The interfaces include validating and running a SeedBlock or SeedX
  // program, registering observers to visualize program execution, etc.
  public sealed class Engine : IEngine {
    private static readonly Lazy<IEngine> _lazyInstance = new Lazy<IEngine>(() => new Engine());

    public static IEngine Instance => _lazyInstance.Value;

    public IEnumerable<string> BinaryOperators {
      get {
        // TODO: implement it.
        return new List<string>();
      }
    }

    private Engine() {
    }

    public bool Run(string source, string module, ProgrammingLanguage language, ParseRule rule,
                    RunType runType, DiagnosticCollection collection = null) {
      if (runType == RunType.DryRun) {
        switch (language) {
          case ProgrammingLanguage.Python:
            return PythonParser.Validate(source, module, rule, collection);
          default:
            Debug.Assert(false, $"Not implemented SeedX language: {language}");
            return false;
        }
      }

      AstNode node = null;
      switch (language) {
        case ProgrammingLanguage.Python:
          node = PythonParser.Parse(source, module, rule, collection);
          break;
        default:
          Debug.Assert(false, $"Not implemented SeedX language: {language}");
          break;
      }
      // TODO: run AST node or compile it into bytecode and run it.
      Console.WriteLine(node);
      return true;
    }
  }
}
