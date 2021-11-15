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
using System.Diagnostics;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using SeedLang.Ast;
using SeedLang.Common;
using SeedLang.Runtime;
using SeedLang.X;

namespace SeedLang.Block {
  // The visitor class to visit a block inline text of SeedBlock programs and generate the
  // corresponding AST tree.
  //
  // The default implement of SeedBlockInlineTextBaseVisitor is to visit all the children and return
  // the result of the last one. BlockInlineTextVisitor overrides the method if the default
  // implement is not correct.
  internal class BlockInlineTextVisitor : SeedBlockInlineTextBaseVisitor<AstNode> {
    private readonly VisitorHelper _helper;

    public BlockInlineTextVisitor(IList<SyntaxToken> tokens) {
      _helper = new VisitorHelper(tokens);
    }

    // Visits an unary expression.
    public override AstNode VisitUnaryExpression(
        [NotNull] SeedBlockInlineTextParser.UnaryExpressionContext context) {
      if (context.expression() is SeedBlockInlineTextParser.ExpressionContext expr) {
        IToken op = (context.unaryOperator().GetChild(0) as ITerminalNode).Symbol;
        return _helper.BuildUnary(op, expr, this);
      }
      return null;
    }

    // Visits a multiply or divide binary expression.
    //
    // There should be 2 child expression contexts (left and right) in MulDivExpressionContext.
    public override AstNode VisitMulDivExpression(
        [NotNull] SeedBlockInlineTextParser.MulDivExpressionContext context) {
      if (context.expression() is SeedBlockInlineTextParser.ExpressionContext[] exprs &&
          exprs.Length == 2) {
        ParserRuleContext op = context.mulDivOperator();
        return _helper.BuildBinary(exprs[0], op, exprs[1], ToBinaryOperator, this);
      }
      return null;
    }

    // Visits an add or subtract binary expression.
    //
    // There should be 2 child expression contexts (left and right) in AddSubExpressionContext.
    public override AstNode VisitAddSubExpression(
        [NotNull] SeedBlockInlineTextParser.AddSubExpressionContext context) {
      if (context.expression() is SeedBlockInlineTextParser.ExpressionContext[] exprs &&
          exprs.Length == 2) {
        ParserRuleContext op = context.addSubOperator();
        return _helper.BuildBinary(exprs[0], op, exprs[1], ToBinaryOperator, this);
      }
      return null;
    }

    // Visits a compare expression.
    public override AstNode VisitComapreExpression(
        [NotNull] SeedBlockInlineTextParser.ComapreExpressionContext context) {
      if (GetCompareItems(context, out ParserRuleContext left, out ParserRuleContext op,
                          out ParserRuleContext right)) {
        return _helper.BuildCompare(left, op, right, ToCompareOperator, GetCompareItems, this);
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
      if (context.expression() is SeedBlockInlineTextParser.ExpressionContext expr &&
          context.CLOSE_PAREN() is ITerminalNode closeParen && closeParen.Symbol.TokenIndex >= 0) {
        return _helper.BuildGrouping(context.OPEN_PAREN().Symbol, expr, closeParen.Symbol, this);
      }
      return null;
    }

    public override AstNode VisitSingleStatement(
        [NotNull] SeedBlockInlineTextParser.SingleStatementContext context) {
      return Visit(context.expressionStatement());
    }

    private static bool GetCompareItems(ParserRuleContext context, out ParserRuleContext left,
                                        out ParserRuleContext op, out ParserRuleContext right) {
      if (context is SeedBlockInlineTextParser.ComapreExpressionContext compare) {
        if (compare.expression() is SeedBlockInlineTextParser.ExpressionContext[] exprs &&
            compare.compareOperator() is SeedBlockInlineTextParser.CompareOperatorContext[] ops &&
            ops.Length == 1 && exprs.Length == 2) {
          left = exprs[0];
          op = ops[0];
          right = exprs[1];
          return true;
        }
      }
      left = op = right = null;
      return false;
    }

    private static BinaryOperator ToBinaryOperator(ParserRuleContext context) {
      Debug.Assert(context.ChildCount == 1 && context.GetChild(0) is ITerminalNode);
      int tokenType = (context.GetChild(0) as ITerminalNode).Symbol.Type;
      switch (tokenType) {
        case SeedBlockInlineTextParser.ADD:
          return BinaryOperator.Add;
        case SeedBlockInlineTextParser.SUB:
          return BinaryOperator.Subtract;
        case SeedBlockInlineTextParser.MUL:
          return BinaryOperator.Multiply;
        case SeedBlockInlineTextParser.DIV:
          return BinaryOperator.Divide;
        default:
          throw new NotImplementedException($"Unsupported compare operator token: {tokenType}.");
      }
    }
    private static CompareOperator ToCompareOperator(ParserRuleContext context) {
      Debug.Assert(context.ChildCount == 1 && context.GetChild(0) is ITerminalNode);
      int tokenType = (context.GetChild(0) as ITerminalNode).Symbol.Type;
      switch (tokenType) {
        case SeedBlockInlineTextParser.LESS:
          return CompareOperator.Less;
        case SeedBlockInlineTextParser.GREAT:
          return CompareOperator.Great;
        case SeedBlockInlineTextParser.LESSEQUAL:
          return CompareOperator.LessEqual;
        case SeedBlockInlineTextParser.GREATEQUAL:
          return CompareOperator.GreatEqual;
        case SeedBlockInlineTextParser.EQUALEQUAL:
          return CompareOperator.EqualEqual;
        case SeedBlockInlineTextParser.NOTEQUAL:
          return CompareOperator.NotEqual;
        default:
          throw new NotImplementedException($"Unsupported compare operator token: {tokenType}.");
      }
    }
  }
}
