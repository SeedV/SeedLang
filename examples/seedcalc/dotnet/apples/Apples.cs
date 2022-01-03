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
using System.Text;

using SeedLang.Common;
using SeedLang.Runtime;

class Apples {
  private class Visualizer :
      IVisualizer<UnaryEvent>, IVisualizer<BinaryEvent>, IVisualizer<EvalEvent> {
    private int _step = 0;

    public void On(UnaryEvent e) {
      _step++;
      string operand = NumberInApples(e.Value.Number);
      string op = _unaryOperatorToString[e.Op];
      string result = NumberInApples(e.Result.Number);
      Console.WriteLine(
          $"STEP {_step}: {op} {operand} = {result}");
    }

    public void On(BinaryEvent e) {
      _step++;
      string operand1 = NumberInApples(e.Left.Number);
      string operand2 = NumberInApples(e.Right.Number);
      string op = _binaryOperatorToString[e.Op];
      string result = NumberInApples(e.Result.Number);
      Console.WriteLine(
          $"STEP {_step}: {operand1} {op} {operand2} = {result}");
    }

    public void On(EvalEvent e) {
      string result = NumberInApples(e.Value.Number);
      Console.WriteLine($"Result: {result}");
    }
  }

  private const string _moduleName = "Apples";

  private const string _welcome =
@"Enter arithmetic expressions to calculate. The integer numbers ranging from 1 to 20 will be
displayed as red apples, unless your console doesn't support Unicode encoding or Unicode fonts.
Enter ""bye"" to exit.";

  private const string _prompt = "] ";
  private const string _bye = "bye";
  private const string _errorDivByZero = "ERROR: division by zero.";
  private const string _errorOverflow = "ERROR: overflow.";
  private const string _errorSyntax = "ERROR: syntax error.";

  private static readonly Dictionary<BinaryOperator, string> _binaryOperatorToString =
      new Dictionary<BinaryOperator, string> {
    { BinaryOperator.Add, "+" },
    { BinaryOperator.Subtract, "-" },
    { BinaryOperator.Multiply, "*" },
    { BinaryOperator.Divide, "/" },
  };

  private static readonly Dictionary<UnaryOperator, string> _unaryOperatorToString =
      new Dictionary<UnaryOperator, string> {
    { UnaryOperator.Positive, "+" },
    { UnaryOperator.Negative, "-" },
  };

  static void Main(string[] args) {
    Console.WriteLine(_welcome);
    Console.Write(_prompt);
    string? line;
    while ((line = Console.ReadLine()) != null) {
      string input = line.ToLower();
      if (input == _bye) {
        break;
      }
      if (input.Length > 0) {
        var executor = new Executor();
        var visualizer = new Visualizer();
        executor.Register(visualizer);
        var collection = new DiagnosticCollection();
        if (!executor.Run(input, _moduleName, SeedXLanguage.SeedCalc, RunType.Ast, collection)) {
          switch (collection.Diagnostics[0].MessageId) {
            case Message.RuntimeErrorDivideByZero:
              Console.WriteLine(_errorDivByZero);
              break;
            case Message.RuntimeErrorOverflow:
              Console.WriteLine(_errorOverflow);
              break;
            default:
              Console.WriteLine(_errorSyntax);
              break;
          }
        }
        executor.Unregister(visualizer);
      }
      Console.Write(_prompt);
    }
  }

  private static string GetApple() {
    string encodingName = Console.OutputEncoding.WebName.ToLower();
    if (encodingName.StartsWith("utf") || encodingName.StartsWith("unicode")) {
      return "\uD83C\uDF4E";  // U+1F34E: Unicode character red apple.
    } else {
      return "@";
    }
  }

  private static string NumberInApples(double number) {
    if (number % 1 == 0 && number >= 1 && number <= 20) {
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < (int)number; i++) {
        sb.Append(GetApple());
      }
      return sb.ToString();
    } else {
      return number.ToString();
    }
  }
}
