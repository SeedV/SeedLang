using System.Linq;
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
using SeedLang.Ast;
using SeedLang.Common;
using SeedLang.Runtime;
using Antlr4.Runtime.Tree;

namespace SeedLang.X {
  // The visitor class to visit a SeedPython parse tree and generate the corresponding AST tree.
  //
  // The default implement of SeedPythonBaseVisitor is to visit all the children and return the
  // result of the last one. PythonVisitor overrides the method if the default implement is not
  // correct.
  internal class PythonVisitor : SeedPythonBaseVisitor<AstNode> {
    private readonly VisitorHelper _helper;

    public PythonVisitor(IList<SyntaxToken> tokens) {
      _helper = new VisitorHelper(tokens);
    }

    // Visits an unary expression.
    public override AstNode VisitUnaryExpression(
        [NotNull] SeedPythonParser.UnaryExpressionContext context) {
      if (context.expression() is SeedPythonParser.ExpressionContext expr) {
        IToken op = (context.unaryOperator().GetChild(0) as ITerminalNode).Symbol;
        return _helper.BuildUnary(op, expr, this);
      }
      return null;
    }

    // Visits a multiply and divide binary expression.
    //
    // There should be 2 child expression contexts (left and right) in Mul_divContext.
    public override AstNode VisitMulDivExpression(
        [NotNull] SeedPythonParser.MulDivExpressionContext context) {
      if (context.expression() is SeedPythonParser.ExpressionContext[] exprs && exprs.Length == 2) {
        IToken op = (context.mulDivOperator().GetChild(0) as ITerminalNode).Symbol;
        return _helper.BuildBinary(op, TokenToOperator(op), exprs, this);
      }
      return null;
    }

    // Visits an add or subtract binary expression.
    //
    // There should be 2 child expression contexts (left and right) in Add_subContext.
    public override AstNode VisitAddSubExpression(
        [NotNull] SeedPythonParser.AddSubExpressionContext context) {
      if (context.expression() is SeedPythonParser.ExpressionContext[] exprs && exprs.Length == 2) {
        IToken op = (context.addSubOperator().GetChild(0) as ITerminalNode).Symbol;
        return _helper.BuildBinary(op, TokenToOperator(op), exprs, this);
      }
      return null;
    }

    // Visits a compare expression.
    public override AstNode VisitComapreExpression(
        [NotNull] SeedPythonParser.ComapreExpressionContext context) {
      if (context.expression() is SeedPythonParser.ExpressionContext[] exprs &&
          context.compareOperator() is SeedPythonParser.CompareOperatorContext[] ops &&
          ops.Length > 0 && exprs.Length == ops.Length + 1) {
        return _helper.BuildCompare(exprs, ops, ToCompareOperator, this);
      }
      return null;
    }

    // Visits an identifier.
    public override AstNode VisitIdentifier([NotNull] SeedPythonParser.IdentifierContext context) {
      return _helper.BuildIdentifier(context.IDENTIFIER().Symbol);
    }

    // Visits a number expression.
    public override AstNode VisitNumber([NotNull] SeedPythonParser.NumberContext context) {
      return _helper.BuildNumber(context.NUMBER().Symbol);
    }

    // Visits a grouping expression.
    //
    // There is no corresponding grouping AST node. The order of the expression node in the AST tree
    // represents the grouping structure.
    // The parser still calls this method with null references or an invalid terminal node when
    // syntax errors happen. Returns a null AST node in this situation.
    public override AstNode VisitGrouping([NotNull] SeedPythonParser.GroupingContext context) {
      if (context.expression() is SeedPythonParser.ExpressionContext expr &&
          context.CLOSE_PAREN() is ITerminalNode closeParen && closeParen.Symbol.TokenIndex >= 0) {
        return _helper.BuildGrouping(context.OPEN_PAREN().Symbol, expr, closeParen.Symbol, this);
      }
      return null;
    }
    // Visits a single statement.
    public override AstNode VisitSingleStatement(
        [NotNull] SeedPythonParser.SingleStatementContext context) {
      return Visit(context.smallStatement());
    }

    // Visits a simple statement.
    //
    // The small_stmt() method of the Simple_stmtContext returns a array which contains all the
    // small statements. There is at least one small statement in it.
    public override AstNode VisitSimpleStatement(
        [NotNull] SeedPythonParser.SimpleStatementContext context) {
      // TODO: parse all the small statements in it, only parse the first one now.
      SeedPythonParser.SmallStatementContext[] smallStatements = context.smallStatement();
      Debug.Assert(smallStatements.Length > 0);
      return Visit(smallStatements[0]);
    }

    // Visits an assignment statement.
    public override AstNode VisitAssignStatement(
        [NotNull] SeedPythonParser.AssignStatementContext context) {
      if (context.expression() is SeedPythonParser.ExpressionContext expr) {
        return _helper.BuildAssign(context.IDENTIFIER().Symbol, context.EQUAL().Symbol, expr, this);
      }
      return null;
    }

    // Visits an expression statement.
    public override AstNode VisitExpressionStatement(
        [NotNull] SeedPythonParser.ExpressionStatementContext context) {
      return VisitorHelper.BuildExpressionStatement(context.expression(), this);
    }

    internal static BinaryOperator TokenToOperator(IToken token) {
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
          throw new ArgumentException("Unsupported binary operator token.");
      }
    }

    internal static CompareOperator ToCompareOperator(ParserRuleContext context) {
      Debug.Assert(context.ChildCount == 1 && context.GetChild(0) is ITerminalNode);
      int tokenType = (context.GetChild(0) as ITerminalNode).Symbol.Type;
      switch (tokenType) {
        case SeedPythonParser.LESS:
          return CompareOperator.Less;
        case SeedPythonParser.GREAT:
          return CompareOperator.Great;
        case SeedPythonParser.LESSEQUAL:
          return CompareOperator.LessEqual;
        case SeedPythonParser.GREATEQUAL:
          return CompareOperator.GreatEqual;
        case SeedPythonParser.EQUALEQUAL:
          return CompareOperator.EqualEqual;
        case SeedPythonParser.NOTEQUAL:
          return CompareOperator.NotEqual;
        default:
          throw new NotImplementedException($"Unsupported compare operator token: {tokenType}.");
      }
    }
  }
}
