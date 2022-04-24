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

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
  // result of the last one. SeedPythonVisitor overrides the method if the default implement is not
  // correct.
  internal class SeedPythonVisitor : SeedPythonBaseVisitor<AstNode> {
    private readonly VisitorHelper _helper;

    public SeedPythonVisitor(IList<TokenInfo> tokens) {
      _helper = new VisitorHelper(tokens);
    }

    public override AstNode VisitProgram([NotNull] SeedPythonParser.ProgramContext context) {
      return Visit(context.statements());
    }

    public override AstNode VisitStatements([NotNull] SeedPythonParser.StatementsContext context) {
      return VisitorHelper.BuildBlock(context.NEWLINE(), context.statement(), this);
    }

    public override AstNode VisitSimple_stmts(
        [NotNull] SeedPythonParser.Simple_stmtsContext context) {
      return _helper.BuildSimpleStatements(context.simple_stmt(), context.SEMICOLON(), this);
    }

    public override AstNode VisitExpression_stmt(
        [NotNull] SeedPythonParser.Expression_stmtContext context) {
      SeedPythonParser.ExpressionsContext exprs = context.expressions();
      return _helper.BuildExpressionStmt(exprs.expression(), exprs.COMMA(), this);
    }

    public override AstNode VisitPass([NotNull] SeedPythonParser.PassContext context) {
      return _helper.BuildTokenOnlyStatement(context.PASS().Symbol, Statement.Pass);
    }

    public override AstNode VisitBreak([NotNull] SeedPythonParser.BreakContext context) {
      return _helper.BuildTokenOnlyStatement(context.BREAK().Symbol, Statement.Break);
    }

    public override AstNode VisitContinue([NotNull] SeedPythonParser.ContinueContext context) {
      return _helper.BuildTokenOnlyStatement(context.CONTINUE().Symbol, Statement.Continue);
    }

    public override AstNode VisitSingle_line_vtag_stmt(
        [NotNull] SeedPythonParser.Single_line_vtag_stmtContext context) {
      (var startToken, var nameTokens, var argContexts) = VisitVTagStart(context.vtag_start());
      var statementContexts = context.statement() is null ?
                              Array.Empty<ParserRuleContext>() :
                              new ParserRuleContext[] { context.statement() };
      return _helper.BuildVTag(startToken, nameTokens, argContexts, context.VTAG_END().Symbol,
                               statementContexts, this);
    }

    public override AstNode VisitMultiple_line_vtag_stmt(
        [NotNull] SeedPythonParser.Multiple_line_vtag_stmtContext context) {
      (var startToken, var nameTokens, var argContexts) = VisitVTagStart(context.vtag_start());
      return _helper.BuildVTag(startToken, nameTokens, argContexts, context.VTAG_END().Symbol,
                               context.statements().statement(), this);
    }

    public override AstNode VisitAssign([NotNull] SeedPythonParser.AssignContext context) {
      SeedPythonParser.TargetsContext targets = context.targets();
      SeedPythonParser.ExpressionsContext exprs = context.expressions();
      return _helper.BuildAssignment(targets.target(), targets.COMMA(), context.EQUAL().Symbol,
                                     exprs.expression(), exprs.COMMA(), this);
    }

    public override AstNode VisitAdd_assign([NotNull] SeedPythonParser.Add_assignContext context) {
      return _helper.BuildAugAssignment(context.target(), context.ADD_ASSIGN().Symbol,
                                        BinaryOperator.Add, context.expression(), this);
    }

    public override AstNode VisitSubstract_assign(
        [NotNull] SeedPythonParser.Substract_assignContext context) {
      return _helper.BuildAugAssignment(context.target(), context.SUBSTRACT_ASSIGN().Symbol,
                                        BinaryOperator.Subtract, context.expression(), this);
    }

    public override AstNode VisitMultiply_assign(
        [NotNull] SeedPythonParser.Multiply_assignContext context) {
      return _helper.BuildAugAssignment(context.target(), context.MULTIPLY_ASSIGN().Symbol,
                                        BinaryOperator.Multiply, context.expression(), this);
    }

    public override AstNode VisitDivide_assign(
        [NotNull] SeedPythonParser.Divide_assignContext context) {
      return _helper.BuildAugAssignment(context.target(), context.DIVIDE_ASSIGN().Symbol,
                                        BinaryOperator.Divide, context.expression(), this);
    }

    public override AstNode VisitModulo_assign(
        [NotNull] SeedPythonParser.Modulo_assignContext context) {
      return _helper.BuildAugAssignment(context.target(), context.MODULO_ASSIGN().Symbol,
                                        BinaryOperator.Modulo, context.expression(), this);
    }

    public override AstNode VisitSubscript_target(
      [NotNull] SeedPythonParser.Subscript_targetContext context) {
      return _helper.BuildSubscript(context.primary(), context.OPEN_BRACK().Symbol,
                                    context.expression(), context.CLOSE_BRACK().Symbol, this);
    }

    public override AstNode VisitIf_elif([NotNull] SeedPythonParser.If_elifContext context) {
      return _helper.BuildIfElif(context.IF().Symbol, context.expression(), context.COLON().Symbol,
                                 context.block(), context.elif_stmt(), this);
    }

    public override AstNode VisitIf_else([NotNull] SeedPythonParser.If_elseContext context) {
      return _helper.BuildIfElse(context.IF().Symbol, context.expression(), context.COLON().Symbol,
                                 context.block(), context.else_block(), this);
    }

    public override AstNode VisitElif_elif([NotNull] SeedPythonParser.Elif_elifContext context) {
      return _helper.BuildIfElif(context.ELIF().Symbol, context.expression(),
                                 context.COLON().Symbol, context.block(), context.elif_stmt(),
                                 this);
    }

    public override AstNode VisitElif_else([NotNull] SeedPythonParser.Elif_elseContext context) {
      return _helper.BuildIfElse(context.ELIF().Symbol, context.expression(),
                                 context.COLON().Symbol, context.block(), context.else_block(),
                                 this);
    }

    public override AstNode VisitElse_block([NotNull] SeedPythonParser.Else_blockContext context) {
      return _helper.BuildElse(context.ELSE().Symbol, context.COLON().Symbol, context.block(),
                               this);
    }

    public override AstNode VisitFor_in_stmt(
        [NotNull] SeedPythonParser.For_in_stmtContext context) {
      return _helper.BuildForIn(context.FOR().Symbol, context.identifier(), context.IN().Symbol,
                                context.expression(), context.COLON().Symbol, context.block(),
                                this);
    }

    public override AstNode VisitWhile_stmt([NotNull] SeedPythonParser.While_stmtContext context) {
      return _helper.BuildWhile(context.WHILE().Symbol, context.expression(),
                                context.COLON().Symbol, context.block(), this);
    }

    public override AstNode VisitFunction_def(
        [NotNull] SeedPythonParser.Function_defContext context) {
      SeedPythonParser.ParametersContext parameters = context.parameters();
      var parameterNodes = Array.Empty<ITerminalNode>();
      var commaNodes = Array.Empty<ITerminalNode>();
      if (!(parameters is null)) {
        parameterNodes = parameters.NAME();
        commaNodes = parameters.COMMA();
      }
      return _helper.BuildFuncDef(context.DEF().Symbol, context.NAME().Symbol,
                                  context.OPEN_PAREN().Symbol, parameterNodes, commaNodes,
                                  context.CLOSE_PAREN().Symbol, context.COLON().Symbol,
                                  context.block(), this);
    }

    public override AstNode VisitReturn_stmt(
        [NotNull] SeedPythonParser.Return_stmtContext context) {
      return _helper.BuildReturn(context.RETURN().Symbol, context.expressions()?.expression(),
                                 context.expressions()?.COMMA(), this);
    }

    public override AstNode VisitStatements_as_block(
        [NotNull] SeedPythonParser.Statements_as_blockContext context) {
      return Visit(context.statements());
    }

    public override AstNode VisitDisjunction(
        [NotNull] SeedPythonParser.DisjunctionContext context) {
      ParserRuleContext[] operands = context.conjunction();
      if (operands.Length == 1) {
        return Visit(operands[0]);
      }
      return _helper.BuildAndOr(BooleanOperator.Or, operands, context.OR(), this);
    }

    public override AstNode VisitConjunction(
        [NotNull] SeedPythonParser.ConjunctionContext context) {
      ParserRuleContext[] operands = context.inversion();
      if (operands.Length == 1) {
        return Visit(operands[0]);
      }
      return _helper.BuildAndOr(BooleanOperator.And, operands, context.AND(), this);
    }

    public override AstNode VisitNot([NotNull] SeedPythonParser.NotContext context) {
      return _helper.BuildUnary(context.NOT().Symbol, UnaryOperator.Not, context.inversion(), this);
    }

    public override AstNode VisitComparison([NotNull] SeedPythonParser.ComparisonContext context) {
      ParserRuleContext[] operands = context.bitwise_or();
      Debug.Assert(operands.Length > 0);
      ParserRuleContext[] operators = context.comparison_op();
      if (operators.Length == 0) {
        Debug.Assert(operands.Length == 1);
        return Visit(operands[0]);
      }
      var opTokens = new IToken[operators.Length];
      var ops = new ComparisonOperator[operators.Length];
      for (int i = 0; i < operators.Length; i++) {
        opTokens[i] = (operators[i].GetChild(0) as ITerminalNode).Symbol;
        ops[i] = ToComparisonOperator(opTokens[i]);
      }
      return _helper.BuildComparison(operands, opTokens, ops, this);
    }

    public override AstNode VisitAdd([NotNull] SeedPythonParser.AddContext context) {
      return _helper.BuildBinary(context.sum(), context.ADD().Symbol, BinaryOperator.Add,
                                 context.term(), this);
    }

    public override AstNode VisitSubtract([NotNull] SeedPythonParser.SubtractContext context) {
      return _helper.BuildBinary(context.sum(), context.SUBTRACT().Symbol, BinaryOperator.Subtract,
                                 context.term(), this);
    }

    public override AstNode VisitDivide([NotNull] SeedPythonParser.DivideContext context) {
      return _helper.BuildBinary(context.term(), context.DIVIDE().Symbol, BinaryOperator.Divide,
                                 context.factor(), this);
    }

    public override AstNode VisitMultiply([NotNull] SeedPythonParser.MultiplyContext context) {
      return _helper.BuildBinary(context.term(), context.MULTIPLY().Symbol, BinaryOperator.Multiply,
                                 context.factor(), this);
    }

    public override AstNode VisitFloor_divide(
        [NotNull] SeedPythonParser.Floor_divideContext context) {
      return _helper.BuildBinary(context.term(), context.FLOOR_DIVIDE().Symbol,
                                 BinaryOperator.FloorDivide, context.factor(), this);
    }

    public override AstNode VisitModulo([NotNull] SeedPythonParser.ModuloContext context) {
      return _helper.BuildBinary(context.term(), context.MODULO().Symbol, BinaryOperator.Modulo,
                                 context.factor(), this);
    }

    public override AstNode VisitPositive([NotNull] SeedPythonParser.PositiveContext context) {
      return _helper.BuildUnary(context.ADD().Symbol, UnaryOperator.Positive, context.factor(),
                                this);
    }

    public override AstNode VisitNegative([NotNull] SeedPythonParser.NegativeContext context) {
      return _helper.BuildUnary(context.SUBTRACT().Symbol, UnaryOperator.Negative, context.factor(),
                                this);
    }

    public override AstNode VisitPower([NotNull] SeedPythonParser.PowerContext context) {
      return _helper.BuildBinary(context.primary(), context.POWER().Symbol, BinaryOperator.Power,
                                 context.factor(), this);
    }

    public override AstNode VisitCall([NotNull] SeedPythonParser.CallContext context) {
      SeedPythonParser.ArgumentsContext arguments = context.arguments();
      var exprContexts = Array.Empty<ParserRuleContext>();
      var commaNodes = Array.Empty<ITerminalNode>();
      if (!(arguments is null)) {
        exprContexts = arguments.expression();
        commaNodes = arguments.COMMA();
      }
      return _helper.BuildCall(context.primary(), context.OPEN_PAREN().Symbol, exprContexts,
                               commaNodes, context.CLOSE_PAREN().Symbol, this);
    }

    public override AstNode VisitSubscript([NotNull] SeedPythonParser.SubscriptContext context) {
      return _helper.BuildSubscript(context.primary(), context.OPEN_BRACK().Symbol,
                                    context.expression(), context.CLOSE_BRACK().Symbol, this);
    }

    public override AstNode VisitAttribute([NotNull] SeedPythonParser.AttributeContext context) {
      return _helper.BuildAttribute(context.primary(), context.DOT().Symbol, context.identifier(),
                                    this);
    }

    public override AstNode VisitTrue([NotNull] SeedPythonParser.TrueContext context) {
      return _helper.BuildBooleanConstant(context.TRUE().Symbol, true);
    }

    public override AstNode VisitFalse([NotNull] SeedPythonParser.FalseContext context) {
      return _helper.BuildBooleanConstant(context.FALSE().Symbol, false);
    }

    public override AstNode VisitNone([NotNull] SeedPythonParser.NoneContext context) {
      return _helper.BuildNilConstant(context.NONE().Symbol);
    }

    public override AstNode VisitNumber([NotNull] SeedPythonParser.NumberContext context) {
      return _helper.BuildNumberConstant(context.NUMBER().Symbol);
    }

    public override AstNode VisitStrings([NotNull] SeedPythonParser.StringsContext context) {
      return _helper.BuildStringConstant(context.STRING());
    }

    public override AstNode VisitIdentifier([NotNull] SeedPythonParser.IdentifierContext context) {
      return _helper.BuildIdentifier(context.NAME().Symbol);
    }

    public override AstNode VisitGroup([NotNull] SeedPythonParser.GroupContext context) {
      return _helper.BuildGrouping(context.OPEN_PAREN().Symbol, context.expression(),
                                   context.CLOSE_PAREN().Symbol, this);
    }

    public override AstNode VisitDict([NotNull] SeedPythonParser.DictContext context) {
      SeedPythonParser.KvpairsContext kvPairs = context.kvpairs();
      var keyContexts = new List<ParserRuleContext>();
      var valueContexts = new List<ParserRuleContext>();
      var colonNodes = new List<IToken>();
      ITerminalNode[] commaNodes = Array.Empty<ITerminalNode>();
      if (!(kvPairs is null)) {
        foreach (SeedPythonParser.KvpairContext kvPair in kvPairs.kvpair()) {
          ParserRuleContext[] exprs = kvPair.expression();
          Debug.Assert(exprs.Length == 2);
          keyContexts.Add(exprs[0]);
          valueContexts.Add(exprs[1]);
          colonNodes.Add(kvPair.COLON().Symbol);
        }
        commaNodes = kvPairs.COMMA();
      }
      return _helper.BuildDict(context.OPEN_BRACE().Symbol, keyContexts, valueContexts, colonNodes,
                               commaNodes, context.CLOSE_BRACE().Symbol, this);
    }

    public override AstNode VisitList([NotNull] SeedPythonParser.ListContext context) {
      // There isn't a corresponding AST node for the parse rule "expressions". Parses them by the
      // BuildList function.
      var exprContexts = Array.Empty<ParserRuleContext>();
      ITerminalNode[] commaNodes = Array.Empty<ITerminalNode>();
      if (!(context.expressions() is null)) {
        exprContexts = context.expressions().expression();
        commaNodes = context.expressions().COMMA();
      }
      return _helper.BuildList(context.OPEN_BRACK().Symbol, exprContexts, commaNodes,
                               context.CLOSE_BRACK().Symbol, this);
    }

    public override AstNode VisitTuple([NotNull] SeedPythonParser.TupleContext context) {
      // There isn't a corresponding AST node for the parse rule "expressions". Parses them by the
      // BuildTuple function.
      var exprContexts = Array.Empty<ParserRuleContext>();
      ITerminalNode[] commaNodes = Array.Empty<ITerminalNode>();
      if (!(context.expressions() is null)) {
        exprContexts = context.expressions().expression();
        commaNodes = context.expressions().COMMA();
      }
      return _helper.BuildTuple(context.OPEN_PAREN().Symbol, exprContexts, commaNodes,
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
        case SeedPythonParser.IN:
          return ComparisonOperator.In;
        default:
          throw new NotImplementedException(
              $"Unsupported comparison operator token: {token.Type}.");
      }
    }

    private (IToken, IToken[], ParserRuleContext[][]) VisitVTagStart(
    SeedPythonParser.Vtag_startContext vTagStartContext) {
      SeedPythonParser.VtagContext[] vTagContexts = vTagStartContext.vtag();
      var nameTokens = new IToken[vTagContexts.Length];
      var argContexts = new ParserRuleContext[vTagContexts.Length][];
      for (int i = 0; i < vTagContexts.Length; i++) {
        nameTokens[i] = vTagContexts[i].NAME().Symbol;
        argContexts[i] = vTagContexts[i].arguments()?.expression() ?? null;
      }
      return (vTagStartContext.VTAG_START().Symbol, nameTokens, argContexts);
    }
  }
}
