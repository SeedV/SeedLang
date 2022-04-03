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
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using SeedLang.Ast;
using SeedLang.Common;
using SeedLang.Runtime;

namespace SeedLang.X {
  // A helper class to build AST nodes from parser tree contexts.
  internal class VisitorHelper {
    private readonly IList<TokenInfo> _semanticTokens;
    private TextRange _groupingRange;

    public VisitorHelper(IList<TokenInfo> tokens) {
      _semanticTokens = tokens;
    }

    // Builds binary expressions.
    internal BinaryExpression BuildBinary(ParserRuleContext leftContext, IToken opToken,
                                          BinaryOperator op, ParserRuleContext rightContext,
                                          AbstractParseTreeVisitor<AstNode> visitor) {
      TextRange range = _groupingRange;
      _groupingRange = null;
      if (visitor.Visit(leftContext) is Expression left) {
        AddSemanticToken(TokenType.Operator, CodeReferenceUtils.RangeOfToken(opToken));
        if (visitor.Visit(rightContext) is Expression right) {
          if (range is null) {
            Debug.Assert(left.Range is TextRange);
            Debug.Assert(right.Range is TextRange);
            range = CodeReferenceUtils.CombineRanges(left.Range as TextRange,
                                                     right.Range as TextRange);
          }
          return Expression.Binary(left, op, right, range);
        }
      }
      return null;
    }

    // Builds comparison expressions.
    internal ComparisonExpression BuildComparison(ParserRuleContext[] operands, IToken[] opTokens,
                                                  ComparisonOperator[] ops,
                                                  AbstractParseTreeVisitor<AstNode> visitor) {
      TextRange range = _groupingRange;
      _groupingRange = null;
      Debug.Assert(operands.Length > 1 && operands.Length == opTokens.Length + 1 &&
                   opTokens.Length == ops.Length);
      var first = visitor.Visit(operands[0]) as Expression;
      var exprs = new Expression[ops.Length];
      for (int i = 0; i < ops.Length; i++) {
        AddSemanticToken(TokenType.Operator, CodeReferenceUtils.RangeOfToken(opTokens[i]));
        exprs[i] = visitor.Visit(operands[i + 1]) as Expression;
      }
      if (range is null) {
        Expression last = exprs[exprs.Length - 1];
        Debug.Assert(first.Range is TextRange);
        Debug.Assert(last.Range is TextRange);
        range = CodeReferenceUtils.CombineRanges(first.Range as TextRange, last.Range as TextRange);
      }
      return Expression.Comparison(first, ops, exprs, range);
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
          AddSemanticToken(TokenType.Operator, orRange);
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
      AddSemanticToken(TokenType.Operator, opRange);

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
      AddSemanticToken(TokenType.OpenParenthesis, openRange);

      if (_groupingRange is null) {
        _groupingRange = CodeReferenceUtils.CombineRanges(openRange, closeRange);
      }

      AstNode node = visitor.Visit(exprContext);
      AddSemanticToken(TokenType.CloseParenthesis, closeRange);
      return node;
    }

    // Builds identifier expressions.
    internal IdentifierExpression BuildIdentifier(IToken token) {
      TextRange range = HandleConstantOrVariableExpression(token, TokenType.Variable);
      return Expression.Identifier(token.Text, range);
    }

    // Builds boolean costant expressions.
    internal BooleanConstantExpression BuildBooleanConstant(IToken token, bool value) {
      TextRange range = HandleConstantOrVariableExpression(token, TokenType.Boolean);
      return Expression.BooleanConstant(value, range);
    }

    // Builds nil costant expressions.
    internal NilConstantExpression BuildNilConstant(IToken token) {
      TextRange range = HandleConstantOrVariableExpression(token, TokenType.Nil);
      return Expression.NilConstant(range);
    }

    // Builds number constant expressions.
    internal NumberConstantExpression BuildNumberConstant(IToken token) {
      TextRange range = HandleConstantOrVariableExpression(token, TokenType.Number);
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
    internal StringConstantExpression BuildStringConstant(ITerminalNode[] strNodes) {
      Debug.Assert(strNodes.Length >= 1);
      var sb = new StringBuilder();
      foreach (ITerminalNode strNode in strNodes) {
        AddSemanticToken(TokenType.String, CodeReferenceUtils.RangeOfToken(strNode.Symbol));
        string str = strNode.Symbol.Text;
        Debug.Assert(str.Length >= 2 && (str[0] == '"' || str[0] == '\'') &&
                     (str[str.Length - 1] == '"' || str[str.Length - 1] == '\''));
        sb.Append(str, 1, str.Length - 2);
      }
      TextRange range = CodeReferenceUtils.RangeOfTokens(strNodes[0].Symbol,
                                                         strNodes[strNodes.Length - 1].Symbol);
      range = _groupingRange is null ? range : _groupingRange;
      _groupingRange = null;
      return Expression.StringConstant(sb.ToString(), range);
    }

    // Builds dict expressions.
    internal DictExpression BuildDict(IToken openBraceToken,
                                  IReadOnlyList<ParserRuleContext> keyContexts,
                                  IReadOnlyList<ParserRuleContext> valueContexts,
                                  IReadOnlyList<IToken> colonNodes, ITerminalNode[] commaNodes,
                                  IToken closeBraceToken,
                                  AbstractParseTreeVisitor<AstNode> visitor) {
      Debug.Assert(keyContexts.Count == valueContexts.Count &&
                   keyContexts.Count == colonNodes.Count);
      Debug.Assert(keyContexts.Count == commaNodes.Length ||
                   keyContexts.Count == commaNodes.Length + 1);
      TextRange openBraceRange = CodeReferenceUtils.RangeOfToken(openBraceToken);
      AddSemanticToken(TokenType.OpenBrace, openBraceRange);
      var keyValues = new KeyValuePair<Expression, Expression>[keyContexts.Count];
      for (int i = 0; i < keyContexts.Count; i++) {
        var key = visitor.Visit(keyContexts[i]) as Expression;
        AddSemanticToken(TokenType.Symbol, CodeReferenceUtils.RangeOfToken(colonNodes[i]));
        var value = visitor.Visit(valueContexts[i]) as Expression;
        keyValues[i] = new KeyValuePair<Expression, Expression>(key, value);
      }
      TextRange closeBraceRange = CodeReferenceUtils.RangeOfToken(closeBraceToken);
      AddSemanticToken(TokenType.CloseBrace, closeBraceRange);
      TextRange range = CodeReferenceUtils.CombineRanges(openBraceRange, closeBraceRange);
      return Expression.Dict(keyValues, range);
    }

    // Builds list expressions.
    internal ListExpression BuildList(IToken openBrackToken, ParserRuleContext[] exprContexts,
                                      ITerminalNode[] commaNodes, IToken closeBrackToken,
                                      AbstractParseTreeVisitor<AstNode> visitor) {
      Debug.Assert(exprContexts.Length == commaNodes.Length ||
                   exprContexts.Length == commaNodes.Length + 1);
      TextRange openBrackRange = CodeReferenceUtils.RangeOfToken(openBrackToken);
      AddSemanticToken(TokenType.OpenBracket, openBrackRange);
      if (BuildExpressions(exprContexts, commaNodes, visitor) is Expression[] exprs) {
        TextRange closeBrackRange = CodeReferenceUtils.RangeOfToken(closeBrackToken);
        AddSemanticToken(TokenType.CloseBracket, closeBrackRange);
        TextRange range = CodeReferenceUtils.CombineRanges(openBrackRange, closeBrackRange);
        return Expression.List(exprs, range);
      }
      return null;
    }

    internal TupleExpression BuildTuple(IToken openParenToken, ParserRuleContext[] exprContexts,
                                        ITerminalNode[] commaNodes, IToken closeParenToken,
                                        AbstractParseTreeVisitor<AstNode> visitor) {
      Debug.Assert(exprContexts.Length == commaNodes.Length ||
                   exprContexts.Length == commaNodes.Length + 1);
      TextRange openParenRange = CodeReferenceUtils.RangeOfToken(openParenToken);
      AddSemanticToken(TokenType.OpenParenthesis, openParenRange);
      if (BuildExpressions(exprContexts, commaNodes, visitor) is Expression[] exprs) {
        TextRange closeParenRange = CodeReferenceUtils.RangeOfToken(closeParenToken);
        AddSemanticToken(TokenType.CloseParenthesis, closeParenRange);
        TextRange range = CodeReferenceUtils.CombineRanges(openParenRange, closeParenRange);
        return Expression.Tuple(exprs, range);
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
        AddSemanticToken(TokenType.OpenBracket, CodeReferenceUtils.RangeOfToken(openBrackToken));
        if (visitor.Visit(exprContext) is Expression expr) {
          TextRange closeBrackRange = CodeReferenceUtils.RangeOfToken(closeBrackToken);
          AddSemanticToken(TokenType.CloseBracket, closeBrackRange);
          Debug.Assert(primary.Range is TextRange);
          var primaryRange = primary.Range as TextRange;
          TextRange range = CodeReferenceUtils.CombineRanges(primaryRange, closeBrackRange);
          return Expression.Subscript(primary, expr, range);
        }
      }
      return null;
    }

    // Builds attribute expressions.
    internal AttributeExpression BuildAttribute(ParserRuleContext primaryContext, IToken dotToken,
                                                ParserRuleContext identifierContext,
                                                AbstractParseTreeVisitor<AstNode> visitor) {
      if (visitor.Visit(primaryContext) is Expression value) {
        AddSemanticToken(TokenType.Symbol, CodeReferenceUtils.RangeOfToken(dotToken));
        if (visitor.Visit(identifierContext) is IdentifierExpression identifier) {
          Debug.Assert(value.Range is TextRange && identifier.Range is TextRange);
          var primaryRange = value.Range as TextRange;
          var identifierRange = identifier.Range as TextRange;
          TextRange range = CodeReferenceUtils.CombineRanges(primaryRange, identifierRange);
          return Expression.Attribute(value, identifier, range);
        }
      }
      return null;
    }

    // Builds call expressions.
    internal CallExpression BuildCall(ParserRuleContext primaryContext, IToken openParenToken,
                                      ParserRuleContext[] exprContexts, ITerminalNode[] commaNodes,
                                      IToken closeParenToken,
                                      AbstractParseTreeVisitor<AstNode> visitor) {
      AstNode callee = visitor.Visit(primaryContext);
      Expression func = null;
      Expression firstParam = null;
      if (callee is AttributeExpression attr) {
        func = attr.Attr;
        firstParam = attr.Value;
      } else if (callee is Expression expr) {
        func = expr;
      }
      if (!(func is null)) {
        AddSemanticToken(TokenType.OpenParenthesis,
                         CodeReferenceUtils.RangeOfToken(openParenToken));
        Debug.Assert(exprContexts.Length == 0 && commaNodes.Length == 0 ||
                     exprContexts.Length == commaNodes.Length + 1);
        int additionalParam = firstParam is null ? 0 : 1;
        var exprs = new Expression[exprContexts.Length + additionalParam];
        if (!(firstParam is null)) {
          exprs[0] = firstParam;
        }
        for (int i = 0; i < exprContexts.Length; i++) {
          if (visitor.Visit(exprContexts[i]) is Expression expr) {
            exprs[i + additionalParam] = expr;
          }
          if (i < commaNodes.Length) {
            AddSemanticToken(TokenType.Symbol,
                             CodeReferenceUtils.RangeOfToken(commaNodes[i].Symbol));
          }
        }
        TextRange closeParenRange = CodeReferenceUtils.RangeOfToken(closeParenToken);
        AddSemanticToken(TokenType.CloseParenthesis, closeParenRange);
        Debug.Assert(callee.Range is TextRange);
        TextRange range = CodeReferenceUtils.CombineRanges(callee.Range as TextRange,
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
      AddSemanticToken(TokenType.Operator, CodeReferenceUtils.RangeOfToken(equalToken));
      var exprs = BuildExpressions(exprContexts, exprCommaNodes, visitor);
      if (!(targets is null) && !(exprs is null)) {
        Debug.Assert(targets.Length > 0 && exprs.Length > 0);
        TextRange range = CodeReferenceUtils.CombineRanges(
            targets[0].Range as TextRange, exprs[exprs.Length - 1].Range as TextRange);
        return Statement.Assignment(targets, exprs, range);
      }
      return null;
    }

    // Builds augmented assignment statements.
    internal AstNode BuildAugAssignment(ParserRuleContext targetContext, IToken opToken,
                                        BinaryOperator op, ParserRuleContext exprContext,
                                        AbstractParseTreeVisitor<AstNode> visitor) {
      if (visitor.Visit(targetContext) is Expression target) {
        AddSemanticToken(TokenType.Operator, CodeReferenceUtils.RangeOfToken(opToken));
        if (visitor.Visit(exprContext) is Expression expr) {
          Debug.Assert(target.Range is TextRange);
          Debug.Assert(expr.Range is TextRange);
          Range range = CodeReferenceUtils.CombineRanges(target.Range as TextRange,
                                                         expr.Range as TextRange);
          Expression binary = Expression.Binary(target, op, expr, range);
          return Statement.Assignment(new Expression[] { target }, new Expression[] { binary },
                                      range);
        }
      }
      return null;
    }

    // Builds block statements.
    internal static AstNode BuildBlock(ITerminalNode[] newLineNodes,
                                       ParserRuleContext[] statementContexts,
                                       AbstractParseTreeVisitor<AstNode> visitor) {
      if (statementContexts.Length == 0) {
        TextRange range = null;
        if (newLineNodes.Length > 0) {
          range = CodeReferenceUtils.RangeOfTokens(newLineNodes[0].Symbol,
                                                   newLineNodes[newLineNodes.Length - 1].Symbol);
        }
        return Statement.Block(System.Array.Empty<Statement>(), range);
      } else if (statementContexts.Length == 1) {
        return visitor.Visit(statementContexts[0]);
      }
      var statements = new List<Statement>();
      VTagStatement vTag = null;
      foreach (ParserRuleContext context in statementContexts) {
        var statement = visitor.Visit(context) as Statement;
        if (statement is VTagStatement vTagStatement) {
          vTag = vTagStatement;
        } else if (!(vTag is null)) {
          Range range = CodeReferenceUtils.CombineRanges(vTag.Range as TextRange,
                                                         statement.Range as TextRange);
          statements.Add(new VTagStatement(vTag.VTags, new Statement[] { statement }, range));
        } else {
          statements.Add(statement);
        }
      }
      return BuildBlock(statements.ToArray());
    }

    // Builds expression statements.
    internal ExpressionStatement BuildExpressionStmt(ParserRuleContext[] exprContexts,
                                                     ITerminalNode[] commaNodes,
                                                     AbstractParseTreeVisitor<AstNode> visitor) {
      if (exprContexts.Length == 1 && visitor.Visit(exprContexts[0]) is Expression expr) {
        return Statement.Expression(expr, expr.Range);
      }
      Expression[] exprs = BuildExpressions(exprContexts, commaNodes, visitor);
      Debug.Assert(exprs.Length > 1);
      Debug.Assert(exprs[0].Range is TextRange);
      Debug.Assert(exprs[exprs.Length - 1].Range is TextRange);
      var firstRange = exprs[0].Range as TextRange;
      var lastRange = exprs[exprs.Length - 1].Range as TextRange;
      TextRange range = CodeReferenceUtils.CombineRanges(firstRange, lastRange);
      return Statement.Expression(Expression.Tuple(exprs, range), range);
    }

    // Builds function definition statements.
    internal FuncDefStatement BuildFuncDef(IToken defToken, IToken nameToken,
                                           IToken openParenToken, ITerminalNode[] parameterNodes,
                                           ITerminalNode[] commaNodes, IToken closeParenToken,
                                           IToken colonToken, ParserRuleContext blockContext,
                                           AbstractParseTreeVisitor<AstNode> visitor) {
      TextRange defRange = CodeReferenceUtils.RangeOfToken(defToken);
      AddSemanticToken(TokenType.Keyword, defRange);
      AddSemanticToken(TokenType.Function, CodeReferenceUtils.RangeOfToken(nameToken));
      AddSemanticToken(TokenType.OpenParenthesis, CodeReferenceUtils.RangeOfToken(openParenToken));
      Debug.Assert(parameterNodes.Length == 0 && commaNodes.Length == 0 ||
                   parameterNodes.Length == commaNodes.Length + 1);
      var arguments = new string[parameterNodes.Length];
      for (int i = 0; i < parameterNodes.Length; i++) {
        TextRange parameterRange = CodeReferenceUtils.RangeOfToken(parameterNodes[i].Symbol);
        AddSemanticToken(TokenType.Parameter, parameterRange);
        arguments[i] = parameterNodes[i].Symbol.Text;
        if (i < commaNodes.Length) {
          AddSemanticToken(TokenType.Symbol, CodeReferenceUtils.RangeOfToken(commaNodes[i].Symbol));
        }
      }
      AddSemanticToken(TokenType.CloseParenthesis, CodeReferenceUtils.RangeOfToken(closeParenToken));
      AddSemanticToken(TokenType.Symbol, CodeReferenceUtils.RangeOfToken(colonToken));
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
      AddSemanticToken(TokenType.Keyword, ifRange);
      if (visitor.Visit(exprContext) is Expression expr) {
        AddSemanticToken(TokenType.Symbol, CodeReferenceUtils.RangeOfToken(colonToken));
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
      AddSemanticToken(TokenType.Keyword, ifRange);
      if (visitor.Visit(exprContext) is Expression expr) {
        AddSemanticToken(TokenType.Symbol, CodeReferenceUtils.RangeOfToken(colonToken));
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
      AddSemanticToken(TokenType.Keyword, CodeReferenceUtils.RangeOfToken(elseToken));
      AddSemanticToken(TokenType.Symbol, CodeReferenceUtils.RangeOfToken(colonToken));
      return visitor.Visit(blockContext) as Statement;
    }

    // Builds return statements.
    internal ReturnStatement BuildReturn(IToken returnToken, ParserRuleContext[] exprContexts,
                                         ITerminalNode[] commaNodes,
                                         AbstractParseTreeVisitor<AstNode> visitor) {
      TextRange returnRange = CodeReferenceUtils.RangeOfToken(returnToken);
      AddSemanticToken(TokenType.Keyword, returnRange);
      if (exprContexts is null) {
        return Statement.Return(System.Array.Empty<Expression>(), returnRange);
      }
      Expression[] exprs = BuildExpressions(exprContexts, commaNodes, visitor);
      TextRange range = returnRange;
      if (exprs.Length > 0) {
        range = CodeReferenceUtils.CombineRanges(returnRange,
                                                 exprs[exprs.Length - 1].Range as TextRange);
      }
      return Statement.Return(exprs, range);
    }

    // Builds a block of simple statements.
    internal AstNode BuildSimpleStatements(ParserRuleContext[] statementContexts,
                                           ITerminalNode[] semicolonNodes,
                                           AbstractParseTreeVisitor<AstNode> visitor) {
      Debug.Assert(statementContexts.Length == semicolonNodes.Length + 1 ||
                   statementContexts.Length == semicolonNodes.Length);
      var statements = new Statement[statementContexts.Length];
      for (int i = 0; i < statementContexts.Length; i++) {
        statements[i] = visitor.Visit(statementContexts[i]) as Statement;
        if (i < semicolonNodes.Length) {
          TextRange semicolonRange = CodeReferenceUtils.RangeOfToken(semicolonNodes[i].Symbol);
          AddSemanticToken(TokenType.Symbol, semicolonRange);
        }
      }
      return BuildBlock(statements);
    }

    // Builds "for in" statements.
    internal ForInStatement BuildForIn(IToken forToken, ParserRuleContext identifierContext,
                                       IToken inToken, ParserRuleContext exprContext,
                                       IToken colonToken, ParserRuleContext blockContext,
                                       AbstractParseTreeVisitor<AstNode> visitor) {
      TextRange forRange = CodeReferenceUtils.RangeOfToken(forToken);
      AddSemanticToken(TokenType.Keyword, forRange);
      if (visitor.Visit(identifierContext) is IdentifierExpression loopVar) {
        AddSemanticToken(TokenType.Keyword, CodeReferenceUtils.RangeOfToken(inToken));
        if (visitor.Visit(exprContext) is Expression expr) {
          AddSemanticToken(TokenType.Symbol, CodeReferenceUtils.RangeOfToken(colonToken));
          if (visitor.Visit(blockContext) is Statement block) {
            TextRange range = CodeReferenceUtils.CombineRanges(forRange, block.Range as TextRange);
            return Statement.ForIn(loopVar, expr, block, range);
          }
        }
      }
      return null;
    }

    // Builds pass statements.
    internal AstNode BuildPass(IToken passToken) {
      TextRange range = CodeReferenceUtils.RangeOfToken(passToken);
      AddSemanticToken(TokenType.Keyword, range);
      return Statement.Pass(range);
    }

    // Builds while statements.
    internal WhileStatement BuildWhile(IToken whileToken, ParserRuleContext exprContext,
                                       IToken colonToken, ParserRuleContext blockContext,
                                       AbstractParseTreeVisitor<AstNode> visitor) {
      TextRange whileRange = CodeReferenceUtils.RangeOfToken(whileToken);
      AddSemanticToken(TokenType.Keyword, whileRange);
      if (visitor.Visit(exprContext) is Expression expr) {
        AddSemanticToken(TokenType.Symbol, CodeReferenceUtils.RangeOfToken(colonToken));
        if (visitor.Visit(blockContext) is Statement block) {
          Debug.Assert(block.Range is TextRange);
          TextRange range = CodeReferenceUtils.CombineRanges(whileRange, block.Range as TextRange);
          return Statement.While(expr, block, range);
        }
      }
      return null;
    }

    // Builds VTag statements.
    internal AstNode BuildVTag(IToken startToken, IToken name, IToken endToken) {
      TextRange startRange = CodeReferenceUtils.RangeOfToken(startToken);
      TextRange endRange = CodeReferenceUtils.RangeOfToken(endToken);
      TextRange range = CodeReferenceUtils.CombineRanges(startRange, endRange);
      var vTags = new VTagStatement.VTagInfo[1] {
        new VTagStatement.VTagInfo(name.Text, System.Array.Empty<string>())
      };
      return Statement.VTag(vTags, null, range);
    }

    private static AstNode BuildBlock(Statement[] statements) {
      Debug.Assert(statements.Length > 0);
      if (statements.Length == 1) {
        return statements[0];
      }
      Statement first = statements[0];
      Statement last = statements[statements.Length - 1];
      Range range = CodeReferenceUtils.CombineRanges(first.Range as TextRange,
                                                     last.Range as TextRange);
      return new BlockStatement(statements, range);
    }

    private Expression[] BuildExpressions(ParserRuleContext[] exprContexts,
                                          ITerminalNode[] commaNodes,
                                          AbstractParseTreeVisitor<AstNode> visitor) {
      var exprs = new Expression[exprContexts.Length];
      for (int i = 0; i < exprContexts.Length; i++) {
        exprs[i] = visitor.Visit(exprContexts[i]) as Expression;
        if (i < commaNodes.Length) {
          TextRange commaRange = CodeReferenceUtils.RangeOfToken(commaNodes[i].Symbol);
          AddSemanticToken(TokenType.Symbol, commaRange);
        }
      }
      return exprs;
    }

    private TextRange HandleConstantOrVariableExpression(IToken token, TokenType type) {
      TextRange tokenRange = CodeReferenceUtils.RangeOfToken(token);
      AddSemanticToken(type, tokenRange);
      TextRange range = _groupingRange is null ? tokenRange : _groupingRange;
      _groupingRange = null;
      return range;
    }

    private void AddSemanticToken(TokenType type, TextRange range) {
      _semanticTokens.Add(new TokenInfo(type, range));
    }
  }
}
