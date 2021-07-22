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
using System.Diagnostics;
using System.Text;

namespace SeedLang.Ast {
  internal static class BinaryOperatorExtensions {
    // Returns the internal string representation of the binary operator.
    internal static string Symbol(this BinaryOperator op) {
      switch (op) {
        case BinaryOperator.Add:
          return "+";
        case BinaryOperator.Substract:
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
          throw new ArgumentException("Unsupported binary operator.");
      }
    }
  }

  internal sealed class AstStringBuilder : AstVisitor {
    private readonly StringBuilder _out = new StringBuilder();

    public override string ToString() {
      return _out.ToString();
    }

    // Outputs a given AST tree to a string.
    internal static string AstToString(AstNode node) {
      Debug.Assert(node != null);
      var asb = new AstStringBuilder();
      asb.Visit(node);
      return asb.ToString();
    }

    protected internal override void VisitBinaryExpression(BinaryExpression binary) {
      _out.Append($"({binary.Left} {binary.Op.Symbol()} {binary.Right})");
    }

    protected internal override void VisitNumberConstant(NumberConstantExpression number) {
      _out.Append(number.Value);
    }

    protected internal override void VisitStringConstant(StringConstantExpression str) {
      _out.Append(str.Value);
    }

    protected internal override void VisitEvalStatement(EvalStatement eval) {
      _out.Append($"eval {eval.Expr}\n");
    }
  }
}
