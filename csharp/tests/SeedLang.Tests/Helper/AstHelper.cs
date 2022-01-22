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

namespace SeedLang.Tests.Helper {
  internal class AstHelper {
    public static readonly TextRange TextRange = new TextRange(0, 1, 2, 3);

    internal static AssignmentStatement Assign(Expression target, Expression expr) {
      return Statement.Assignment(target, expr, TextRange);
    }

    internal static BinaryExpression Binary(Expression left, BinaryOperator op, Expression right) {
      return Expression.Binary(left, op, right, TextRange);
    }

    internal static BlockStatement Block(params Statement[] statements) {
      return Statement.Block(statements, TextRange);
    }

    internal static BooleanExpression Boolean(BooleanOperator op, params Expression[] exprs) {
      return Expression.Boolean(op, exprs, TextRange);
    }

    internal static BooleanConstantExpression BooleanConstant(bool value) {
      return Expression.BooleanConstant(value, TextRange);
    }

    internal static CallExpression Call(Expression func, params Expression[] arguments) {
      return Expression.Call(func, arguments, TextRange);
    }

    internal static ComparisonExpression Comparison(Expression first, ComparisonOperator[] ops,
                                                    params Expression[] exprs) {
      return Expression.Comparison(first, ops, exprs, TextRange);
    }

    internal static ComparisonOperator[] CompOps(params ComparisonOperator[] ops) {
      return ops;
    }

    internal static ExpressionStatement ExpressionStmt(Expression expr) {
      return Statement.Expression(expr, TextRange);
    }

    internal static FuncDefStatement FuncDef(string name, string[] parameters, Statement body) {
      return Statement.FuncDef(name, parameters, body, TextRange);
    }

    internal static string[] Params(params string[] parameters) {
      return parameters;
    }

    internal static IdentifierExpression Id(string name) {
      return Expression.Identifier(name, TextRange);
    }

    internal static IfStatement If(Expression test, Statement thenBody, Statement elseBody) {
      return Statement.If(test, thenBody, elseBody, TextRange);
    }

    internal static ListExpression List(params Expression[] exprs) {
      return Expression.List(exprs, TextRange);
    }

    internal static NoneConstantExpression NoneConstant() {
      return Expression.NoneConstant(TextRange);
    }

    internal static NumberConstantExpression NumberConstant(double value) {
      return Expression.NumberConstant(value, TextRange);
    }

    internal static ReturnStatement Return(Expression result) {
      return Statement.Return(result, TextRange);
    }

    internal static StringConstantExpression StringConstant(string value) {
      return Expression.StringConstant(value, TextRange);
    }

    internal static SubscriptExpression Subscript(Expression expr, Expression index) {
      return Expression.Subscript(expr, index, TextRange);
    }

    internal static UnaryExpression Unary(UnaryOperator op, Expression expr) {
      return Expression.Unary(op, expr, TextRange);
    }

    internal static WhileStatement While(Expression test, Statement body) {
      return Statement.While(test, body, TextRange);
    }
  }
}
