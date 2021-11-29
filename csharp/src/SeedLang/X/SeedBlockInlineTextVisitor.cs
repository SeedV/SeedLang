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

namespace SeedLang.X {
  // The visitor class to visit a block inline text of SeedBlock programs and generate the
  // corresponding AST tree.
  //
  // The default implement of SeedBlockInlineTextBaseVisitor is to visit all the children and return
  // the result of the last one. SeedBlockInlineTextVisitor overrides the method if the default
  // implement is not correct.
  internal class SeedBlockInlineTextVisitor : SeedBlockInlineTextBaseVisitor<AstNode> {
    private readonly VisitorHelper _helper;

    public SeedBlockInlineTextVisitor(IList<SyntaxToken> tokens) {
      _helper = new VisitorHelper(tokens);
    }

    public override AstNode VisitExpressionStatement(
        [NotNull] SeedBlockInlineTextParser.ExpressionStatementContext context) {
      return Visit(context.expression());
    }

    public override AstNode VisitMultiple_comparison(
        [NotNull] SeedBlockInlineTextParser.Multiple_comparisonContext context) {
      return _helper.BuildComparison(context.bitwise_or(), context.compare_op_bitwise_or_pair(),
                                     ToComparisonOperator, this);
    }

    public override AstNode VisitAdd([NotNull] SeedBlockInlineTextParser.AddContext context) {
      return _helper.BuildBinary(context.sum(), context.ADD().Symbol, BinaryOperator.Add,
                                 context.term(), this);
    }

    public override AstNode VisitSubtract(
        [NotNull] SeedBlockInlineTextParser.SubtractContext context) {
      return _helper.BuildBinary(context.sum(), context.SUBTRACT().Symbol, BinaryOperator.Subtract,
                                 context.term(), this);
    }

    public override AstNode VisitDivide([NotNull] SeedBlockInlineTextParser.DivideContext context) {
      return _helper.BuildBinary(context.term(), context.DIVIDE().Symbol, BinaryOperator.Divide,
                                 context.factor(), this);
    }

    public override AstNode VisitMultiply(
        [NotNull] SeedBlockInlineTextParser.MultiplyContext context) {
      return _helper.BuildBinary(context.term(), context.MULTIPLY().Symbol, BinaryOperator.Multiply,
                                 context.factor(), this);
    }

    public override AstNode VisitFloor_divide(
        [NotNull] SeedBlockInlineTextParser.Floor_divideContext context) {
      return _helper.BuildBinary(context.term(), context.FLOOR_DIVIDE().Symbol,
                                 BinaryOperator.FloorDivide, context.factor(), this);
    }

    public override AstNode VisitModulo([NotNull] SeedBlockInlineTextParser.ModuloContext context) {
      return _helper.BuildBinary(context.term(), context.MODULO().Symbol, BinaryOperator.Modulo,
                                 context.factor(), this);
    }

    public override AstNode VisitPositive(
        [NotNull] SeedBlockInlineTextParser.PositiveContext context) {
      return _helper.BuildUnary(context.ADD().Symbol, UnaryOperator.Positive, context.factor(),
                                this);
    }

    public override AstNode VisitNegative(
        [NotNull] SeedBlockInlineTextParser.NegativeContext context) {
      return _helper.BuildUnary(context.SUBTRACT().Symbol, UnaryOperator.Negative,
                                context.factor(), this);
    }

    public override AstNode VisitPower([NotNull] SeedBlockInlineTextParser.PowerContext context) {
      return _helper.BuildBinary(context.primary(), context.POWER().Symbol, BinaryOperator.Power,
                                 context.factor(), this);
    }

    public override AstNode VisitName([NotNull] SeedBlockInlineTextParser.NameContext context) {
      return _helper.BuildIdentifier(context.NAME().Symbol);
    }

    public override AstNode VisitTrue([NotNull] SeedBlockInlineTextParser.TrueContext context) {
      return _helper.BuildBooleanConstant(context.TRUE().Symbol, true);
    }

    public override AstNode VisitFalse([NotNull] SeedBlockInlineTextParser.FalseContext context) {
      return _helper.BuildBooleanConstant(context.FALSE().Symbol, false);
    }

    public override AstNode VisitNone([NotNull] SeedBlockInlineTextParser.NoneContext context) {
      // TODO: return a none constant expresssion.
      return null;
    }

    public override AstNode VisitNumber([NotNull] SeedBlockInlineTextParser.NumberContext context) {
      return _helper.BuildNumberConstant(context.NUMBER().Symbol);
    }

    public override AstNode VisitGroup([NotNull] SeedBlockInlineTextParser.GroupContext context) {
      return _helper.BuildGrouping(context.OPEN_PAREN().Symbol, context.expression(),
                                   context.CLOSE_PAREN().Symbol, this);
    }

    private static ComparisonOperator ToComparisonOperator(IToken token) {
      switch (token.Type) {
        case SeedBlockInlineTextParser.LESS:
          return ComparisonOperator.Less;
        case SeedBlockInlineTextParser.GREATER:
          return ComparisonOperator.Greater;
        case SeedBlockInlineTextParser.LESS_EQUAL:
          return ComparisonOperator.LessEqual;
        case SeedBlockInlineTextParser.GREATER_EQUAL:
          return ComparisonOperator.GreaterEqual;
        case SeedBlockInlineTextParser.EQ_EQUAL:
          return ComparisonOperator.EqEqual;
        case SeedBlockInlineTextParser.NOT_EQUAL:
          return ComparisonOperator.NotEqual;
        default:
          throw new NotImplementedException($"Unsupported comparison operator: {token.Type}.");
      }
    }
  }
}
