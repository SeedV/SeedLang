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

using System.Collections.Generic;
using System.Collections.Immutable;
using FluentAssertions;
using SeedLang.Runtime;
using SeedLang.Runtime.HeapObjects;
using SeedLang.Tests.Helper;
using Xunit;

namespace SeedLang.Ast {
  public class ExpressionExecutorTests {
    private class MockupEnvironment : IEnvironment {
      public bool GetValueOfVariable(string name, out VMValue value) {
        throw new System.NotImplementedException();
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
