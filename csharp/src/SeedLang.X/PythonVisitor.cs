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
using System.Diagnostics;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using SeedLang.Ast;
using SeedLang.Runtime;

namespace SeedLang.X {
  // The visitor class to visit a SeedPython parse tree and generate the corresponding AST tree.
  //
  // The default implement of SeedPythonBaseVisitor is to visit all the children and return the
  // result of the last one. PythonVisitor overrides the method if the default implement is not
  // correct.
  internal class PythonVisitor : SeedPythonBaseVisitor<AstNode> {
    // Visits a single identifier.
    public override AstNode VisitSingle_identifier(
        [NotNull] SeedPythonParser.Single_identifierContext context) {
      return Visit(context.identifier());
    }

    // Visits a single number.
    public override AstNode VisitSingle_number(
        [NotNull] SeedPythonParser.Single_numberContext context) {
      return Visit(context.number());
    }

    // Visits a single string.
    public override AstNode VisitSingle_string(
        [NotNull] SeedPythonParser.Single_stringContext context) {
      return Visit(context.@string());
    }

    // Visits a single expression.
    public override AstNode VisitSingle_expr(
        [NotNull] SeedPythonParser.Single_exprContext context) {
      return Visit(context.expr());
    }

    // Visits a single statement.
    public override AstNode VisitSingle_stmt(
        [NotNull] SeedPythonParser.Single_stmtContext context) {
      return Visit(context.small_stmt());
    }

    // Visits an add or subtract binary expression.
    //
    // The expr() method of the Add_subContext returns a ExprContext array which contains exact 2
    // items: the left and right ExprContexts.
    public override AstNode VisitAdd_sub([NotNull] SeedPythonParser.Add_subContext context) {
      return BuildBinary(context.op, context.expr());
    }

    // Visits a multiply and divide binary expression.
    //
    // The expr() method of the Add_subContext returns a ExprContext array which contains exact 2
    // items: the left and right ExprContexts.
    public override AstNode VisitMul_div([NotNull] SeedPythonParser.Mul_divContext context) {
      return BuildBinary(context.op, context.expr());
    }

    // Visits an identifier.
    public override AstNode VisitIdentifier([NotNull] SeedPythonParser.IdentifierContext context) {
      return Expression.Identifier(context.GetText());
    }

    // Visits a number expression.
    public override AstNode VisitNumber([NotNull] SeedPythonParser.NumberContext context) {
      return Expression.Number(double.Parse(context.GetText()));
    }

    // Visits a grouping expression.
    //
    // There is no corresponding grouping AST node. The order of the expression node in the AST tree
    // represents the grouping structure.
    public override AstNode VisitGrouping([NotNull] SeedPythonParser.GroupingContext context) {
      return Visit(context.expr());
    }

    // Visits a simple statement.
    //
    // The small_stmt() method of the Simple_stmtContext returns a array which contains all the
    // small statements. There is at least one small statement in it.
    public override AstNode VisitSimple_stmt(
        [NotNull] SeedPythonParser.Simple_stmtContext context) {
      // TODO: parse all the small statements in it, only parse the first one now.
      SeedPythonParser.Small_stmtContext[] smallStatements = context.small_stmt();
      Debug.Assert(smallStatements.Length > 0);
      return Visit(smallStatements[0]);
    }

    // Visits an assignment statement.
    public override AstNode VisitAssignment_stmt(
        [NotNull] SeedPythonParser.Assignment_stmtContext context) {
      var identifier = Visit(context.identifier()) as IdentifierExpression;
      // TODO: if null check is needed in other visit mothods.
      var exprContext = context.expr();
      if (!(exprContext is null)) {
        var expr = Visit(exprContext) as Expression;
        return Statement.Assignment(identifier, expr);
      }
      return null;
    }

    // Visits an eval statement.
    public override AstNode VisitEval_stmt([NotNull] SeedPythonParser.Eval_stmtContext context) {
      var expr = Visit(context.expr()) as Expression;
      return Statement.Eval(expr);
    }

    // Builds a binary expression node from the opToken and exprs.
    //
    // The exprContexts parameter must contain exact 2 items: the left and right ExprContext.
    private BinaryExpression BuildBinary(IToken opToken,
                                         SeedPythonParser.ExprContext[] exprContexts) {
      Debug.Assert(exprContexts.Length == 2);
      var left = Visit(exprContexts[0]) as Expression;
      var right = Visit(exprContexts[1]) as Expression;
      return Expression.Binary(left, TokenToOperator(opToken), right);
    }

    private static BinaryOperator TokenToOperator(IToken token) {
      switch (token.Type) {
        case SeedPythonParser.ADD:
          return BinaryOperator.Add;
        case SeedPythonParser.SUB:
          return BinaryOperator.Subtract;
        case SeedPythonParser.MUL:
          return BinaryOperator.Multiply;
        case SeedPythonParser.DIV:
          return BinaryOperator.Divide;
        default:
          throw new ArgumentException("Unknown operator.");
      }
    }
  }
}
