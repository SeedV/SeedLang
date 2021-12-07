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
    internal delegate ComparisonOperator ToComparisonOperator(IToken token);

    private readonly IList<SyntaxToken> _syntaxTokens;
    private TextRange _groupingRange;

    public VisitorHelper(IList<SyntaxToken> tokens) {
      _syntaxTokens = tokens;
    }

    // Builds a binary expression.
    internal BinaryExpression BuildBinary(ParserRuleContext leftContext, IToken opToken,
                                          BinaryOperator op, ParserRuleContext rightContext,
                                          AbstractParseTreeVisitor<AstNode> visitor) {
      TextRange range = _groupingRange;
      _groupingRange = null;

      AstNode left = visitor.Visit(leftContext);
      if (left is Expression leftExpr) {
        AddSyntaxToken(SyntaxType.Operator, CodeReferenceUtils.RangeOfToken(opToken));
        AstNode right = visitor.Visit(rightContext);

        if (right is Expression rightExpr) {
          if (range is null) {
            Debug.Assert(left.Range is TextRange);
            Debug.Assert(right.Range is TextRange);
            range = CodeReferenceUtils.CombineRanges(left.Range as TextRange,
                                                     right.Range as TextRange);
          }
          return Expression.Binary(leftExpr, op, rightExpr, range);
        }
      }
      return null;
    }

    // Builds a comparison expression.
    internal ComparisonExpression BuildComparison(ParserRuleContext left,
                                                  ParserRuleContext[] rightPairs,
                                                  ToComparisonOperator toComparisonOperator,
                                                  AbstractParseTreeVisitor<AstNode> visitor) {
      TextRange range = _groupingRange;
      _groupingRange = null;

      AstNode first = visitor.Visit(left);
      if (first is Expression firstExpr) {
        var ops = new List<ComparisonOperator>();
        var exprs = new List<Expression>();
        foreach (ParserRuleContext rightContext in rightPairs) {
          if (rightContext.ChildCount == 2 && rightContext.GetChild(0) is ITerminalNode opNode) {
            IToken opToken = opNode.Symbol;
            AddSyntaxToken(SyntaxType.Operator, CodeReferenceUtils.RangeOfToken(opToken));
            ops.Add(toComparisonOperator(opToken));
            AstNode right = visitor.Visit(rightContext.GetChild(1));
            if (right is Expression rightExpr) {
              exprs.Add(rightExpr);
            } else {
              return null;
            }
          } else {
            return null;
          }
        }
        if (range is null) {
          Debug.Assert(first.Range is TextRange);
          Debug.Assert(exprs.Last().Range is TextRange);
          range = CodeReferenceUtils.CombineRanges(first.Range as TextRange,
                                                   exprs.Last().Range as TextRange);
        }
        return Expression.Comparison(firstExpr, ops.ToArray(), exprs.ToArray(), range);
      }
      return null;
    }

    // Builds an unary expresssion.
    internal UnaryExpression BuildUnary(IToken opToken, UnaryOperator op,
                                        ParserRuleContext exprContext,
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
        return Expression.Unary(op, expr, range);
      }
      return null;
    }

    // Builds a grouping expressions, and sets grouping range for sub-expression to use. Only keeps
    // the largest grouping range.
    // There is no corresponding grouping AST node. The order of the expression node in the AST tree
    // represents the grouping structure.
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

    // Builds an identifier expresssion.
    internal IdentifierExpression BuildIdentifier(IToken token) {
      TextRange range = HandleConstantOrVariableExpression(token, SyntaxType.Variable);
      return Expression.Identifier(token.Text, range);
    }

    // Builds a boolean costant expression.
    internal BooleanConstantExpression BuildBooleanConstant(IToken token, bool value) {
      TextRange range = HandleConstantOrVariableExpression(token, SyntaxType.Boolean);
      return Expression.BooleanConstant(value, range);
    }

    // Builds a number constant expresssion.
    internal NumberConstantExpression BuildNumberConstant(IToken token) {
      TextRange range = HandleConstantOrVariableExpression(token, SyntaxType.Number);
      return Expression.NumberConstant(token.Text, range);
    }

    // Builds a string constant expresssion.
    internal StringConstantExpression BuildStringConstant(IToken token) {
      TextRange range = HandleConstantOrVariableExpression(token, SyntaxType.String);
      return Expression.StringConstant(token.Text, range);
    }

    // Builds an assignment statement.
    internal AssignmentStatement BuildAssignment(IToken idToken, IToken equalToken,
                                                 ParserRuleContext exprContext,
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

    // Builds an block statement.
    internal static BlockStatement BuildBlock(ParserRuleContext[] statementContexts,
                                              AbstractParseTreeVisitor<AstNode> visitor) {
      var statements = new Statement[statementContexts.Length];
      for (int i = 0; i < statementContexts.Length; ++i) {
        statements[i] = visitor.Visit(statementContexts[i]) as Statement;
      }
      Debug.Assert(statements.Length > 0);
      Statement first = statements[0];
      Statement last = statements[statements.Length - 1];
      Debug.Assert(first.Range is TextRange && last.Range is TextRange);
      Range range = CodeReferenceUtils.CombineRanges(first.Range as TextRange,
                                                     last.Range as TextRange);
      return new BlockStatement(statements, range);
    }

    // Builds an expression statement.
    internal static ExpressionStatement BuildExpressionStatement(
        ParserRuleContext exprContext, AbstractParseTreeVisitor<AstNode> visitor) {
      if (visitor.Visit(exprContext) is Expression expr) {
        return Statement.Expression(expr, expr.Range);
      }
      return null;
    }

    // Builds an if statement for if ... elif statements.
    internal AstNode BuildIfElif(IToken ifToken, ParserRuleContext exprContext, IToken colonToken,
                                 ParserRuleContext blockContext, ParserRuleContext elifContext,
                                 AbstractParseTreeVisitor<AstNode> visitor) {
      TextRange ifRange = CodeReferenceUtils.RangeOfToken(ifToken);
      AddSyntaxToken(SyntaxType.Keyword, ifRange);
      if (visitor.Visit(exprContext) is Expression expr) {
        AddSyntaxToken(SyntaxType.Symbol, CodeReferenceUtils.RangeOfToken(colonToken));
        if (visitor.Visit(blockContext) is Statement block && visitor.Visit(elifContext) is Statement elif) {
          TextRange range = CodeReferenceUtils.CombineRanges(ifRange, elif.Range as TextRange);
          return Statement.If(expr, block, elif, range);
        }
      }
      return null;
    }

    // Builds an if statement for if ... else statements.
    internal IfStatement BuildIfElse(IToken ifToken, ParserRuleContext exprContext,
                                     IToken colonToken, ParserRuleContext blockContext,
                                     ParserRuleContext elseBlockContext,
                                     AbstractParseTreeVisitor<AstNode> visitor) {
      TextRange ifRange = CodeReferenceUtils.RangeOfToken(ifToken);
      AddSyntaxToken(SyntaxType.Keyword, ifRange);
      if (visitor.Visit(exprContext) is Expression expr) {
        AddSyntaxToken(SyntaxType.Symbol, CodeReferenceUtils.RangeOfToken(colonToken));
        if (visitor.Visit(blockContext) is Statement block) {
          AstNode elseBlock = elseBlockContext is null ? null : visitor.Visit(elseBlockContext);
          TextRange range = CodeReferenceUtils.CombineRanges(
              ifRange, elseBlock is null ? block.Range as TextRange : elseBlock.Range as TextRange);
          return Statement.If(expr, block, elseBlock as Statement, range);
        }
      }
      return null;
    }

    // Build an else block statement
    internal Statement BuildElse(IToken elseToken, IToken colonToken,
                                 ParserRuleContext blockContext,
                                 AbstractParseTreeVisitor<AstNode> visitor) {
      AddSyntaxToken(SyntaxType.Keyword, CodeReferenceUtils.RangeOfToken(elseToken));
      AddSyntaxToken(SyntaxType.Symbol, CodeReferenceUtils.RangeOfToken(colonToken));
      return visitor.Visit(blockContext) as Statement;
    }

    // Builds a while statement.
    internal WhileStatement BuildWhile(IToken whileToken, ParserRuleContext exprContext,
                                       IToken colonToken, ParserRuleContext blockContext,
                                       AbstractParseTreeVisitor<AstNode> visitor) {
      TextRange whileRange = CodeReferenceUtils.RangeOfToken(whileToken);
      AddSyntaxToken(SyntaxType.Keyword, whileRange);
      if (visitor.Visit(exprContext) is Expression expr) {
        AddSyntaxToken(SyntaxType.Symbol, CodeReferenceUtils.RangeOfToken(colonToken));
        if (visitor.Visit(blockContext) is Statement block) {
          Debug.Assert(block.Range is TextRange);
          TextRange range = CodeReferenceUtils.CombineRanges(whileRange, block.Range as TextRange);
          return Statement.While(expr, block, range);
        }
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
