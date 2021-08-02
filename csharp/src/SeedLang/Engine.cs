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
using SeedLang.Ast;
using SeedLang.Common;
using SeedLang.X;

namespace SeedLang {
  // A facade class to provide an unified interface for the SeedLang system.
  //
  // This is a singleton class. The interfaces include validating and running a SeedBlock or SeedX
  // program, registering observers to visualize program execution, etc.
  public sealed class Engine {
    private static readonly Lazy<Engine> _engine = new Lazy<Engine>(() => new Engine());

    public static Engine Instance {
      get {
        return _engine.Value;
      }
    }

    private Engine() {
    }

    // Runs a statement and returns diagnostic information collected during parsing and execution.
    public static DiagnosticCollection RunStatement(string source) {
      var collection = new DiagnosticCollection();
      AstNode node = PythonParser.Parse(source, ParseRule.Statement, collection);
      if (!(node is null) && collection.Diagnostics.Count == 0) {
        // TODO: implement the execution of statements.
        Console.Write($"AST: {node}");
      }
      return collection;
    }
  }
}
