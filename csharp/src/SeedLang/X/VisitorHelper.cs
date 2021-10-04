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
using Antlr4.Runtime.Tree;
using SeedLang.Ast;
using SeedLang.Common;
using SeedLang.Runtime;

namespace SeedLang.X {
  // A helper class to build AST nodes from parser tree contexts.
  internal class VisitorHelper {
    private TextRange _groupingRange;

    // Builds an assignment statement from the identifier token and the expression context.
    internal static AssignmentStatement BuildAssign(IToken idToken, ParserRuleContext exprContext,
                                                    AbstractParseTreeVisitor<AstNode> visitor) {
      var idRange = RangeOfToken(idToken);
      var identifier = Expression.Identifier(idToken.Text, idRange);
      // TODO: if null check is needed in other visit mothods.
      if (!(exprContext is null)) {
        var expr = visitor.Visit(exprContext) as Expression;
        Debug.Assert(expr.Range is TextRange);
        TextRange range = CombineRanges(idRange, expr.Range as TextRange);
        return Statement.Assignment(identifier, expr, range);
      }
      return null;
    }

    // Builds an eval statement from the eval token and the expression context.
    internal static EvalStatement BuildEval(IToken evalToken, ParserRuleContext exprContext,
                                            AbstractParseTreeVisitor<AstNode> visitor) {
      var evalRange = RangeOfToken(evalToken);
      var expr = visitor.Visit(exprContext) as Expression;
      Debug.Assert(expr.Range is TextRange);
      return Statement.Eval(expr, CombineRanges(evalRange, expr.Range as TextRange));
    }

    // Sets grouping range for sub-expression to use. Only keeps the largest grouping range.
    internal void SetGroupingRange(IToken leftParen, IToken rightParen) {
      if (_groupingRange is null) {
        _groupingRange = CombineRanges(RangeOfToken(leftParen), RangeOfToken(rightParen));
      }
    }

    // Builds a binary expression from the binary operator and expression contexts.
    //
    // The exprContexts parameter must contain exact 2 items: the left and right ExprContext.
    internal BinaryExpression BuildBinary(BinaryOperator op, ParserRuleContext[] exprContexts,
                                          AbstractParseTreeVisitor<AstNode> visitor) {
      TextRange range = _groupingRange;
      _groupingRange = null;

      Debug.Assert(exprContexts.Length == 2);
      var left = visitor.Visit(exprContexts[0]) as Expression;
      var right = visitor.Visit(exprContexts[1]) as Expression;
      if (range is null) {
        Debug.Assert(left.Range is TextRange);
        Debug.Assert(right.Range is TextRange);
        range = CombineRanges(left.Range as TextRange, right.Range as TextRange);
      }
      return Expression.Binary(left, op, right, range);
    }

    // Builds an identifier expresssion from the identifier token.
    internal IdentifierExpression BuildIdentifier(IToken token) {
      TextRange range = _groupingRange is null ? RangeOfToken(token) : _groupingRange;
      _groupingRange = null;
      return Expression.Identifier(token.Text, range);
    }

    // Builds a number constant expresssion from the number token.
    internal NumberConstantExpression BuildNumber(IToken token) {
      TextRange range = _groupingRange is null ? RangeOfToken(token) : _groupingRange;
      _groupingRange = null;
      return Expression.Number(token.Text, range);
    }

    // Builds a string constant expresssion from the string token.
    internal StringConstantExpression BuildString(IToken token) {
      TextRange range = _groupingRange is null ? RangeOfToken(token) : _groupingRange;
      _groupingRange = null;
      return Expression.String(token.Text, range);
    }

    // Builds an unary expresssion from the unary operator token and the expression context.
    internal UnaryExpression BuildUnary(IToken op, ParserRuleContext exprContext,
                                               AbstractParseTreeVisitor<AstNode> visitor) {
      TextRange range = _groupingRange;
      _groupingRange = null;

      var expr = visitor.Visit(exprContext) as Expression;
      if (range is null) {
        TextRange opRange = RangeOfToken(op);
        Debug.Assert(expr.Range is TextRange);
        range = CombineRanges(opRange, expr.Range as TextRange);
      }
      // TODO: handle other unary operators.
      return Expression.Unary(UnaryOperator.Negative, expr, range);
    }

    private static TextRange RangeOfToken(IToken t) {
      return new TextRange(t.Line, t.Column, t.Line, t.Column + t.Text.Length - 1);
    }

    private static TextRange CombineRanges(TextRange begin, TextRange end) {
      return new TextRange(begin.Start.Line, begin.Start.Column, end.End.Line, end.End.Column);
    }
  }
}
