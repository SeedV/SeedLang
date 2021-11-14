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

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using SeedLang.Ast;
using SeedLang.Common;
using SeedLang.Runtime;

namespace SeedLang.X {
  // A helper class to build AST nodes from parser tree contexts.
  internal class VisitorHelper {
    internal delegate CompareOperator ToCompareOperator(ParserRuleContext opContext);
    internal delegate bool GetCompareItems(ParserRuleContext context, out ParserRuleContext left,
                                           out ParserRuleContext op, out ParserRuleContext right);

    private readonly IList<SyntaxToken> _syntaxTokens;
    private TextRange _groupingRange;

    public VisitorHelper(IList<SyntaxToken> tokens) {
      _syntaxTokens = tokens;
    }

    // Builds a binary expression from the binary operator and expression contexts.
    //
    // The exprContexts parameter must contain exact 2 items: the left and right ExprContext.
    internal BinaryExpression BuildBinary(
        ParserRuleContext opContext, ParserRuleContext[] exprContexts,
        System.Func<ParserRuleContext, BinaryOperator> toOperator,
        AbstractParseTreeVisitor<AstNode> visitor) {
      TextRange range = _groupingRange;
      _groupingRange = null;

      Debug.Assert(exprContexts.Length == 2);
      AstNode left = visitor.Visit(exprContexts[0]);
      if (left is Expression leftExpr) {
        IToken opToken = (opContext.GetChild(0) as ITerminalNode).Symbol;
        AddSyntaxToken(SyntaxType.Operator, CodeReferenceUtils.RangeOfToken(opToken));
        AstNode right = visitor.Visit(exprContexts[1]);

        if (right is Expression rightExpr) {
          if (range is null) {
            Debug.Assert(left.Range is TextRange);
            Debug.Assert(right.Range is TextRange);
            range = CodeReferenceUtils.CombineRanges(left.Range as TextRange,
                                                     right.Range as TextRange);
          }
          return Expression.Binary(leftExpr, toOperator(opContext), rightExpr, range);
        }
      }
      return null;
    }

    internal CompareExpression BuildCompare(ParserRuleContext left, ParserRuleContext op,
                                            ParserRuleContext right,
                                            ToCompareOperator toCompareOperator,
                                            GetCompareItems getCompareItems,
                                            AbstractParseTreeVisitor<AstNode> visitor) {
      TextRange range = _groupingRange;
      _groupingRange = null;

      AstNode first = visitor.Visit(left);
      if (first is Expression firstExpr) {
        var ops = new List<CompareOperator>();
        var exprs = new List<Expression>();
        ParserRuleContext opContext = op;
        ParserRuleContext nextContext = right;
        while (!(nextContext is null)) {
          IToken opToken = (opContext.GetChild(0) as ITerminalNode).Symbol;
          AddSyntaxToken(SyntaxType.Operator, CodeReferenceUtils.RangeOfToken(opToken));
          ops.Add(toCompareOperator(opContext));
          bool isNextCompare = getCompareItems(nextContext, out ParserRuleContext nextFirst,
                                               out opContext, out ParserRuleContext nextNext);
          AstNode next = isNextCompare ? visitor.Visit(nextFirst) : visitor.Visit(nextContext);
          if (next is Expression nextExpr) {
            exprs.Add(nextExpr);
          } else {
            return null;
          }
          nextContext = nextNext;
        }
        if (range is null) {
          Debug.Assert(first.Range is TextRange);
          Debug.Assert(exprs.Last().Range is TextRange);
          range = CodeReferenceUtils.CombineRanges(first.Range as TextRange,
                                                   exprs.Last().Range as TextRange);
        }
        return Expression.Compare(firstExpr, ops.ToArray(), exprs.ToArray(), range);
      }
      return null;
    }

    // Builds an unary expresssion from the unary operator token and the expression context.
    internal UnaryExpression BuildUnary(IToken opToken, ParserRuleContext exprContext,
                                        AbstractParseTreeVisitor<AstNode> visitor) {
      TextRange range = _groupingRange;
      _groupingRange = null;

      TextRange opRange = CodeReferenceUtils.RangeOfToken(opToken);
      AddSyntaxToken(SyntaxType.Operator, opRange);

      if (visitor.Visit(exprContext) is Expression expr) {
        if (range is null) {
          Debug.Assert(expr.Range is TextRange);
          range = CodeReferenceUtils.CombineRanges(opRange, expr.Range as TextRange);
        }
        // TODO: handle other unary operators.
        return Expression.Unary(UnaryOperator.Negative, expr, range);
      }
      return null;
    }

    // Sets grouping range for sub-expression to use. Only keeps the largest grouping range.
    internal AstNode BuildGrouping(IToken openParen, ParserRuleContext exprContext,
                                   IToken closeParen, AbstractParseTreeVisitor<AstNode> visitor) {
      TextRange openRange = CodeReferenceUtils.RangeOfToken(openParen);
      TextRange closeRange = CodeReferenceUtils.RangeOfToken(closeParen);
      AddSyntaxToken(SyntaxType.Parenthesis, openRange);

      if (_groupingRange is null) {
        _groupingRange = CodeReferenceUtils.CombineRanges(openRange, closeRange);
      }

      AstNode node = visitor.Visit(exprContext);
      AddSyntaxToken(SyntaxType.Parenthesis, closeRange);
      return node;
    }

    // Builds an identifier expresssion from the identifier token.
    internal IdentifierExpression BuildIdentifier(IToken token) {
      TextRange range = HandleConstantOrVariableExpression(token, SyntaxType.Variable);
      return Expression.Identifier(token.Text, range);
    }

    // Builds a number constant expresssion from the number token.
    internal NumberConstantExpression BuildNumber(IToken token) {
      TextRange range = HandleConstantOrVariableExpression(token, SyntaxType.Number);
      return Expression.Number(token.Text, range);
    }

    // Builds a string constant expresssion from the string token.
    internal StringConstantExpression BuildString(IToken token) {
      TextRange range = HandleConstantOrVariableExpression(token, SyntaxType.String);
      return Expression.String(token.Text, range);
    }

    // Builds an assignment statement from the identifier token and the expression context.
    internal AssignmentStatement BuildAssignStatement(
        IToken idToken, IToken equalToken, ParserRuleContext exprContext,
        AbstractParseTreeVisitor<AstNode> visitor) {
      TextRange idRange = CodeReferenceUtils.RangeOfToken(idToken);
      var identifier = Expression.Identifier(idToken.Text, idRange);
      AddSyntaxToken(SyntaxType.Variable, idRange);
      AddSyntaxToken(SyntaxType.Operator, CodeReferenceUtils.RangeOfToken(equalToken));

      if (visitor.Visit(exprContext) is Expression expr) {
        Debug.Assert(expr.Range is TextRange);
        TextRange range = CodeReferenceUtils.CombineRanges(idRange, expr.Range as TextRange);
        return Statement.Assignment(identifier, expr, range);
      }
      return null;
    }

    // Builds an expression statement from the expression context.
    internal static ExpressionStatement BuildExpressionStatement(
        ParserRuleContext exprContext, AbstractParseTreeVisitor<AstNode> visitor) {
      if (visitor.Visit(exprContext) is Expression expr) {
        return Statement.Expression(expr, expr.Range);
      }
      return null;
    }

    private TextRange HandleConstantOrVariableExpression(IToken token, SyntaxType type) {
      TextRange tokenRange = CodeReferenceUtils.RangeOfToken(token);
      AddSyntaxToken(type, tokenRange);
      TextRange range = _groupingRange is null ? tokenRange : _groupingRange;
      _groupingRange = null;
      return range;
    }

    private void AddSyntaxToken(SyntaxType type, TextRange range) {
      _syntaxTokens.Add(new SyntaxToken(type, range));
    }
  }
}
