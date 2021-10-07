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
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using SeedLang.Ast;
using SeedLang.Common;
using SeedLang.Runtime;
using SeedLang.X;

namespace SeedLang.Block {
  // The visitor class to visit an inline text of SeedBlock programs and generate the corresponding
  // AST tree.
  //
  // The default implement of SeedBlockBaseVisitor is to visit all the children and return the
  // result of the last one. InlineTextVisitor overrides the method if the default implement is not
  // correct.
  internal class InlineTextVisitor : SeedBlockBaseVisitor<AstNode> {
    private readonly VisitorHelper _helper;

    public InlineTextVisitor(IList<SyntaxToken> tokens) {
      _helper = new VisitorHelper(tokens);
    }

    // Visits a single identifier.
    public override AstNode VisitSingle_identifier(
        [NotNull] SeedBlockParser.Single_identifierContext context) {
      VisitorHelper.EnsureChildCountOfContext(context, 2);
      return _helper.BuildIdentifier(context.IDENTIFIER().Symbol);
    }

    // Visits a single number.
    public override AstNode VisitSingle_number(
        [NotNull] SeedBlockParser.Single_numberContext context) {
      VisitorHelper.EnsureChildCountOfContext(context, 2);
      return _helper.BuildNumber(context.NUMBER().Symbol);
    }

    // Visits a single string.
    public override AstNode VisitSingle_string(
        [NotNull] SeedBlockParser.Single_stringContext context) {
      VisitorHelper.EnsureChildCountOfContext(context, 2);
      return _helper.BuildString(context.STRING().Symbol);
    }

    // Visits a single expression.
    public override AstNode VisitSingle_expr(
        [NotNull] SeedBlockParser.Single_exprContext context) {
      VisitorHelper.EnsureChildCountOfContext(context, 2);
      return Visit(context.expr());
    }

    // Visits an unary expression.
    public override AstNode VisitUnary([NotNull] SeedBlockParser.UnaryContext context) {
      VisitorHelper.EnsureChildCountOfContext(context, 2);
      return _helper.BuildUnary(context.op, context.expr(), this);
    }

    // Visits an add or subtract binary expression.
    public override AstNode VisitAdd_sub([NotNull] SeedBlockParser.Add_subContext context) {
      VisitorHelper.EnsureChildCountOfContext(context, 3);
      return _helper.BuildBinary(context.op, TokenToOperator(context.op), context.expr(), this);
    }

    // Visits a multiply or divide binary expression.
    public override AstNode VisitMul_div([NotNull] SeedBlockParser.Mul_divContext context) {
      VisitorHelper.EnsureChildCountOfContext(context, 3);
      return _helper.BuildBinary(context.op, TokenToOperator(context.op), context.expr(), this);
    }

    // Visits an identifier.
    public override AstNode VisitIdentifier([NotNull] SeedBlockParser.IdentifierContext context) {
      VisitorHelper.EnsureChildCountOfContext(context, 1);
      return _helper.BuildIdentifier(context.IDENTIFIER().Symbol);
    }

    // Visits a number expression.
    public override AstNode VisitNumber([NotNull] SeedBlockParser.NumberContext context) {
      VisitorHelper.EnsureChildCountOfContext(context, 1);
      return _helper.BuildNumber(context.NUMBER().Symbol);
    }

    // Visits a grouping expression.
    //
    // There is no corresponding grouping AST node. The order of the expression node in the AST tree
    // represents the grouping structure.
    public override AstNode VisitGrouping([NotNull] SeedBlockParser.GroupingContext context) {
      VisitorHelper.EnsureChildCountOfContext(context, 3);
      return _helper.BuildGrouping(context.OPEN_PAREN().Symbol, context.expr(),
                                   context.CLOSE_PAREN().Symbol, this);
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
          throw new ArgumentException("Unsupported binary operator token.");
      }
    }
  }
}
