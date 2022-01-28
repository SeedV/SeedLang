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
          return "And";
        case BooleanOperator.Or:
          return "Or";
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
          return "Not";
        default:
          throw new NotImplementedException($"Unsupported unary operator: {op}.");
      }
    }
  }

  // A helper class to create the string representation of an AST tree.
  internal sealed class AstStringBuilder : AstWalker {
    private readonly StringBuilder _out = new StringBuilder();
    private int _level = 0;

    public override string ToString() {
      return _out.ToString();
    }

    // Outputs a given AST tree to a string.
    internal static string AstToString(AstNode node) {
      var asb = new AstStringBuilder();
      asb.Visit(node);
      return asb.ToString();
    }

    protected override void Visit(BinaryExpression binary) {
      Enter(binary);
      _out.Append($" ({binary.Op.Symbol()})");
      Visit(binary.Left);
      Visit(binary.Right);
      Exit();
    }

    protected override void Visit(BooleanExpression boolean) {
      Enter(boolean);
      _out.Append($" ({boolean.Op.Symbol()})");
      foreach (Expression expr in boolean.Exprs) {
        Visit(expr);
      }
      Exit();
    }

    protected override void Visit(ComparisonExpression comparison) {
      Enter(comparison);
      Visit(comparison.First);
      for (int i = 0; i < comparison.Ops.Length; ++i) {
        _out.Append($" ({comparison.Ops[i].Symbol()})");
        Visit(comparison.Exprs[i]);
      }
      Exit();
    }

    protected override void Visit(UnaryExpression unary) {
      Enter(unary);
      _out.Append($" ({unary.Op.Symbol()})");
      Visit(unary.Expr);
      Exit();
    }

    protected override void Visit(IdentifierExpression identifier) {
      Enter(identifier);
      _out.Append($" ({identifier.Name})");
      Exit();
    }

    protected override void Visit(BooleanConstantExpression booleanConstant) {
      Enter(booleanConstant);
      _out.Append($" ({booleanConstant.Value})");
      Exit();
    }

    protected override void Visit(NoneConstantExpression noneConstant) {
      Enter(noneConstant);
      Exit();
    }

    protected override void Visit(NumberConstantExpression numberConstant) {
      Enter(numberConstant);
      _out.Append($" ({numberConstant.Value})");
      Exit();
    }

    protected override void Visit(StringConstantExpression stringConstant) {
      Enter(stringConstant);
      _out.Append($" ({stringConstant.Value})");
      Exit();
    }

    protected override void Visit(ListExpression list) {
      Enter(list);
      foreach (Expression expr in list.Exprs) {
        Visit(expr);
      }
      Exit();
    }

    protected override void Visit(SubscriptExpression subscript) {
      Enter(subscript);
      Visit(subscript.Expr);
      Visit(subscript.Index);
      Exit();
    }

    protected override void Visit(CallExpression call) {
      Enter(call);
      Visit(call.Func);
      foreach (Expression argument in call.Arguments) {
        Visit(argument);
      }
      Exit();
    }

    protected override void Visit(AssignmentStatement assignment) {
      Enter(assignment);
      for (int i = 0; i < assignment.Targets.Length; i++) {
        Visit(assignment.Targets[i]);
        _out.Append(i < assignment.Targets.Length - 1 ? ", " : " ");
      }
      for (int i = 0; i < assignment.Exprs.Length; i++) {
        Visit(assignment.Exprs[i]);
        _out.Append(i < assignment.Exprs.Length - 1 ? ", " : " ");
      }
      Exit();
    }

    protected override void Visit(BlockStatement block) {
      Enter(block);
      foreach (var statement in block.Statements) {
        Visit(statement);
      }
      Exit();
    }

    protected override void Visit(ExpressionStatement expr) {
      Enter(expr);
      Visit(expr.Expr);
      Exit();
    }

    protected override void Visit(FuncDefStatement funcDef) {
      Enter(funcDef);
      _out.Append($" ({funcDef.Name}:{string.Join(",", funcDef.Parameters)})");
      Visit(funcDef.Body);
      Exit();
    }

    protected override void Visit(IfStatement @if) {
      Enter(@if);
      Visit(@if.Test);
      Visit(@if.ThenBody);
      if (!(@if.ElseBody is null)) {
        Visit(@if.ElseBody);
      }
      Exit();
    }

    protected override void Visit(ReturnStatement @return) {
      Enter(@return);
      Visit(@return.Result);
      Exit();
    }

    protected override void Visit(WhileStatement @while) {
      Enter(@while);
      Visit(@while.Test);
      Visit(@while.Body);
      Exit();
    }

    private void Enter(AstNode node) {
      if (_level > 0) {
        _out.Append($"\n{new string(' ', _level * 2)}");
      }
      _out.Append($"{node.Range} {node.GetType().Name}");
      _level++;
    }

    private void Exit() {
      _level--;
    }
  }
}
