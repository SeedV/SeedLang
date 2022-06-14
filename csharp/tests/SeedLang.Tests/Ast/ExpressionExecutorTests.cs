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
using System.Collections.Immutable;
using FluentAssertions;
using SeedLang.Common;
using SeedLang.Runtime;
using SeedLang.Runtime.HeapObjects;
using SeedLang.Tests.Helper;
using Xunit;

namespace SeedLang.Ast {
  public class ExpressionExecutorTests {
    private class MockupEnvironment : IEnvironment {
      public bool TryGetValueOfVariable(string name, out VMValue value) {
        if (name == "a") {
          value = new VMValue(1);
          return true;
        }
        value = new VMValue();
        return false;
      }
    }

    [Fact]
    public void TestBinary() {
      double left = 11;
      double right = 2;
      var expected = new Dictionary<BinaryOperator, double> {
        [BinaryOperator.Add] = 13,
        [BinaryOperator.Subtract] = 9,
        [BinaryOperator.Multiply] = 22,
        [BinaryOperator.Divide] = 5.5,
        [BinaryOperator.FloorDivide] = 5,
        [BinaryOperator.Power] = 121,
        [BinaryOperator.Modulo] = 1,
      };
      var executor = new ExpressionExecutor(new MockupEnvironment());
      foreach ((var op, var expectedResult) in expected) {
        var binary = AstHelper.Binary(AstHelper.NumberConstant(left),
                                      op,
                                      AstHelper.NumberConstant(right));
        var result = new ExpressionExecutor.Result();
        executor.Visit(binary, result);
        result.Value.Should().Be(new VMValue(expectedResult));
      }
    }

    [Fact]
    public void TestBoolean() {
      var executor = new ExpressionExecutor(new MockupEnvironment());
      var result = new ExpressionExecutor.Result();
      var boolean = AstHelper.Boolean(BooleanOperator.And,
                                      AstHelper.BooleanConstant(true),
                                      AstHelper.BooleanConstant(true),
                                      AstHelper.BooleanConstant(true));
      executor.Visit(boolean, result);
      result.Value.Should().Be(new VMValue(true));

      boolean = AstHelper.Boolean(BooleanOperator.And,
                                  AstHelper.BooleanConstant(true),
                                  AstHelper.BooleanConstant(false),
                                  AstHelper.BooleanConstant(true));
      executor.Visit(boolean, result);
      result.Value.Should().Be(new VMValue(false));

      boolean = AstHelper.Boolean(BooleanOperator.Or,
                                  AstHelper.BooleanConstant(true),
                                  AstHelper.BooleanConstant(false),
                                  AstHelper.BooleanConstant(true));
      executor.Visit(boolean, result);
      result.Value.Should().Be(new VMValue(true));

      boolean = AstHelper.Boolean(BooleanOperator.Or,
                                  AstHelper.BooleanConstant(false),
                                  AstHelper.BooleanConstant(false),
                                  AstHelper.BooleanConstant(false));
      executor.Visit(boolean, result);
      result.Value.Should().Be(new VMValue(false));
    }

    [Fact]
    public void TestBooleanConstant() {
      var executor = new ExpressionExecutor(new MockupEnvironment());
      var result = new ExpressionExecutor.Result();

      var booleanConstant = AstHelper.BooleanConstant(false);
      executor.Visit(booleanConstant, result);
      result.Value.Should().Be(new VMValue(false));

      booleanConstant = AstHelper.BooleanConstant(true);
      executor.Visit(booleanConstant, result);
      result.Value.Should().Be(new VMValue(true));
    }

    [Fact]
    public void TestCall() {
      var executor = new ExpressionExecutor(new MockupEnvironment());
      var result = new ExpressionExecutor.Result();
      var call = AstHelper.Call(AstHelper.Id("func"));
      Action action = () => executor.Visit(call, result);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.UnsupportedEvalSyntax);
    }

    [Fact]
    public void TestComparison() {
      var executor = new ExpressionExecutor(new MockupEnvironment());
      var result = new ExpressionExecutor.Result();
      var comparison = AstHelper.Comparison(AstHelper.NumberConstant(1),
                                            AstHelper.CompOps(ComparisonOperator.Less),
                                            AstHelper.NumberConstant(2));
      executor.Visit(comparison, result);
      result.Value.Should().Be(new VMValue(true));

      comparison = AstHelper.Comparison(AstHelper.NumberConstant(1),
                                        AstHelper.CompOps(ComparisonOperator.Greater),
                                        AstHelper.NumberConstant(2));
      executor.Visit(comparison, result);
      result.Value.Should().Be(new VMValue(false));

      comparison = AstHelper.Comparison(AstHelper.NumberConstant(2),
                                        AstHelper.CompOps(ComparisonOperator.LessEqual),
                                        AstHelper.NumberConstant(2));
      executor.Visit(comparison, result);
      result.Value.Should().Be(new VMValue(true));

      comparison = AstHelper.Comparison(AstHelper.NumberConstant(3),
                                        AstHelper.CompOps(ComparisonOperator.GreaterEqual),
                                        AstHelper.NumberConstant(2));
      executor.Visit(comparison, result);
      result.Value.Should().Be(new VMValue(true));

      comparison = AstHelper.Comparison(AstHelper.NumberConstant(2),
                                        AstHelper.CompOps(ComparisonOperator.EqEqual),
                                        AstHelper.NumberConstant(2));
      executor.Visit(comparison, result);
      result.Value.Should().Be(new VMValue(true));

      comparison = AstHelper.Comparison(AstHelper.NumberConstant(2),
                                        AstHelper.CompOps(ComparisonOperator.NotEqual),
                                        AstHelper.NumberConstant(2));
      executor.Visit(comparison, result);
      result.Value.Should().Be(new VMValue(false));

      comparison = AstHelper.Comparison(AstHelper.StringConstant("abc"),
                                        AstHelper.CompOps(ComparisonOperator.In),
                                        AstHelper.StringConstant("a"));
      executor.Visit(comparison, result);
      result.Value.Should().Be(new VMValue(true));

      comparison = AstHelper.Comparison(AstHelper.NumberConstant(1),
                                        AstHelper.CompOps(ComparisonOperator.Less,
                                                          ComparisonOperator.Less),
                                        AstHelper.NumberConstant(2),
                                        AstHelper.NumberConstant(3));
      executor.Visit(comparison, result);
      result.Value.Should().Be(new VMValue(true));

      comparison = AstHelper.Comparison(AstHelper.NumberConstant(1),
                                        AstHelper.CompOps(ComparisonOperator.GreaterEqual,
                                                          ComparisonOperator.Less),
                                        AstHelper.NumberConstant(2),
                                        AstHelper.NumberConstant(3));
      executor.Visit(comparison, result);
      result.Value.Should().Be(new VMValue(false));
    }

    [Fact]
    public void TestDict() {
      var executor = new ExpressionExecutor(new MockupEnvironment());
      var result = new ExpressionExecutor.Result();

      var dict = AstHelper.Dict(
        AstHelper.KeyValue(AstHelper.StringConstant("a"), AstHelper.NumberConstant(1)),
        AstHelper.KeyValue(AstHelper.StringConstant("b"), AstHelper.NumberConstant(2)),
        AstHelper.KeyValue(AstHelper.StringConstant("c"), AstHelper.NumberConstant(3))
      );
      executor.Visit(dict, result);
      result.Value.Should().Be(new VMValue(new Dictionary<VMValue, VMValue> {
        [new VMValue("a")] = new VMValue(1),
        [new VMValue("b")] = new VMValue(2),
        [new VMValue("c")] = new VMValue(3),
      }));
    }

    [Fact]
    public void TestIdentifier() {
      var executor = new ExpressionExecutor(new MockupEnvironment());
      var result = new ExpressionExecutor.Result();
      var identifier = AstHelper.Id("a");
      executor.Visit(identifier, result);
      result.Value.Should().Be(new VMValue(1));

      identifier = AstHelper.Id("b");
      Action action = () => executor.Visit(identifier, result);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorVariableNotDefined);
    }

    [Fact]
    public void TestList() {
      var executor = new ExpressionExecutor(new MockupEnvironment());
      var result = new ExpressionExecutor.Result();

      var list = AstHelper.List(AstHelper.NumberConstant(1),
                                AstHelper.NumberConstant(2),
                                AstHelper.NumberConstant(3));
      executor.Visit(list, result);
      result.Value.Should().Be(new VMValue(new List<VMValue> {
        new VMValue(1),
        new VMValue(2),
        new VMValue(3),
      }));
    }

    [Fact]
    public void TestNilConstant() {
      var executor = new ExpressionExecutor(new MockupEnvironment());
      var result = new ExpressionExecutor.Result();
      var numberConstant = AstHelper.NilConstant();
      executor.Visit(numberConstant, result);
      result.Value.Should().Be(new VMValue());
    }

    [Fact]
    public void TestNumberConstant() {
      var executor = new ExpressionExecutor(new MockupEnvironment());
      var result = new ExpressionExecutor.Result();
      var numberConstant = AstHelper.NumberConstant(1);
      executor.Visit(numberConstant, result);
      result.Value.Should().Be(new VMValue(1));
    }

    [Fact]
    public void TestSlice() {
      var executor = new ExpressionExecutor(new MockupEnvironment());
      var result = new ExpressionExecutor.Result();

      var stringConstant = AstHelper.Slice(AstHelper.NumberConstant(1),
                                           AstHelper.NumberConstant(2),
                                           AstHelper.NumberConstant(3));
      executor.Visit(stringConstant, result);
      result.Value.AsSlice().Should().BeEquivalentTo(new Slice(1, 2, 3));
    }

    [Fact]
    public void TestStringConstant() {
      var executor = new ExpressionExecutor(new MockupEnvironment());
      var result = new ExpressionExecutor.Result();

      string str = "test string";
      var stringConstant = AstHelper.StringConstant(str);
      executor.Visit(stringConstant, result);
      result.Value.Should().Be(new VMValue(str));
    }

    [Fact]
    public void TestSubscript() {
      var executor = new ExpressionExecutor(new MockupEnvironment());
      var result = new ExpressionExecutor.Result();

      var subscript = AstHelper.Subscript(
        AstHelper.List(AstHelper.NumberConstant(1),
                       AstHelper.NumberConstant(2),
                       AstHelper.NumberConstant(3)),
        AstHelper.NumberConstant(1)
      );
      executor.Visit(subscript, result);
      result.Value.Should().Be(new VMValue(2));
    }

    [Fact]
    public void TestTuple() {
      var executor = new ExpressionExecutor(new MockupEnvironment());
      var result = new ExpressionExecutor.Result();

      var tuple = AstHelper.Tuple(AstHelper.NumberConstant(1),
                                  AstHelper.NumberConstant(2),
                                  AstHelper.NumberConstant(3));
      executor.Visit(tuple, result);
      result.Value.Should().Be(new VMValue(ImmutableArray.Create(new VMValue(1),
                                                                 new VMValue(2),
                                                                 new VMValue(3))));
    }

    [Fact]
    public void TestUnary() {
      var executor = new ExpressionExecutor(new MockupEnvironment());
      var result = new ExpressionExecutor.Result();

      var unary = AstHelper.Unary(UnaryOperator.Negative, AstHelper.NumberConstant(1));
      executor.Visit(unary, result);
      result.Value.Should().Be(new VMValue(-1));

      unary = AstHelper.Unary(UnaryOperator.Not, AstHelper.BooleanConstant(true));
      executor.Visit(unary, result);
      result.Value.Should().Be(new VMValue(false));

      unary = AstHelper.Unary(UnaryOperator.Positive, AstHelper.NumberConstant(1));
      executor.Visit(unary, result);
      result.Value.Should().Be(new VMValue(1));
    }
  }
}
