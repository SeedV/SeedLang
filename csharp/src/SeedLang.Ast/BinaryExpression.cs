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

namespace SeedLang.Ast {
  public enum BinaryOperator {
    Add,
    Substract,
    Multiply,
    Divide
  }

  public static class BinaryOperatorExtensions {
    public static string Symbol(this BinaryOperator op) {
      switch (op) {
        case BinaryOperator.Add:
          return "+";
        case BinaryOperator.Substract:
          return "-";
        case BinaryOperator.Multiply:
          return "*";
        case BinaryOperator.Divide:
          return "/";
        default:
          throw new ArgumentException("Unsupported binary operator.");
      }
    }
  }

  public sealed class BinaryExpression : Expression {
    public Expression Left { get; set; }
    public BinaryOperator Op { get; set; }
    public Expression Right { get; set; }

    internal BinaryExpression(Expression left, BinaryOperator op, Expression right) {
      Left = left;
      Op = op;
      Right = right;
    }

    protected internal override void Accept(AstVisitor visitor) {
      visitor.VisitBinaryExpression(this);
    }
  }
}