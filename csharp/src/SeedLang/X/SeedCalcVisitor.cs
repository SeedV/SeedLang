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
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using SeedLang.Ast;
using SeedLang.Common;
using SeedLang.Runtime;

namespace SeedLang.X {
  // The visitor class to visit a SeedCalc expression and generate the corresponding AST tree.
  //
  // The default implement of SeedCalcBaseVisitor is to visit all the children and return the result
  // of the last one. SeedCalcVisitor overrides the methods if the default implement is not correct.
  internal class SeedCalcVisitor : SeedCalcBaseVisitor<AstNode> {
    private readonly VisitorHelper _helper;

    public SeedCalcVisitor(IList<SyntaxToken> tokens) {
      _helper = new VisitorHelper(tokens);
    }

    public override AstNode VisitExpressionStatement(
            [NotNull] SeedCalcParser.ExpressionStatementContext context) {
      var exprContexts = new ParserRuleContext[] { context.expression() };
      return _helper.BuildExpressionStmt(exprContexts, null, this);
    }

    public override AstNode VisitAdd([NotNull] SeedCalcParser.AddContext context) {
      return _helper.BuildBinary(context.sum(), context.ADD().Symbol, BinaryOperator.Add,
                                 context.term(), this);
    }

    public override AstNode VisitSubtract([NotNull] SeedCalcParser.SubtractContext context) {
      return _helper.BuildBinary(context.sum(), context.SUBTRACT().Symbol, BinaryOperator.Subtract,
                                 context.term(), this);
    }

    public override AstNode VisitDivide([NotNull] SeedCalcParser.DivideContext context) {
      return _helper.BuildBinary(context.term(), context.DIVIDE().Symbol, BinaryOperator.Divide,
                                 context.factor(), this);
    }

    public override AstNode VisitMultiply([NotNull] SeedCalcParser.MultiplyContext context) {
      return _helper.BuildBinary(context.term(), context.MULTIPLY().Symbol, BinaryOperator.Multiply,
                                 context.factor(), this);
    }

    public override AstNode VisitPositive([NotNull] SeedCalcParser.PositiveContext context) {
      return _helper.BuildUnary(context.ADD().Symbol, UnaryOperator.Positive, context.atom(),
                                this);
    }

    public override AstNode VisitNegative([NotNull] SeedCalcParser.NegativeContext context) {
      return _helper.BuildUnary(context.SUBTRACT().Symbol, UnaryOperator.Negative,
                                context.atom(), this);
    }

    public override AstNode VisitNumber([NotNull] SeedCalcParser.NumberContext context) {
      return _helper.BuildNumberConstant(context.NUMBER().Symbol);
    }

    public override AstNode VisitGroup([NotNull] SeedCalcParser.GroupContext context) {
      return _helper.BuildGrouping(context.OPEN_PAREN().Symbol, context.expression(),
                                   context.CLOSE_PAREN().Symbol, this);
    }
  }
}
