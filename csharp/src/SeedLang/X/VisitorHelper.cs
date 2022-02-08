using System.Net.Http.Headers;
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
using System.Diagnostics;
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

    // Builds binary expressions.
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

    // Builds comparison expressions.
    internal ComparisonExpression BuildComparison(ParserRuleContext leftContext,
                                                  ParserRuleContext[] rightContexts,
                                                  ToComparisonOperator toComparisonOperator,
                                                  AbstractParseTreeVisitor<AstNode> visitor) {
      TextRange range = _groupingRange;
      _groupingRange = null;

      AstNode left = visitor.Visit(leftContext);
      if (left is Expression leftExpr) {
        var ops = new ComparisonOperator[rightContexts.Length];
        var exprs = new Expression[rightContexts.Length];
        for (int i = 0; i < rightContexts.Length; i++) {
          if (rightContexts[i].ChildCount == 2 &&
              rightContexts[i].GetChild(0) is ITerminalNode opNode) {
            IToken opToken = opNode.Symbol;
            AddSyntaxToken(SyntaxType.Operator, CodeReferenceUtils.RangeOfToken(opToken));
            ops[i] = toComparisonOperator(opToken);
            AstNode right = visitor.Visit(rightContexts[i].GetChild(1));
            if (right is Expression rightExpr) {
              exprs[i] = rightExpr;
            } else {
              return null;
            }
          } else {
            return null;
          }
        }
        if (range is null) {
          Expression last = exprs[exprs.Length - 1];
          Debug.Assert(left.Range is TextRange);
          Debug.Assert(last.Range is TextRange);
          range = CodeReferenceUtils.CombineRanges(left.Range as TextRange,
                                                   last.Range as TextRange);
        }
        return Expression.Comparison(leftExpr, ops, exprs, range);
      }
      return null;
    }

    // Builds boolean expressions.
    internal AstNode BuildAndOr(BooleanOperator op, ParserRuleContext[] operandContexts,
                                ITerminalNode[] orNodes,
                                AbstractParseTreeVisitor<AstNode> visitor) {
      Debug.Assert(orNodes.Length > 0 && operandContexts.Length == orNodes.Length + 1);
      var exprs = new Expression[operandContexts.Length];
      if (visitor.Visit(operandContexts[0]) is Expression first) {
        exprs[0] = first;
        for (int i = 0; i < orNodes.Length; i++) {
          TextRange orRange = CodeReferenceUtils.RangeOfToken(orNodes[i].Symbol);
          AddSyntaxToken(SyntaxType.Operator, orRange);
          if (visitor.Visit(operandContexts[i + 1]) is Expression expr) {
            exprs[i + 1] = expr;
          } else {
            return null;
          }
        }
        Expression last = exprs[exprs.Length - 1];
        Debug.Assert(first.Range is TextRange);
        Debug.Assert(last.Range is TextRange);
        TextRange range = CodeReferenceUtils.CombineRanges(first.Range as TextRange,
                                                           last.Range as TextRange);
        return Expression.Boolean(op, exprs, range);
      }
      return null;
    }

    // Builds unary expressions.
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

    // Builds grouping expressions, and sets grouping range for sub-expression to use. Only keeps
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

    // Builds identifier expressions.
    internal IdentifierExpression BuildIdentifier(IToken token) {
      TextRange range = HandleConstantOrVariableExpression(token, SyntaxType.Variable);
      return Expression.Identifier(token.Text, range);
    }

    // Builds boolean costant expressions.
    internal BooleanConstantExpression BuildBooleanConstant(IToken token, bool value) {
      TextRange range = HandleConstantOrVariableExpression(token, SyntaxType.Boolean);
      return Expression.BooleanConstant(value, range);
    }

    // Builds none costant expressions.
    internal NoneConstantExpression BuildNoneConstant(IToken token) {
      TextRange range = HandleConstantOrVariableExpression(token, SyntaxType.None);
      return Expression.NoneConstant(range);
    }

    // Builds number constant expressions.
    internal NumberConstantExpression BuildNumberConstant(IToken token) {
      TextRange range = HandleConstantOrVariableExpression(token, SyntaxType.Number);
      try {
        // The behavior of double.Parse is different between net6.0 and netstandard2.0 frameworks.
        // It returns an infinity double value without throwing any exception on net6.0 framework.
        // It throws an OverflowException on netstandard2.0. Handle both cases here.
        double value = double.Parse(token.Text);
        ValueHelper.CheckOverflow(value, range);
        return Expression.NumberConstant(value, range);
      } catch (System.OverflowException) {
        throw new DiagnosticException(SystemReporters.SeedX, Severity.Fatal, "", range,
                                      Message.RuntimeErrorOverflow);
      }
    }

    // Builds string constant expressions.
    internal StringConstantExpression BuildStringConstant(IToken token) {
      TextRange range = HandleConstantOrVariableExpression(token, SyntaxType.String);
      return Expression.StringConstant(token.Text, range);
    }

    // Builds list expressions.
    internal ListExpression BuildList(IToken openBrackToken, ParserRuleContext[] exprContexts,
                                      ITerminalNode[] commaNodes, IToken closeBrackToken,
                                      AbstractParseTreeVisitor<AstNode> visitor) {
      Debug.Assert(exprContexts.Length == commaNodes.Length ||
                   exprContexts.Length == commaNodes.Length + 1);
      TextRange openBrackRange = CodeReferenceUtils.RangeOfToken(openBrackToken);
      AddSyntaxToken(SyntaxType.Bracket, openBrackRange);
      if (BuildExpressions(exprContexts, commaNodes, visitor) is Expression[] exprs) {
        TextRange closeBrackRange = CodeReferenceUtils.RangeOfToken(closeBrackToken);
        AddSyntaxToken(SyntaxType.Bracket, closeBrackRange);
        TextRange range = CodeReferenceUtils.CombineRanges(openBrackRange, closeBrackRange);
        return Expression.List(exprs, range);
      }
      return null;
    }

    // Builds subscript expressions.
    internal SubscriptExpression BuildSubscript(ParserRuleContext primaryContext,
                                                IToken openBrackToken,
                                                ParserRuleContext exprContext,
                                                IToken closeBrackToken,
                                                AbstractParseTreeVisitor<AstNode> visitor) {
      if (visitor.Visit(primaryContext) is Expression primary) {
        AddSyntaxToken(SyntaxType.Bracket, CodeReferenceUtils.RangeOfToken(openBrackToken));
        if (visitor.Visit(exprContext) is Expression expr) {
          TextRange closeBrackRange = CodeReferenceUtils.RangeOfToken(closeBrackToken);
          AddSyntaxToken(SyntaxType.Bracket, closeBrackRange);
          Debug.Assert(primary.Range is TextRange);
          var primaryRange = primary.Range as TextRange;
          TextRange range = CodeReferenceUtils.CombineRanges(primaryRange, closeBrackRange);
          return Expression.Subscript(primary, expr, range);
        }
      }
      return null;
    }

    // Builds call expressions.
    internal CallExpression BuildCall(ParserRuleContext primaryContext, IToken openParenToken,
                                      ParserRuleContext[] exprContexts, ITerminalNode[] commaNodes,
                                      IToken closeParenToken,
                                      AbstractParseTreeVisitor<AstNode> visitor) {
      if (visitor.Visit(primaryContext) is Expression func) {
        AddSyntaxToken(SyntaxType.Parenthesis, CodeReferenceUtils.RangeOfToken(openParenToken));
        Debug.Assert(exprContexts.Length == 0 && commaNodes.Length == 0 ||
                     exprContexts.Length == commaNodes.Length + 1);
        var exprs = new Expression[exprContexts.Length];
        for (int i = 0; i < exprContexts.Length; i++) {
          if (visitor.Visit(exprContexts[i]) is Expression expr) {
            exprs[i] = expr;
          }
          if (i < commaNodes.Length) {
            AddSyntaxToken(SyntaxType.Symbol,
                           CodeReferenceUtils.RangeOfToken(commaNodes[i].Symbol));
          }
        }
        TextRange closeParenRange = CodeReferenceUtils.RangeOfToken(closeParenToken);
        AddSyntaxToken(SyntaxType.Parenthesis, closeParenRange);
        Debug.Assert(func.Range is TextRange);
        TextRange range = CodeReferenceUtils.CombineRanges(func.Range as TextRange,
                                                           closeParenRange);
        return Expression.Call(func, exprs, range);
      }
      return null;
    }

    // Builds assignment statements.
    internal AssignmentStatement BuildAssignment(ParserRuleContext[] targetContexts,
                                                 ITerminalNode[] targetCommaNodes,
                                                 IToken equalToken,
                                                 ParserRuleContext[] exprContexts,
                                                 ITerminalNode[] exprCommaNodes,
                                                 AbstractParseTreeVisitor<AstNode> visitor) {
      Debug.Assert(targetContexts.Length > 0 && exprContexts.Length > 0);
      var targets = BuildExpressions(targetContexts, targetCommaNodes, visitor);
      AddSyntaxToken(SyntaxType.Operator, CodeReferenceUtils.RangeOfToken(equalToken));
      var exprs = BuildExpressions(exprContexts, exprCommaNodes, visitor);
      if (!(targets is null) && !(exprs is null)) {
        Debug.Assert(targets.Length > 0 && exprs.Length > 0);
        TextRange range = CodeReferenceUtils.CombineRanges(
            targets[0].Range as TextRange, exprs[exprs.Length - 1].Range as TextRange);
        return Statement.Assignment(targets, exprs, range);
      }
      return null;
    }

    // Builds block statements.
    internal static BlockStatement BuildBlock(ParserRuleContext[] statementContexts,
                                              AbstractParseTreeVisitor<AstNode> visitor) {
      var statements = new Statement[statementContexts.Length];
      for (int i = 0; i < statementContexts.Length; i++) {
        statements[i] = visitor.Visit(statementContexts[i]) as Statement;
      }
      return BuildBlock(statements);
    }

    // Builds expression statements.
    internal static ExpressionStatement BuildExpressionStatement(
        ParserRuleContext exprContext, AbstractParseTreeVisitor<AstNode> visitor) {
      if (visitor.Visit(exprContext) is Expression expr) {
        return Statement.Expression(expr, expr.Range);
      }
      return null;
    }

    // Builds function define statements.
    internal FuncDefStatement BuildFuncDef(IToken defToken, IToken nameToken,
                                           IToken openParenToken, ITerminalNode[] parameterNodes,
                                           ITerminalNode[] commaNodes, IToken closeParenToken,
                                           IToken colonToken, ParserRuleContext blockContext,
                                           AbstractParseTreeVisitor<AstNode> visitor) {
      TextRange defRange = CodeReferenceUtils.RangeOfToken(defToken);
      AddSyntaxToken(SyntaxType.Keyword, defRange);
      AddSyntaxToken(SyntaxType.Function, CodeReferenceUtils.RangeOfToken(nameToken));
      AddSyntaxToken(SyntaxType.Parenthesis, CodeReferenceUtils.RangeOfToken(openParenToken));
      Debug.Assert(parameterNodes.Length == 0 && commaNodes.Length == 0 ||
                   parameterNodes.Length == commaNodes.Length + 1);
      var arguments = new string[parameterNodes.Length];
      for (int i = 0; i < parameterNodes.Length; i++) {
        TextRange parameterRange = CodeReferenceUtils.RangeOfToken(parameterNodes[i].Symbol);
        AddSyntaxToken(SyntaxType.Parameter, parameterRange);
        arguments[i] = parameterNodes[i].Symbol.Text;
        if (i < commaNodes.Length) {
          AddSyntaxToken(SyntaxType.Symbol, CodeReferenceUtils.RangeOfToken(commaNodes[i].Symbol));
        }
      }
      AddSyntaxToken(SyntaxType.Parenthesis, CodeReferenceUtils.RangeOfToken(closeParenToken));
      AddSyntaxToken(SyntaxType.Symbol, CodeReferenceUtils.RangeOfToken(colonToken));
      if (visitor.Visit(blockContext) is Statement block) {
        Debug.Assert(block.Range is TextRange);
        TextRange range = CodeReferenceUtils.CombineRanges(defRange, block.Range as TextRange);
        return Statement.FuncDef(nameToken.Text, arguments, block, range);
      }
      return null;
    }

    // Builds "if" statements of "if ... elif" statements.
    internal AstNode BuildIfElif(IToken ifToken, ParserRuleContext exprContext, IToken colonToken,
                                 ParserRuleContext blockContext, ParserRuleContext elifContext,
                                 AbstractParseTreeVisitor<AstNode> visitor) {
      TextRange ifRange = CodeReferenceUtils.RangeOfToken(ifToken);
      AddSyntaxToken(SyntaxType.Keyword, ifRange);
      if (visitor.Visit(exprContext) is Expression expr) {
        AddSyntaxToken(SyntaxType.Symbol, CodeReferenceUtils.RangeOfToken(colonToken));
        if (visitor.Visit(blockContext) is Statement block &&
            visitor.Visit(elifContext) is Statement elif) {
          TextRange range = CodeReferenceUtils.CombineRanges(ifRange, elif.Range as TextRange);
          return Statement.If(expr, block, elif, range);
        }
      }
      return null;
    }

    // Builds "if" statements of "if ... else" statements.
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

    // Builds "else" statements.
    internal Statement BuildElse(IToken elseToken, IToken colonToken,
                                 ParserRuleContext blockContext,
                                 AbstractParseTreeVisitor<AstNode> visitor) {
      AddSyntaxToken(SyntaxType.Keyword, CodeReferenceUtils.RangeOfToken(elseToken));
      AddSyntaxToken(SyntaxType.Symbol, CodeReferenceUtils.RangeOfToken(colonToken));
      return visitor.Visit(blockContext) as Statement;
    }

    // Builds return statements.
    internal ReturnStatement BuildReturn(IToken returnToken, ParserRuleContext exprContext,
                                         AbstractParseTreeVisitor<AstNode> visitor) {
      TextRange returnRange = CodeReferenceUtils.RangeOfToken(returnToken);
      AddSyntaxToken(SyntaxType.Keyword, returnRange);
      if (exprContext is null) {
        return Statement.Return(null, returnRange);
      }
      if (visitor.Visit(exprContext) is Expression expr) {
        Debug.Assert(expr.Range is TextRange);
        TextRange range = CodeReferenceUtils.CombineRanges(returnRange, expr.Range as TextRange);
        return Statement.Return(expr, range);
      }
      return null;
    }

    // Builds a block of simple statements.
    internal BlockStatement BuildSimpleStatements(ParserRuleContext[] statementContexts,
                                                  ITerminalNode[] semicolonNodes,
                                                  AbstractParseTreeVisitor<AstNode> visitor) {
      Debug.Assert(statementContexts.Length == semicolonNodes.Length + 1 ||
                   statementContexts.Length == semicolonNodes.Length);
      var statements = new Statement[statementContexts.Length];
      for (int i = 0; i < statementContexts.Length; i++) {
        statements[i] = visitor.Visit(statementContexts[i]) as Statement;
        if (i < semicolonNodes.Length) {
          TextRange semicolonRange = CodeReferenceUtils.RangeOfToken(semicolonNodes[i].Symbol);
          AddSyntaxToken(SyntaxType.Symbol, semicolonRange);
        }
      }
      return BuildBlock(statements);
    }

    // Builds "for in" statements.
    // TODO: The "for in" statement is converted to a block of initializer and while statements.
    // It's possible to compile the "for in range" statement into a "FORPREP" opcode for
    // optimization.
    internal BlockStatement BuildFor(IToken forToken, ParserRuleContext identifierContext,
                                     IToken inToken, ParserRuleContext exprContext,
                                     IToken colonToken, ParserRuleContext blockContext,
                                     AbstractParseTreeVisitor<AstNode> visitor) {
      TextRange forRange = CodeReferenceUtils.RangeOfToken(forToken);
      AddSyntaxToken(SyntaxType.Keyword, forRange);
      if (visitor.Visit(identifierContext) is IdentifierExpression loopVar) {
        AddSyntaxToken(SyntaxType.Keyword, CodeReferenceUtils.RangeOfToken(inToken));
        if (visitor.Visit(exprContext) is Expression expr) {
          var iterVariable = Expression.Identifier($"__for_{loopVar.Name}_iter__", expr.Range);
          var iterFunc = Expression.Identifier(NativeFunctions.Iter, expr.Range);
          var iterator = Expression.Call(iterFunc, new Expression[] { expr }, expr.Range);
          var initializer = Statement.Assignment(new Expression[] { iterVariable },
                                                 new Expression[] { iterator }, expr.Range);
          AddSyntaxToken(SyntaxType.Symbol, CodeReferenceUtils.RangeOfToken(colonToken));
          if (visitor.Visit(blockContext) is Statement block) {
            var hasNextFunc = Expression.Identifier(NativeFunctions.HasNext, block.Range);
            var test = Expression.Call(hasNextFunc, new Expression[] { iterVariable }, block.Range);
            var nextFunc = Expression.Identifier(NativeFunctions.Next, block.Range);
            var next = Expression.Call(nextFunc, new Expression[] { iterVariable }, block.Range);
            var assign = Statement.Assignment(new Expression[] { loopVar },
                                              new Expression[] { next }, block.Range);
            var body = Statement.Block(new Statement[] { assign, block, }, block.Range);
            var @while = Statement.While(test, body, block.Range);
            TextRange range = CodeReferenceUtils.CombineRanges(forRange, block.Range as TextRange);
            return Statement.Block(new Statement[] { initializer, @while }, range);
          }
        }
      }
      return null;
    }

    // Builds while statements.
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

    private static BlockStatement BuildBlock(Statement[] statements) {
      Debug.Assert(statements.Length > 0);
      Statement first = statements[0];
      Statement last = statements[statements.Length - 1];
      Debug.Assert(first.Range is TextRange && last.Range is TextRange);
      Range range = CodeReferenceUtils.CombineRanges(first.Range as TextRange,
                                                     last.Range as TextRange);
      return new BlockStatement(statements, range);
    }

    private Expression[] BuildExpressions(ParserRuleContext[] exprContexts,
                                          ITerminalNode[] commaNodes,
                                          AbstractParseTreeVisitor<AstNode> visitor) {
      var exprs = new Expression[exprContexts.Length];
      for (int i = 0; i < exprContexts.Length; i++) {
        if (visitor.Visit(exprContexts[i]) is Expression expr) {
          exprs[i] = expr;
          if (i < commaNodes.Length) {
            TextRange commaRange = CodeReferenceUtils.RangeOfToken(commaNodes[i].Symbol);
            AddSyntaxToken(SyntaxType.Symbol, commaRange);
          }
        } else {
          Debug.Fail($"Expression at position {i} shall be validated by ANTLR.");
        }
      }
      return exprs;
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
