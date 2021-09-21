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

using System.Diagnostics;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using SeedLang.Ast;
using SeedLang.Runtime;

namespace SeedLang.Block {
  // The visitor class to visit an inline text of SeedBlock programs and generate the corresponding
  // AST tree.
  //
  // The default implement of SeedBlockBaseVisitor is to visit all the children and return the
  // result of the last one. InlineTextVisitor overrides the method if the default implement is not
  // correct.
  internal class InlineTextVisitor : SeedBlockBaseVisitor<AstNode> {
    // Visits a single identifier.
    public override AstNode VisitSingle_identifier(
        [NotNull] SeedBlockParser.Single_identifierContext context) {
      return Expression.Identifier(context.IDENTIFIER().GetText());
    }

    // Visits a single number.
    public override AstNode VisitSingle_number(
        [NotNull] SeedBlockParser.Single_numberContext context) {
      return Expression.Number(context.NUMBER().GetText());
    }

    // Visits a single string.
    public override AstNode VisitSingle_string(
        [NotNull] SeedBlockParser.Single_stringContext context) {
      return Expression.String(context.STRING().GetText());
    }

    // Visits a single expression.
    public override AstNode VisitSingle_expr(
        [NotNull] SeedBlockParser.Single_exprContext context) {
      return Visit(context.expr());
    }

    // Visits an unary expression.
    public override AstNode VisitUnary([NotNull] SeedBlockParser.UnaryContext context) {
      var expr = Visit(context.expr()) as Expression;
      // TODO: handle other unary operators.
      return Expression.Unary(UnaryOperator.Negative, expr);
    }

    // Visits an add or subtract binary expression.
    //
    // The expr() method of the Add_subContext returns a ExprContext array which contains exact 2
    // items: the left and right ExprContexts.
    public override AstNode VisitAdd_sub([NotNull] SeedBlockParser.Add_subContext context) {
      return BuildBinary(context.op, context.expr());
    }

    // Visits a multiply or divide binary expression.
    //
    // The expr() method of the Add_subContext returns a ExprContext array which contains exact 2
    // items: the left and right ExprContexts.
    public override AstNode VisitMul_div([NotNull] SeedBlockParser.Mul_divContext context) {
      return BuildBinary(context.op, context.expr());
    }

    // Visits an identifier.
    public override AstNode VisitIdentifier([NotNull] SeedBlockParser.IdentifierContext context) {
      return Expression.Identifier(context.GetText());
    }

    // Visits a number expression.
    public override AstNode VisitNumber([NotNull] SeedBlockParser.NumberContext context) {
      return Expression.Number(context.GetText());
    }

    // Visits a grouping expression.
    //
    // There is no corresponding grouping AST node. The order of the expression node in the AST tree
    // represents the grouping structure.
    public override AstNode VisitGrouping([NotNull] SeedBlockParser.GroupingContext context) {
      return Visit(context.expr());
    }

    // Builds a binary expression node from the opToken and exprs.
    //
    // The exprContexts parameter must contain exact 2 items: the left and right ExprContext.
    private BinaryExpression BuildBinary(IToken opToken,
                                         SeedBlockParser.ExprContext[] exprContexts) {
      Debug.Assert(exprContexts.Length == 2);
      var left = Visit(exprContexts[0]) as Expression;
      var right = Visit(exprContexts[1]) as Expression;
      return Expression.Binary(left, TokenToOperator(opToken), right);
    }

    private static BinaryOperator TokenToOperator(IToken token) {
      switch (token.Type) {
        case SeedBlockParser.ADD:
          return BinaryOperator.Add;
        case SeedBlockParser.SUB:
          return BinaryOperator.Subtract;
        case SeedBlockParser.MUL:
          return BinaryOperator.Multiply;
        case SeedBlockParser.DIV:
          return BinaryOperator.Divide;
        default:
          Debug.Fail($"Unknown operator: {token}");
          return default;
      }
    }
  }
}