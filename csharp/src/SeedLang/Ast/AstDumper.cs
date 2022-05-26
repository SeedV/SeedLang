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
using System.Linq;
using System.Text;
using SeedLang.Runtime;

namespace SeedLang.Ast {
  internal static class OperatorExtensions {
    // Returns the internal string representation of binary operators.
    internal static string Symbol(this BinaryOperator op) {
      switch (op) {
        case BinaryOperator.Add:
          return "+";
        case BinaryOperator.Subtract:
          return "-";
        case BinaryOperator.Multiply:
          return "*";
        case BinaryOperator.Divide:
          return "/";
        case BinaryOperator.FloorDivide:
          return "//";
        case BinaryOperator.Power:
          return "**";
        case BinaryOperator.Modulo:
          return "%";
        default:
          throw new NotImplementedException($"Unsupported binary operator: {op}.");
      }
    }

    // Returns the internal string representation of boolean operators.
    internal static string Symbol(this BooleanOperator op) {
      switch (op) {
        case BooleanOperator.And:
          return "and";
        case BooleanOperator.Or:
          return "or";
        default:
          throw new NotImplementedException($"Unsupported boolean operator: {op}.");
      }
    }

    // Returns the internal string representation of comparison operators.
    internal static string Symbol(this ComparisonOperator op) {
      switch (op) {
        case ComparisonOperator.Less:
          return "<";
        case ComparisonOperator.Greater:
          return ">";
        case ComparisonOperator.LessEqual:
          return "<=";
        case ComparisonOperator.GreaterEqual:
          return ">=";
        case ComparisonOperator.EqEqual:
          return "==";
        case ComparisonOperator.NotEqual:
          return "!=";
        case ComparisonOperator.In:
          return "in";
        default:
          throw new NotImplementedException($"Unsupported comparison operator: {op}.");
      }
    }

    // Returns the internal string representation of unary operators.
    internal static string Symbol(this UnaryOperator op) {
      switch (op) {
        case UnaryOperator.Positive:
          return "+";
        case UnaryOperator.Negative:
          return "-";
        case UnaryOperator.Not:
          return "not";
        default:
          throw new NotImplementedException($"Unsupported unary operator: {op}.");
      }
    }
  }

  // A helper class to create the string representation of an AST tree.
  internal class AstDumper {
    private class HeaderDumper {
      private readonly StringBuilder _builder;
      private int _level = 0;

      internal HeaderDumper(StringBuilder builder) {
        _builder = builder;
      }

      internal void Enter(AstNode node) {
        if (_level > 0) {
          _builder.AppendLine();
          _builder.Append($"{new string(' ', _level * 2)}");
        }
        _builder.Append($"{node.Range} {node.GetType().Name}");
        _level++;
      }

      internal void Exit(AstNode _) {
        _level--;
      }
    }

    private class ExpressionDumper : ExpressionWalker {
      private readonly StringBuilder _builder;
      private readonly HeaderDumper _headerDumper;

      public ExpressionDumper(StringBuilder builder, HeaderDumper headerDumper) {
        _builder = builder;
        _headerDumper = headerDumper;
      }

      protected override void Enter(Expression expr) {
        _headerDumper.Enter(expr);
      }

      protected override void Exit(Expression expr) {
        _headerDumper.Exit(expr);
      }

      protected override void VisitBinary(BinaryExpression binary) {
        _builder.Append($" ({binary.Op.Symbol()})");
        Visit(binary.Left);
        Visit(binary.Right);
      }

      protected override void VisitBoolean(BooleanExpression boolean) {
        _builder.Append($" ({boolean.Op.Symbol()})");
        foreach (Expression expr in boolean.Exprs) {
          Visit(expr);
        }
      }

      protected override void VisitBooleanConstant(BooleanConstantExpression booleanConstant) {
        _builder.Append($" ({booleanConstant.Value})");
      }

      protected override void VisitCall(CallExpression call) {
        Visit(call.Func);
        foreach (Expression argument in call.Arguments) {
          Visit(argument);
        }
      }

      protected override void VisitComparison(ComparisonExpression comparison) {
        Visit(comparison.First);
        for (int i = 0; i < comparison.Ops.Length; ++i) {
          _builder.Append($" ({comparison.Ops[i].Symbol()})");
          Visit(comparison.Exprs[i]);
        }
      }

      protected override void VisitDict(DictExpression dict) {
        foreach (var item in dict.Items) {
          Visit(item.Key);
          Visit(item.Value);
        }
      }

      protected override void VisitIdentifier(IdentifierExpression identifier) {
        _builder.Append($" ({identifier.Name})");
      }

      protected override void VisitList(ListExpression list) {
        foreach (Expression expr in list.Exprs) {
          Visit(expr);
        }
      }

      protected override void VisitNilConstant(NilConstantExpression nilConstant) { }

      protected override void VisitNumberConstant(NumberConstantExpression numberConstant) {
        _builder.Append($" ({numberConstant.Value})");
      }

      protected override void VisitSlice(SliceExpression slice) {
        _builder.Append($" ({(slice.Start is null ? "" : "start")}:");
        _builder.Append($"{(slice.Stop is null ? "" : "stop")}:");
        _builder.Append($"{(slice.Step is null ? "" : "step")})");
        if (!(slice.Start is null)) {
          Visit(slice.Start);
        }
        if (!(slice.Stop is null)) {
          Visit(slice.Stop);
        }
        if (!(slice.Step is null)) {
          Visit(slice.Step);
        }
      }

      protected override void VisitStringConstant(StringConstantExpression stringConstant) {
        _builder.Append($" ({stringConstant.Value})");
      }

      protected override void VisitSubscript(SubscriptExpression subscript) {
        Visit(subscript.Container);
        Visit(subscript.Key);
      }

      protected override void VisitTuple(TupleExpression tuple) {
        foreach (Expression expr in tuple.Exprs) {
          Visit(expr);
        }
      }

      protected override void VisitUnary(UnaryExpression unary) {
        _builder.Append($" ({unary.Op.Symbol()})");
        Visit(unary.Expr);
      }
    }

    private class StatementDumper : StatementWalker {
      private readonly StringBuilder _builder;
      private readonly ExpressionDumper _exprDumper;
      private readonly HeaderDumper _headerDumper;

      public StatementDumper(StringBuilder builder, HeaderDumper headerDumper,
                             ExpressionDumper exprDumper) {
        _builder = builder;
        _headerDumper = headerDumper;
        _exprDumper = exprDumper;
      }

      protected override void Enter(Statement statement) {
        _headerDumper.Enter(statement);
      }

      protected override void Exit(Statement statement) {
        _headerDumper.Exit(statement);
      }

      protected override void VisitAssignment(AssignmentStatement assignment) {
        foreach (Expression[] targets in assignment.Targets) {
          foreach (Expression target in targets) {
            _exprDumper.Visit(target);
          }
          _builder.Append(" =");
        }
        foreach (Expression expr in assignment.Values) {
          _exprDumper.Visit(expr);
        }
      }

      protected override void VisitBlock(BlockStatement block) {
        foreach (Statement statement in block.Statements) {
          Visit(statement);
        }
      }

      protected override void VisitBreak(BreakStatement @break) { }

      protected override void VisitContinue(ContinueStatement @continue) { }

      protected override void VisitExpression(ExpressionStatement expr) {
        _exprDumper.Visit(expr.Expr);
      }

      protected override void VisitForIn(ForInStatement forIn) {
        _exprDumper.Visit(forIn.Id);
        _exprDumper.Visit(forIn.Expr);
        Visit(forIn.Body);
      }

      protected override void VisitFuncDef(FuncDefStatement funcDef) {
        _builder.Append($" ({funcDef.Name}");
        if (funcDef.Parameters.Length > 0) {
          _builder.Append($":{string.Join(",", funcDef.Parameters.Select(param => param.Name))}");
        }
        _builder.Append(')');
        Visit(funcDef.Body);
      }

      protected override void VisitIf(IfStatement @if) {
        _exprDumper.Visit(@if.Test);
        Visit(@if.ThenBody);
        if (!(@if.ElseBody is null)) {
          Visit(@if.ElseBody);
        }
      }

      protected override void VisitPass(PassStatement pass) { }

      protected override void VisitReturn(ReturnStatement @return) {
        foreach (Expression value in @return.Exprs) {
          _exprDumper.Visit(value);
        }
      }

      protected override void VisitVTag(VTagStatement vTag) {
        _builder.Append($" ({string.Join<VTagStatement.VTagInfo>(",", vTag.VTagInfos)})");
        foreach (var statement in vTag.Statements) {
          Visit(statement);
        }
      }

      protected override void VisitWhile(WhileStatement @while) {
        _exprDumper.Visit(@while.Test);
        Visit(@while.Body);
      }
    }

    private readonly StringBuilder _builder = new StringBuilder();
    private readonly ExpressionDumper _expressionDumper;
    private readonly StatementDumper _statementDumper;
    private readonly HeaderDumper _headerDumper;

    internal AstDumper() {
      _headerDumper = new HeaderDumper(_builder);
      _expressionDumper = new ExpressionDumper(_builder, _headerDumper);
      _statementDumper = new StatementDumper(_builder, _headerDumper, _expressionDumper);
    }

    internal string Dump(AstNode node) {
      switch (node) {
        case Expression expression:
          _expressionDumper.Visit(expression);
          break;
        case Statement statement:
          _statementDumper.Visit(statement);
          break;
      }
      return _builder.ToString();
    }
  }
}
