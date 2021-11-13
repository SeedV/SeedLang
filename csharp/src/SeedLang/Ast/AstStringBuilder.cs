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
        case BinaryOperator.Modulus:
          return "%";
        default:
          throw new NotImplementedException($"Unsupported binary operator: {op}.");
      }
    }

    // Returns the internal string representation of compare operators.
    internal static string Symbol(this CompareOperator op) {
      switch (op) {
        case CompareOperator.Less:
          return "<";
        case CompareOperator.Great:
          return ">";
        case CompareOperator.LessEqual:
          return "<=";
        case CompareOperator.GreatEqual:
          return ">=";
        case CompareOperator.EqualEqual:
          return "==";
        case CompareOperator.NotEqual:
          return "!=";
        default:
          throw new NotImplementedException($"Unsupported binary operator: {op}.");
      }
    }

    // Returns the internal string representation of unary operators.
    internal static string Symbol(this UnaryOperator op) {
      switch (op) {
        case UnaryOperator.Negative:
          return "-";
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

    protected override void Visit(CompareExpression compare) {
      Enter(compare);
      // Ops of the compare expression have at least one items, and the length of Exprs is exact one
      // more than Ops. It is enforced in the constructor of compare expressions.
      Visit(compare.Exprs[0]);
      for (int i = 0; i < compare.Ops.Length; ++i) {
        _out.Append($" ({compare.Ops[i].Symbol()})");
        Visit(compare.Exprs[i + 1]);
      }
      Exit();
    }

    protected override void Visit(IdentifierExpression identifier) {
      Enter(identifier);
      _out.Append($" ({identifier.Name})");
      Exit();
    }

    protected override void Visit(NumberConstantExpression number) {
      Enter(number);
      _out.Append($" ({number.Value})");
      Exit();
    }

    protected override void Visit(StringConstantExpression str) {
      Enter(str);
      _out.Append($" ({str.Value})");
      Exit();
    }

    protected override void Visit(UnaryExpression unary) {
      Enter(unary);
      _out.Append($" ({unary.Op.Symbol()})");
      Visit(unary.Expr);
      Exit();
    }

    protected override void Visit(AssignmentStatement assignment) {
      Enter(assignment);
      Visit(assignment.Identifier);
      Visit(assignment.Expr);
      Exit();
    }

    protected override void Visit(ExpressionStatement eval) {
      Enter(eval);
      Visit(eval.Expr);
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
