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
using Antlr4.Runtime.Tree;
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
  internal class BlockInlineTextVisitor : SeedBlockInlineTextBaseVisitor<AstNode> {
    private readonly VisitorHelper _helper;

    public BlockInlineTextVisitor(IList<SyntaxToken> tokens) {
      _helper = new VisitorHelper(tokens);
    }

    public override AstNode VisitSingle_stmt(
        [NotNull] SeedBlockInlineTextParser.Single_stmtContext context) {
      return Visit(context.expr_stmt());
    }

    // Visits an unary expression.
    public override AstNode VisitUnary([NotNull] SeedBlockInlineTextParser.UnaryContext context) {
      if (context.expr() is SeedBlockInlineTextParser.ExprContext expr) {
        return _helper.BuildUnary(context.op, expr, this);
      }
      return null;
    }

    // Visits an add or subtract binary expression.
    //
    // There should be 2 child expression contexts (left and right) in Add_subContext.
    public override AstNode VisitAdd_sub(
        [NotNull] SeedBlockInlineTextParser.Add_subContext context) {
      if (context.expr() is SeedBlockInlineTextParser.ExprContext[] exprs && exprs.Length == 2) {
        return _helper.BuildBinary(context.op, TokenToOperator(context.op), exprs, this);
      }
      return null;
    }

    // Visits a multiply or divide binary expression.
    //
    // There should be 2 child expression contexts (left and right) in Mul_divContext.
    public override AstNode VisitMul_div(
        [NotNull] SeedBlockInlineTextParser.Mul_divContext context) {
      if (context.expr() is SeedBlockInlineTextParser.ExprContext[] exprs && exprs.Length == 2) {
        return _helper.BuildBinary(context.op, TokenToOperator(context.op), exprs, this);
      }
      return null;
    }

    // Visits an identifier.
    public override AstNode VisitIdentifier(
        [NotNull] SeedBlockInlineTextParser.IdentifierContext context) {
      return _helper.BuildIdentifier(context.IDENTIFIER().Symbol);
    }

    // Visits a number expression.
    public override AstNode VisitNumber([NotNull] SeedBlockInlineTextParser.NumberContext context) {
      return _helper.BuildNumber(context.NUMBER().Symbol);
    }

    // Visits a grouping expression.
    //
    // There is no corresponding grouping AST node. The order of the expression node in the AST tree
    // represents the grouping structure.
    // The parser still calls this method with null references or an invalid terminal node when
    // syntax errors happen. Returns a null AST node in this situation.
    public override AstNode VisitGrouping(
        [NotNull] SeedBlockInlineTextParser.GroupingContext context) {
      if (context.expr() is SeedBlockInlineTextParser.ExprContext expr &&
          context.CLOSE_PAREN() is ITerminalNode closeParen && closeParen.Symbol.TokenIndex >= 0) {
        return _helper.BuildGrouping(context.OPEN_PAREN().Symbol, expr, closeParen.Symbol, this);
      }
      return null;
    }

    private static BinaryOperator TokenToOperator(IToken token) {
      switch (token.Type) {
        case SeedBlockInlineTextParser.ADD:
          return BinaryOperator.Add;
        case SeedBlockInlineTextParser.SUB:
          return BinaryOperator.Subtract;
        case SeedBlockInlineTextParser.MUL:
          return BinaryOperator.Multiply;
        case SeedBlockInlineTextParser.DIV:
          return BinaryOperator.Divide;
        default:
          throw new ArgumentException("Unsupported binary operator token.");
      }
    }
  }
}
