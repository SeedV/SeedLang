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
using FluentAssertions;
using SeedLang.Ast;
using SeedLang.Common;
using SeedLang.Runtime;
using SeedLang.Tests.Helper;
using SeedLang.Visualization;
using SeedLang.X;
using Xunit;

namespace SeedLang.Interpreter.Tests {
  public class VMTests {
    [Fact]
    public void TestBinary() {
      var expr = AstHelper.ExpressionStmt(AstHelper.Binary(AstHelper.NumberConstant(1),
                                                           BinaryOperator.Add,
                                                           AstHelper.NumberConstant(2)));

      (string output, VisualizerHelper vh) = Run(expr, new Type[] { typeof(Event.Binary) });
      output.Should().Be("3" + Environment.NewLine);
      var expected = new string[] {
        $"{AstHelper.TextRange} 1 Add 2 = 3",
      };
      vh.EventStrings.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void TestInComparison() {
      var expr = AstHelper.ExpressionStmt(
        AstHelper.Comparison(AstHelper.NumberConstant(1),
                             AstHelper.CompOps(ComparisonOperator.In),
                             AstHelper.Tuple(AstHelper.NumberConstant(1),
                                             AstHelper.NumberConstant(2)))
      );

      (string output, VisualizerHelper _) = Run(expr, Array.Empty<Type>());
      output.Should().Be("True" + Environment.NewLine);
    }

    [Fact]
    public void TestUnary() {
      var expr = AstHelper.ExpressionStmt(AstHelper.Unary(UnaryOperator.Negative,
                                                          AstHelper.NumberConstant(1)));
      var eventTypes = new Type[] { typeof(Event.Binary), typeof(Event.Unary) };
      (string output, VisualizerHelper vh) = Run(expr, eventTypes);
      output.Should().Be("-1" + Environment.NewLine);
      var expected = new string[] {
        $"{AstHelper.TextRange} Negative 1 = -1",
      };
      vh.EventStrings.Should().BeEquivalentTo(expected);

      expr = AstHelper.ExpressionStmt(AstHelper.Unary(UnaryOperator.Negative,
        AstHelper.Binary(AstHelper.NumberConstant(1),
                         BinaryOperator.Add,
                         AstHelper.NumberConstant(2))
      ));
      (output, vh) = Run(expr, eventTypes);
      output.Should().Be("-3" + Environment.NewLine);
      expected = new string[] {
        $"{AstHelper.TextRange} 1 Add 2 = 3",
        $"{AstHelper.TextRange} Negative 3 = -3",
      };
      vh.EventStrings.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void TestFunctionCall() {
      string name = "add";
      string a = "a";
      string b = "b";
      var program = AstHelper.Block(
        AstHelper.FuncDef(name, AstHelper.Params(AstHelper.Id(a), AstHelper.Id(b)),
          AstHelper.Return(AstHelper.Binary(AstHelper.Id(a),
                                            BinaryOperator.Add,
                                            AstHelper.Id(b))
        )),
        AstHelper.ExpressionStmt(AstHelper.Call(AstHelper.Id(name),
                                                AstHelper.NumberConstant(1),
                                                AstHelper.NumberConstant(2)))
      );
      var eventTypes = new Type[] {
        typeof(Event.Binary),
        typeof(Event.FuncCalled),
        typeof(Event.FuncReturned),
      };
      (string output, VisualizerHelper vh) = Run(program, eventTypes);
      output.Should().Be("3" + Environment.NewLine);
      var expected = new string[] {
        $"{AstHelper.TextRange} add.a:Local 1 Add add.b:Local 2 = 3",
        $"{AstHelper.TextRange} FuncCalled: add(1, 2)",
        $"{AstHelper.TextRange} FuncReturned: add 3",
      };
      vh.EventStrings.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void TestRecursiveFib() {
      string fib = "fib";
      string n = "n";
      var test = AstHelper.Boolean(BooleanOperator.Or,
        AstHelper.Comparison(AstHelper.Id(n), AstHelper.CompOps(ComparisonOperator.EqEqual),
                             AstHelper.NumberConstant(1)),
        AstHelper.Comparison(AstHelper.Id(n), AstHelper.CompOps(ComparisonOperator.EqEqual),
                             AstHelper.NumberConstant(2))
      );
      var trueBlock = AstHelper.Return(AstHelper.NumberConstant(1));
      var falseBlock = AstHelper.Return(AstHelper.Binary(
        AstHelper.Call(AstHelper.Id(fib), AstHelper.Binary(AstHelper.Id(n), BinaryOperator.Subtract,
                                                           AstHelper.NumberConstant(1))),
        BinaryOperator.Add,
        AstHelper.Call(AstHelper.Id(fib), AstHelper.Binary(AstHelper.Id(n), BinaryOperator.Subtract,
                                                           AstHelper.NumberConstant(2)))
      ));
      var program = AstHelper.Block(
        AstHelper.FuncDef(fib, AstHelper.Params(AstHelper.Id(n)), AstHelper.Block(
          AstHelper.If(test, trueBlock, falseBlock)
        )),
        AstHelper.ExpressionStmt(AstHelper.Call(AstHelper.Id(fib), AstHelper.NumberConstant(10)))
      );
      (string output, VisualizerHelper _) = Run(program, Array.Empty<Type>());
      output.Should().Be("55" + Environment.NewLine);
    }

    [Fact]
    public void TestSubscript() {
      var program = AstHelper.ExpressionStmt(AstHelper.Subscript(
        AstHelper.List(AstHelper.NumberConstant(1), AstHelper.NumberConstant(2),
                       AstHelper.NumberConstant(3)),
        AstHelper.NumberConstant(1)
      ));
      (string output, VisualizerHelper _) = Run(program, Array.Empty<Type>());
      output.Should().Be("2" + Environment.NewLine);
    }

    [Fact]
    public void TestSubscriptAssignment() {
      string a = "a";
      var program = AstHelper.Block(
        AstHelper.Assign(AstHelper.ChainedTargets(AstHelper.Targets(AstHelper.Id(a))),
                         AstHelper.List(AstHelper.NumberConstant(1),
                                        AstHelper.NumberConstant(2),
                                        AstHelper.NumberConstant(3))),
        AstHelper.Assign(
          AstHelper.ChainedTargets(AstHelper.Targets(AstHelper.Subscript(AstHelper.Id(a),
                                                     AstHelper.NumberConstant(1)))),
          AstHelper.NumberConstant(5)
        ),
        AstHelper.ExpressionStmt(AstHelper.Subscript(AstHelper.Id(a), AstHelper.NumberConstant(1)))
      );
      (string output, VisualizerHelper vh) = Run(program, new Type[] { typeof(Event.Assignment) });
      output.Should().Be("5" + Environment.NewLine);
      var expected = new string[] {
        $"{AstHelper.TextRange} a:Global[1] = 5",
        $"{AstHelper.TextRange} a:Global = [1, 2, 3]",
      };
      vh.EventStrings.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void TestDict() {
      var program = AstHelper.ExpressionStmt(
        AstHelper.Dict(
          AstHelper.KeyValue(AstHelper.NumberConstant(1), AstHelper.NumberConstant(1)),
          AstHelper.KeyValue(AstHelper.StringConstant("a"), AstHelper.NumberConstant(2))
        )
      );
      (string output, VisualizerHelper _) = Run(program, Array.Empty<Type>());
      output.Should().Be("{1: 1, 'a': 2}" + Environment.NewLine);
    }

    [Fact]
    public void TestTuple() {
      var program = AstHelper.ExpressionStmt(
        AstHelper.Tuple(AstHelper.NumberConstant(1),
                        AstHelper.NumberConstant(2),
                        AstHelper.NumberConstant(3))
      );
      (string output, VisualizerHelper _) = Run(program, Array.Empty<Type>());
      output.Should().Be("(1, 2, 3)" + Environment.NewLine);
    }

    [Fact]
    public void TestAssignment() {
      string name = "name";
      var program = AstHelper.Block(
        AstHelper.Assign(AstHelper.ChainedTargets(AstHelper.Targets(AstHelper.Id(name))),
                         AstHelper.NumberConstant(1)),
        AstHelper.ExpressionStmt(AstHelper.Id(name))
      );
      (string output, VisualizerHelper vh) = Run(program, new Type[] { typeof(Event.Assignment) });
      output.Should().Be("1" + Environment.NewLine);
      var expected = new string[] {
        $"{AstHelper.TextRange} {name}:Global = 1",
      };
      vh.EventStrings.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void TestMultipleAssignment() {
      string a = "a";
      string b = "b";
      var block = AstHelper.Block(
        AstHelper.Assign(
          AstHelper.ChainedTargets(AstHelper.Targets(AstHelper.Id(a), AstHelper.Id(b))),
          AstHelper.NumberConstant(1),
          AstHelper.NumberConstant(2)
        ),
        AstHelper.ExpressionStmt(AstHelper.Id(a)),
        AstHelper.ExpressionStmt(AstHelper.Id(b))
      );
      (string output, VisualizerHelper vh) = Run(block, new Type[] { typeof(Event.Assignment) });
      var expectedOutput = (
        $"1\n" +
        $"2\n"
      ).Replace("\n", Environment.NewLine);
      output.Should().Be(expectedOutput);
      var expected = new string[] {
        $"{AstHelper.TextRange} {a}:Global = 1",
        $"{AstHelper.TextRange} {b}:Global = 2",
      };
      vh.EventStrings.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void TestPackAssignment() {
      string name = "id";
      var block = AstHelper.Block(
        AstHelper.Assign(AstHelper.ChainedTargets(AstHelper.Targets(AstHelper.Id(name))),
                         AstHelper.NumberConstant(1),
                         AstHelper.NumberConstant(2)),
        AstHelper.ExpressionStmt(AstHelper.Id(name))
      );
      (string output, VisualizerHelper vh) = Run(block, new Type[] { typeof(Event.Assignment) });
      output.Should().Be("(1, 2)" + Environment.NewLine);
      var expected = new string[] {
        $"{AstHelper.TextRange} {name}:Global = (1, 2)",
      };
      vh.EventStrings.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void TestUnpackAssignment() {
      string a = "a";
      string b = "b";
      var block = AstHelper.Block(
        AstHelper.Assign(
          AstHelper.ChainedTargets(AstHelper.Targets(AstHelper.Id(a), AstHelper.Id(b))),
          AstHelper.List(AstHelper.NumberConstant(1), AstHelper.NumberConstant(2))
        ),
        AstHelper.ExpressionStmt(AstHelper.Id(a)),
        AstHelper.ExpressionStmt(AstHelper.Id(b))
      );
      (string output, VisualizerHelper _) = Run(block, Array.Empty<Type>());
      var expectedOutput = (
        $"1\n" +
        $"2\n"
      ).Replace("\n", Environment.NewLine);
      output.Should().Be(expectedOutput);
    }

    [Fact]
    public void TestChainedAssignment() {
      string source = @"
a = a[1] = [1, 2, 3]
x, y = z = 1, 2
print(a, x, y, z)
";
      (string output, VisualizerHelper _) = Run(Parse(source), Array.Empty<Type>());
      var expectedOutput = $"[1, [...], 3] 1 2 (1, 2)" + Environment.NewLine;
      output.Should().Be(expectedOutput);
    }

    [Fact]
    public void TestSlice() {
      string source = @"
a = [1, 2, 3, 4, 5]
print(a[1:3])
";
      (string output, VisualizerHelper _) = Run(Parse(source), Array.Empty<Type>());
      output.Should().Be("[2, 3]" + Environment.NewLine);
    }

    private static (string, VisualizerHelper) Run(Statement program,
                                                  IReadOnlyList<Type> eventTypes) {
      var vc = new VisualizerCenter(() => new VMProxy(SeedXLanguage.SeedPython, null));
      var vm = new VM(vc);
      var vh = new VisualizerHelper(eventTypes);
      vh.RegisterToVisualizerCenter(vc);
      var stringWriter = new StringWriter();
      vm.RedirectStdout(stringWriter);
      var compiler = new Compiler();
      Function func = compiler.Compile(program, vm.Env, vc, RunMode.Interactive);
      vm.Run(func);
      return (stringWriter.ToString(), vh);
    }

    private static Statement Parse(string source) {
      new SeedPython().Parse(source, "", new DiagnosticCollection(), out Statement program,
                             out IReadOnlyList<TokenInfo> _).Should().Be(true);
      return program;
    }
  }
}
