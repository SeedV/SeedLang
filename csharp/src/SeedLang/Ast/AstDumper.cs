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
      return op switch {
        BinaryOperator.Add => "+",
        BinaryOperator.Subtract => "-",
        BinaryOperator.Multiply => "*",
        BinaryOperator.Divide => "/",
        BinaryOperator.FloorDivide => "//",
        BinaryOperator.Power => "**",
        BinaryOperator.Modulo => "%",
        _ => throw new NotImplementedException($"Unsupported binary operator: {op}."),
      };
    }

    // Returns the internal string representation of boolean operators.
    internal static string Symbol(this BooleanOperator op) {
      return op switch {
        BooleanOperator.And => "and",
        BooleanOperator.Or => "or",
        _ => throw new NotImplementedException($"Unsupported boolean operator: {op}."),
      };
    }

    // Returns the internal string representation of comparison operators.
    internal static string Symbol(this ComparisonOperator op) {
      return op switch {
        ComparisonOperator.Less => "<",
        ComparisonOperator.Greater => ">",
        ComparisonOperator.LessEqual => "<=",
        ComparisonOperator.GreaterEqual => ">=",
        ComparisonOperator.EqEqual => "==",
        ComparisonOperator.NotEqual => "!=",
        ComparisonOperator.In => "in",
        _ => throw new NotImplementedException($"Unsupported comparison operator: {op}."),
      };
    }

    // Returns the internal string representation of unary operators.
    internal static string Symbol(this UnaryOperator op) {
      return op switch {
        UnaryOperator.Positive => "+",
        UnaryOperator.Negative => "-",
        UnaryOperator.Not => "not",
        _ => throw new NotImplementedException($"Unsupported unary operator: {op}."),
      };
    }
  }

  // A helper class to create the string representation of an AST tree.
  internal class AstDumper {
    private class ExpressionDumper : ExpressionWalker<int> {
      private readonly StringBuilder _builder;

      public ExpressionDumper(StringBuilder builder) {
        _builder = builder;
      }

      protected override void Enter(Expression expr, int level) {
        AppendHeader(_builder, expr, level);
      }

      protected override void VisitBinary(BinaryExpression binary, int level) {
        _builder.Append($" ({binary.Op.Symbol()})");
        Visit(binary.Left, level + 1);
        Visit(binary.Right, level + 1);
      }

      protected override void VisitBoolean(BooleanExpression boolean, int level) {
        _builder.Append($" ({boolean.Op.Symbol()})");
        foreach (Expression expr in boolean.Exprs) {
          Visit(expr, level + 1);
        }
      }

      protected override void VisitBooleanConstant(BooleanConstantExpression booleanConstant,
                                                   int level) {
        _builder.Append($" ({booleanConstant.Value})");
      }

      protected override void VisitCall(CallExpression call, int level) {
        Visit(call.Func, level + 1);
        foreach (Expression argument in call.Arguments) {
          Visit(argument, level + 1);
        }
      }

      protected override void VisitComparison(ComparisonExpression comparison, int level) {
        Visit(comparison.First, level + 1);
        for (int i = 0; i < comparison.Ops.Length; ++i) {
          _builder.Append($" ({comparison.Ops[i].Symbol()})");
          Visit(comparison.Exprs[i], level + 1);
        }
      }

      protected override void VisitDict(DictExpression dict, int level) {
        foreach (var item in dict.KeyValues) {
          Visit(item.Key, level + 1);
          Visit(item.Value, level + 1);
        }
      }

      protected override void VisitIdentifier(IdentifierExpression identifier, int level) {
        _builder.Append($" ({identifier.Name})");
      }

      protected override void VisitList(ListExpression list, int level) {
        foreach (Expression expr in list.Exprs) {
          Visit(expr, level + 1);
        }
      }

      protected override void VisitNilConstant(NilConstantExpression nilConstant, int level) { }

      protected override void VisitNumberConstant(NumberConstantExpression numberConstant,
                                                  int level) {
        _builder.Append($" ({numberConstant.Value})");
      }

      protected override void VisitSlice(SliceExpression slice, int level) {
        _builder.Append($" ({(slice.Start is null ? "" : "start")}:");
        _builder.Append($"{(slice.Stop is null ? "" : "stop")}:");
        _builder.Append($"{(slice.Step is null ? "" : "step")})");
        if (!(slice.Start is null)) {
          Visit(slice.Start, level + 1);
        }
        if (!(slice.Stop is null)) {
          Visit(slice.Stop, level + 1);
        }
        if (!(slice.Step is null)) {
          Visit(slice.Step, level + 1);
        }
      }

      protected override void VisitStringConstant(StringConstantExpression stringConstant,
                                                  int level) {
        _builder.Append($" ({stringConstant.Value})");
      }

      protected override void VisitSubscript(SubscriptExpression subscript, int level) {
        Visit(subscript.Container, level + 1);
        Visit(subscript.Key, level + 1);
      }

      protected override void VisitTuple(TupleExpression tuple, int level) {
        foreach (Expression expr in tuple.Exprs) {
          Visit(expr, level + 1);
        }
      }

      protected override void VisitUnary(UnaryExpression unary, int level) {
        _builder.Append($" ({unary.Op.Symbol()})");
        Visit(unary.Expr, level + 1);
      }
    }

    private class StatementDumper : StatementWalker<int> {
      private readonly StringBuilder _builder;
      private readonly ExpressionDumper _exprDumper;

      public StatementDumper(StringBuilder builder, ExpressionDumper exprDumper) {
        _builder = builder;
        _exprDumper = exprDumper;
      }

      protected override void Enter(Statement statement, int level) {
        AppendHeader(_builder, statement, level);
      }

      protected override void VisitAssignment(AssignmentStatement assignment, int level) {
        foreach (Expression[] targets in assignment.Targets) {
          foreach (Expression target in targets) {
            _exprDumper.Visit(target, level + 1);
          }
          _builder.Append(" =");
        }
        foreach (Expression expr in assignment.Values) {
          _exprDumper.Visit(expr, level + 1);
        }
      }

      protected override void VisitBlock(BlockStatement block, int level) {
        foreach (Statement statement in block.Statements) {
          Visit(statement, level + 1);
        }
      }

      protected override void VisitBreak(BreakStatement @break, int level) { }

      protected override void VisitContinue(ContinueStatement @continue, int level) { }

      protected override void VisitExpression(ExpressionStatement expr, int level) {
        _exprDumper.Visit(expr.Expr, level + 1);
      }

      protected override void VisitForIn(ForInStatement forIn, int level) {
        _exprDumper.Visit(forIn.Id, level + 1);
        _exprDumper.Visit(forIn.Expr, level + 1);
        Visit(forIn.Body, level + 1);
      }

      protected override void VisitFuncDef(FuncDefStatement funcDef, int level) {
        _builder.Append($" ({funcDef.Name}");
        if (funcDef.Parameters.Length > 0) {
          _builder.Append($":{string.Join(",", funcDef.Parameters.Select(param => param.Name))}");
        }
        _builder.Append(')');
        Visit(funcDef.Body, level + 1);
      }

      protected override void VisitIf(IfStatement @if, int level) {
        _exprDumper.Visit(@if.Test, level + 1);
        Visit(@if.ThenBody, level + 1);
        if (!(@if.ElseBody is null)) {
          Visit(@if.ElseBody, level + 1);
        }
      }

      protected override void VisitImport(ImportStatement import, int level) {
        _builder.Append($" ({import.Name})");
      }

      protected override void VisitPass(PassStatement pass, int level) { }

      protected override void VisitReturn(ReturnStatement @return, int level) {
        foreach (Expression value in @return.Exprs) {
          _exprDumper.Visit(value, level + 1);
        }
      }

      protected override void VisitVTag(VTagStatement vTag, int level) {
        _builder.Append($" ({string.Join<VTagStatement.VTagInfo>(",", vTag.VTagInfos)})");
        foreach (var statement in vTag.Statements) {
          Visit(statement, level + 1);
        }
      }

      protected override void VisitWhile(WhileStatement @while, int level) {
        _exprDumper.Visit(@while.Test, level + 1);
        Visit(@while.Body, level + 1);
      }
    }

    private readonly StringBuilder _builder = new StringBuilder();
    private readonly ExpressionDumper _expressionDumper;
    private readonly StatementDumper _statementDumper;

    internal AstDumper() {
      _expressionDumper = new ExpressionDumper(_builder);
      _statementDumper = new StatementDumper(_builder, _expressionDumper);
    }

    internal string Dump(AstNode node) {
      int level = 0;
      switch (node) {
        case Expression expression:
          _expressionDumper.Visit(expression, level);
          break;
        case Statement statement:
          _statementDumper.Visit(statement, level);
          break;
      }
      return _builder.ToString();
    }

    private static void AppendHeader(StringBuilder builder, AstNode node, int level) {
      if (level > 0) {
        builder.AppendLine();
        builder.Append($"{new string(' ', level * 2)}");
      }
      builder.Append($"{node.Range} {node.GetType().Name}");
    }
  }
}
