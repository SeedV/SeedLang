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

using SeedLang.Ast;
using SeedLang.Common;
using SeedLang.Runtime;
using Xunit;

namespace SeedLang.Interpreter.Tests {
  public class VMTests {
    private class MockupVisualizer : IVisualizer<AssignmentEvent>,
                                     IVisualizer<BinaryEvent>,
                                     IVisualizer<EvalEvent> {
      public string Identifier { get; private set; }
      public IValue Left { get; private set; }
      public BinaryOperator Op { get; private set; }
      public IValue Right { get; private set; }
      public IValue Result { get; private set; }
      public Range Range { get; private set; }

      public void On(AssignmentEvent ae) {
        Identifier = ae.Identifier;
        Result = ae.Value;
        Range = ae.Range;
      }

      public void On(BinaryEvent be) {
        Left = be.Left;
        Op = be.Op;
        Right = be.Right;
        Result = be.Result;
        Range = be.Range;
      }

      public void On(EvalEvent ee) {
        Result = ee.Value;
        Range = ee.Range;
      }
    }

    private static TextRange _textRange => new TextRange(0, 1, 2, 3);

    [Fact]
    public void TestBinaryExpressionStatement() {
      var expr = ExpressionStmt(
        Binary(NumberConstant(1), BinaryOperator.Add, NumberConstant(2))
      );

      var compiler = new Compiler();
      (var vm, var visualizer) = NewVMWithVisualizer();
      Function func = compiler.Compile(expr, vm.Env);
      vm.Run(func);

      Assert.Equal(1, visualizer.Left.Number);
      Assert.Equal(BinaryOperator.Add, visualizer.Op);
      Assert.Equal(2, visualizer.Right.Number);
      Assert.Equal(3, visualizer.Result.Number);
      Assert.Equal(_textRange, visualizer.Range);
    }

    [Fact]
    public void TestAssignmentStatement() {
      string name = "name";
      var block = Block(
        Assign(Id(name), NumberConstant(1)),
        ExpressionStmt(Id(name))
      );

      var compiler = new Compiler();
      (var vm, var visualizer) = NewVMWithVisualizer();
      Function func = compiler.Compile(block, vm.Env);
      vm.Run(func);

      Assert.Equal(1, visualizer.Result.Number);
      Assert.Equal(_textRange, visualizer.Range);
    }

    [Fact]
    public void TestUnaryExpressionStatement() {
      var expr = ExpressionStmt(Unary(UnaryOperator.Negative, NumberConstant(1)));

      var compiler = new Compiler();
      (var vm, var visualizer) = NewVMWithVisualizer();
      Function func = compiler.Compile(expr, vm.Env);
      vm.Run(func);

      Assert.Equal(-1, visualizer.Result.Number);
      Assert.Equal(_textRange, visualizer.Range);

      expr = ExpressionStmt(
        Unary(
          UnaryOperator.Negative,
          Binary(NumberConstant(1), BinaryOperator.Add, NumberConstant(2))
        )
      );
      func = compiler.Compile(expr, vm.Env);
      vm.Run(func);

      Assert.Equal(-3, visualizer.Result.Number);
      Assert.Equal(_textRange, visualizer.Range);
    }

    [Fact]
    public void TestFunctionCall() {
      var compiler = new Compiler();
      (var vm, var visualizer) = NewVMWithVisualizer();

      string name = "eval";
      string a = "a";
      string b = "b";
      var block = Block(
        FuncDef(name, Params(a, b), Return(Binary(Id(a), BinaryOperator.Add, Id(b)))),
        ExpressionStmt(
          Call(Id(name), NumberConstant(1), NumberConstant(2))
        )
      );

      Function func = compiler.Compile(block, vm.Env);
      vm.Run(func);

      Assert.Equal(1, visualizer.Left.Number);
      Assert.Equal(BinaryOperator.Add, visualizer.Op);
      Assert.Equal(2, visualizer.Right.Number);
      Assert.Equal(3, visualizer.Result.Number);
      Assert.Equal(_textRange, visualizer.Range);
    }

    [Fact]
    public void TestRecursiveFib() {
      var compiler = new Compiler();
      (var vm, var visualizer) = NewVMWithVisualizer();

      string fib = "fib";
      string n = "n";
      var program = Block(
        FuncDef(fib, new string[] { n }, Block(
          If(
            Boolean(BooleanOperator.Or,
              Comparison(Id(n), CompOps(ComparisonOperator.EqEqual), NumberConstant(1)),
              Comparison(Id(n), CompOps(ComparisonOperator.EqEqual), NumberConstant(2))),
            Return(NumberConstant(1)),
            Return(Binary(
              Call(Id(fib), Binary(Id(n), BinaryOperator.Subtract, NumberConstant(1))),
              BinaryOperator.Add,
              Call(Id(fib), Binary(Id(n), BinaryOperator.Subtract, NumberConstant(2)))
            ))
          )
        )),
        ExpressionStmt(Call(Id(fib), NumberConstant(10)))
      );

      Function func = compiler.Compile(program, vm.Env);
      vm.Run(func);

      Assert.Equal(55, visualizer.Result.Number);
      Assert.Equal(_textRange, visualizer.Range);
    }

    [Fact]
    public void TestNativeFuncCall() {
      var compiler = new Compiler();
      (var vm, var visualizer) = NewVMWithVisualizer();

      var program = ExpressionStmt(
        Call(Id("list"), NumberConstant(1), NumberConstant(2), NumberConstant(3))
      );

      Function func = compiler.Compile(program, vm.Env);
      vm.Run(func);

      Assert.True(visualizer.Result.IsList);
      for (int i = 0; i < visualizer.Result.Count; i++) {
        Assert.Equal(i + 1, visualizer.Result[i].Number);
      }
      Assert.Equal(_textRange, visualizer.Range);
    }

    private static (VM, MockupVisualizer) NewVMWithVisualizer() {
      var visualizer = new MockupVisualizer();
      var visualizerCenter = new VisualizerCenter();
      visualizerCenter.Register(visualizer);
      var vm = new VM(visualizerCenter);
      return (vm, visualizer);
    }

    private static AssignmentStatement Assign(Expression target, Expression expr) {
      return Statement.Assignment(target, expr, _textRange);
    }

    private static BinaryExpression Binary(Expression left, BinaryOperator op, Expression right) {
      return Expression.Binary(left, op, right, _textRange);
    }

    private static BlockStatement Block(params Statement[] statements) {
      return Statement.Block(statements, _textRange);
    }

    private static BooleanExpression Boolean(BooleanOperator op, params Expression[] exprs) {
      return Expression.Boolean(op, exprs, _textRange);
    }

    private static CallExpression Call(Expression func, params Expression[] arguments) {
      return Expression.Call(func, arguments, _textRange);
    }

    private static ComparisonExpression Comparison(Expression first, ComparisonOperator[] ops,
                                                    params Expression[] exprs) {
      return Expression.Comparison(first, ops, exprs, _textRange);
    }

    private static ComparisonOperator[] CompOps(params ComparisonOperator[] ops) {
      return ops;
    }

    private static ExpressionStatement ExpressionStmt(Expression expr) {
      return Statement.Expression(expr, _textRange);
    }

    private static FuncDefStatement FuncDef(string name, string[] parameters, Statement body) {
      return Statement.FuncDef(name, parameters, body, _textRange);
    }

    private static string[] Params(params string[] parameters) {
      return parameters;
    }

    private static IdentifierExpression Id(string name) {
      return Expression.Identifier(name, _textRange);
    }

    private static IfStatement If(Expression test, Statement thenBody, Statement elseBody) {
      return Statement.If(test, thenBody, elseBody, _textRange);
    }

    private static NumberConstantExpression NumberConstant(double value) {
      return Expression.NumberConstant(value, _textRange);
    }

    private static ReturnStatement Return(Expression result) {
      return Statement.Return(result, _textRange);
    }

    private static UnaryExpression Unary(UnaryOperator op, Expression expr) {
      return Expression.Unary(op, expr, _textRange);
    }
  }
}
