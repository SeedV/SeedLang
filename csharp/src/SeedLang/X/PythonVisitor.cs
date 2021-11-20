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
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using SeedLang.Ast;
using SeedLang.Common;
using SeedLang.Runtime;

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

    public override AstNode VisitFile([NotNull] SeedPythonParser.FileContext context) {
      // TODO: implement it.
      return VisitChildren(context);
    }

    public override AstNode VisitInteractive(
        [NotNull] SeedPythonParser.InteractiveContext context) {
      return Visit(context.statement());
    }

    public override AstNode VisitExpression_stmt(
        [NotNull] SeedPythonParser.Expression_stmtContext context) {
      return VisitorHelper.BuildExpressionStatement(context.star_expressions(), this);
    }

    public override AstNode VisitAssignment([NotNull] SeedPythonParser.AssignmentContext context) {
      return _helper.BuildAssignment(context.NAME().Symbol, context.EQUAL().Symbol,
                                     context.star_expressions(), this);
    }

    public override AstNode VisitMultiple_comparison(
      [NotNull] SeedPythonParser.Multiple_comparisonContext context) {
      return _helper.BuildComparison(context.bitwise_or(), context.compare_op_bitwise_or_pair(),
                                     ToComparisonOperator, this);
    }

    public override AstNode VisitAdd([NotNull] SeedPythonParser.AddContext context) {
      IToken opToken = (context.GetChild(1) as ITerminalNode).Symbol;
      return _helper.BuildBinary(context.sum(), opToken, BinaryOperator.Add, context.term(),
                                 this);
    }

    public override AstNode VisitSubstract([NotNull] SeedPythonParser.SubstractContext context) {
      IToken opToken = (context.GetChild(1) as ITerminalNode).Symbol;
      return _helper.BuildBinary(context.sum(), opToken, BinaryOperator.Subtract, context.term(),
                                 this);
    }

    public override AstNode VisitDivide([NotNull] SeedPythonParser.DivideContext context) {
      IToken opToken = (context.GetChild(1) as ITerminalNode).Symbol;
      return _helper.BuildBinary(context.term(), opToken, BinaryOperator.Divide, context.factor(),
                                 this);
    }

    public override AstNode VisitMultiply([NotNull] SeedPythonParser.MultiplyContext context) {
      IToken opToken = (context.GetChild(1) as ITerminalNode).Symbol;
      return _helper.BuildBinary(context.term(), opToken, BinaryOperator.Multiply, context.factor(),
                                 this);
    }

    public override AstNode VisitPos_factor([NotNull] SeedPythonParser.Pos_factorContext context) {
      // TODO
      IToken opToken = (context.GetChild(0) as ITerminalNode).Symbol;
      return _helper.BuildUnary(opToken, context.factor(), this);
    }

    public override AstNode VisitNag_factor([NotNull] SeedPythonParser.Nag_factorContext context) {
      IToken opToken = (context.GetChild(0) as ITerminalNode).Symbol;
      return _helper.BuildUnary(opToken, context.factor(), this);
    }

    public override AstNode VisitName([NotNull] SeedPythonParser.NameContext context) {
      return _helper.BuildIdentifier(context.NAME().Symbol);
    }

    public override AstNode VisitTrue([NotNull] SeedPythonParser.TrueContext context) {
      // TODO:
      return null;
    }

    public override AstNode VisitFalse([NotNull] SeedPythonParser.FalseContext context) {
      // TODO:
      return null;
    }

    public override AstNode VisitNone([NotNull] SeedPythonParser.NoneContext context) {
      // TODO:
      return null;
    }

    public override AstNode VisitNumber([NotNull] SeedPythonParser.NumberContext context) {
      return _helper.BuildNumber(context.NUMBER().Symbol);
    }

    // Visits a grouping expression.
    //
    // There is no corresponding grouping AST node. The order of the expression node in the AST tree
    // represents the grouping structure.
    // The parser still calls this method with null references or an invalid terminal node when
    // syntax errors happen. Returns a null AST node in this situation.
    public override AstNode VisitGroup([NotNull] SeedPythonParser.GroupContext context) {
      return _helper.BuildGrouping(context.OPEN_PAREN().Symbol, context.named_expression(),
                                   context.CLOSE_PAREN().Symbol, this);
    }

    private static ComparisonOperator ToComparisonOperator(IToken token) {
      switch (token.Type) {
        case SeedPythonParser.LESS:
          return ComparisonOperator.Less;
        case SeedPythonParser.GREATER:
          return ComparisonOperator.Greater;
        case SeedPythonParser.LESS_EQUAL:
          return ComparisonOperator.LessEqual;
        case SeedPythonParser.GREATER_EQUAL:
          return ComparisonOperator.GreaterEqual;
        case SeedPythonParser.EQ_EQUAL:
          return ComparisonOperator.EqEqual;
        case SeedPythonParser.NOT_EQUAL:
          return ComparisonOperator.NotEqual;
        default:
          throw new NotImplementedException($"Unsupported comparison operator token: {token.Type}.");
      }
    }
  }
}
