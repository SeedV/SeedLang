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
using SeedLang.Ast;
using SeedLang.Common;
using SeedLang.Runtime;

namespace SeedLang.Tests.Helper {
  internal class AstHelper {
    public static readonly TextRange TextRange = new TextRange(0, 1, 2, 3);

    internal static AssignmentStatement Assign(Expression[][] targets, params Expression[] exprs) {
      return Statement.Assignment(targets, exprs, TextRange);
    }

    internal static Expression[][] ChainedTargets(params Expression[][] targets) {
      return targets;
    }

    internal static Expression[] Targets(params Expression[] targets) {
      return targets;
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

    internal static BreakStatement Break() {
      return Statement.Break(TextRange);
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

    internal static ContinueStatement Continue() {
      return Statement.Continue(TextRange);
    }

    internal static DictExpression Dict(params KeyValuePair<Expression, Expression>[] items) {
      return Expression.Dict(items, TextRange);
    }

    internal static KeyValuePair<Expression, Expression> KeyValue(Expression key,
                                                                  Expression value) {
      return new KeyValuePair<Expression, Expression>(key, value);
    }

    internal static ExpressionStatement ExpressionStmt(Expression expr) {
      return Statement.Expression(expr, TextRange);
    }

    internal static ForInStatement ForIn(IdentifierExpression id, Expression expr, Statement body) {
      return Statement.ForIn(id, expr, body, TextRange);
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

    internal static NilConstantExpression NilConstant() {
      return Expression.NilConstant(TextRange);
    }

    internal static NumberConstantExpression NumberConstant(double value) {
      return Expression.NumberConstant(value, TextRange);
    }

    internal static PassStatement Pass() {
      return Statement.Pass(TextRange);
    }

    internal static ReturnStatement Return(params Expression[] exprs) {
      return Statement.Return(exprs, TextRange);
    }

    internal static StringConstantExpression StringConstant(string value) {
      return Expression.StringConstant(value, TextRange);
    }

    internal static SliceExpression Slice(Expression start, Expression stop, Expression step) {
      return Expression.Slice(start, stop, step, TextRange);
    }

    internal static SubscriptExpression Subscript(Expression expr, Expression index) {
      return Expression.Subscript(expr, index, TextRange);
    }

    internal static TupleExpression Tuple(params Expression[] exprs) {
      return Expression.Tuple(exprs, TextRange);
    }

    internal static UnaryExpression Unary(UnaryOperator op, Expression expr) {
      return Expression.Unary(op, expr, TextRange);
    }

    internal static VTagStatement VTag(VTagStatement.VTagInfo[] vTagInfo,
                                       params Statement[] statements) {
      return Statement.VTag(vTagInfo, statements, TextRange);
    }

    internal static VTagStatement.VTagInfo[] VTagInfos(params VTagStatement.VTagInfo[] vTagInfos) {
      return vTagInfos;
    }

    internal static VTagStatement.VTagInfo VTagInfo(
        string name, params VTagStatement.VTagInfo.Argument[] arguments) {
      return new VTagStatement.VTagInfo(name, arguments);
    }

    internal static VTagStatement.VTagInfo.Argument VTagArg(string text, Expression expr) {
      return new VTagStatement.VTagInfo.Argument(text, expr);
    }

    internal static WhileStatement While(Expression test, Statement body) {
      return Statement.While(test, body, TextRange);
    }
  }
}
