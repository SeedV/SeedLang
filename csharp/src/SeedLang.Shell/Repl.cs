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
using SeedLang.Common;
using SeedLang.Runtime;

namespace SeedLang.Shell {
  // A Read-Evaluate-Print-Loop class to execute SeedX program interactively.
  internal sealed class Repl {
    private class Visualizer : IVisualizer<BinaryEvent>, IVisualizer<EvalEvent> {
      private readonly Dictionary<BinaryOperator, string> _operatorStrings =
        new Dictionary<BinaryOperator, string>() {
          {BinaryOperator.Add, "+"},
          {BinaryOperator.Subtract, "-"},
          {BinaryOperator.Multiply, "*"},
          {BinaryOperator.Divide, "/"},
          {BinaryOperator.FloorDivide, "//"},
          {BinaryOperator.Power, "**"},
          {BinaryOperator.Modulus, "%"},
        };

      public void On(BinaryEvent e) {
        Console.WriteLine($"{e.Left} {_operatorStrings[e.Op]} {e.Right} = {e.Result}");
      }

      public void On(EvalEvent e) {
        Console.WriteLine($"eval {e.Value}");
      }
    }

    private readonly ProgrammingLanguage _language;
    private readonly ParseRule _rule;
    private readonly RunType _runType;
    private readonly Visualizer _visualizer = new Visualizer();

    internal Repl(ProgrammingLanguage language, ParseRule rule, RunType runType) {
      _language = language;
      _rule = rule;
      _runType = runType;
      Engine.Instance.Subscribe(_visualizer);
    }

    internal void Execute() {
      while (true) {
        Console.Write("> ");
        string line = Console.ReadLine();
        if (line == "quit") {
          break;
        }
        var collection = new DiagnosticCollection();
        if (!Engine.Instance.Run(line, "", _language, _rule, _runType, collection)) {
          foreach (var diagnostic in collection.Diagnostics) {
            Console.WriteLine(diagnostic);
          }
        }
      }
    }
  }
}
