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
  // A Read-Evaluate-Print-Loop class to execute SeedX programs interactively.
  internal sealed class Repl {
    private class Visualizer : IVisualizer<AssignmentEvent>,
                               IVisualizer<BinaryEvent>,
                               IVisualizer<ComparisonEvent>,
                               IVisualizer<EvalEvent> {
      // Current executed source code.
      public SourceCode SourceCode { get; } = new SourceCode();

      private readonly Dictionary<BinaryOperator, string> _binaryOperatorStrings =
          new Dictionary<BinaryOperator, string>() {
            {BinaryOperator.Add, "+"},
            {BinaryOperator.Subtract, "-"},
            {BinaryOperator.Multiply, "*"},
            {BinaryOperator.Divide, "/"},
            {BinaryOperator.FloorDivide, "//"},
            {BinaryOperator.Power, "**"},
            {BinaryOperator.Modulo, "%"},
          };

      private readonly Dictionary<ComparisonOperator, string> _comparisonOperatorStrings =
          new Dictionary<ComparisonOperator, string>() {
            {ComparisonOperator.Less, "<"},
            {ComparisonOperator.Greater, ">"},
            {ComparisonOperator.LessEqual, "<="},
            {ComparisonOperator.GreaterEqual, ">="},
            {ComparisonOperator.EqEqual, "=="},
            {ComparisonOperator.NotEqual, "!="},
          };

      public void On(AssignmentEvent ae) {
        if (ae.Range is TextRange range) {
          SourceCode.WriteSourceWithHighlight(range);
        }
        Console.WriteLine($"{ae.Identifier} = {ae.Value}");
      }

      public void On(BinaryEvent be) {
        if (be.Range is TextRange range) {
          SourceCode.WriteSourceWithHighlight(range);
        }
        Console.WriteLine($"{be.Left} {_binaryOperatorStrings[be.Op]} {be.Right} = {be.Result}");
      }

      public void On(ComparisonEvent ce) {
        if (ce.Range is TextRange range) {
          SourceCode.WriteSourceWithHighlight(range);
        }
        Console.Write($"{ce.First} ");
        for (int i = 0; i < ce.Ops.Length; ++i) {
          string exprString = ce.Values[i] is IValue value ? value.String : "?";
          Console.Write($"{_comparisonOperatorStrings[ce.Ops[i]]} {exprString} ");
        }
        Console.WriteLine($"= {ce.Result}");
      }

      public void On(EvalEvent ee) {
        if (ee.Range is TextRange range) {
          SourceCode.WriteSourceWithHighlight(range);
        }
        Console.WriteLine($"Eval result: {ee.Value}");
      }

    }

    private readonly SeedXLanguage _language;
    private readonly RunType _runType;

    internal Repl(SeedXLanguage language, RunType runType) {
      _language = language;
      _runType = runType;
    }

    internal void Execute() {
      ReadLine.HistoryEnabled = true;
      var visualizer = new Visualizer();
      var executor = new Executor();
      executor.Register(visualizer);
      while (true) {
        visualizer.SourceCode.Source = ReadLine.Read(">>> ");
        if (visualizer.SourceCode.Source == "quit") {
          break;
        }
        IReadOnlyList<SyntaxToken> syntaxTokens =
            Executor.ParseSyntaxTokens(visualizer.SourceCode.Source, "", _language);
        visualizer.SourceCode.WriteSourceWithSyntaxTokens(syntaxTokens);
        Console.WriteLine("---------- Run ----------");
        var runCollection = new DiagnosticCollection();
        executor.Run(visualizer.SourceCode.Source, "", _language, _runType, runCollection);
        foreach (var diagnostic in runCollection.Diagnostics) {
          if (diagnostic.Range is TextRange range) {
            visualizer.SourceCode.WriteSourceWithHighlight(range);
          }
          Console.WriteLine(diagnostic);
        }
        Console.WriteLine();
      }
      executor.Unregister(visualizer);
    }
  }
}
