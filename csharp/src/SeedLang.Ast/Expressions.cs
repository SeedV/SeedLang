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

using SeedLang.Runtime;

namespace SeedLang.Ast {
  // The base class of all expression nodes.
  public abstract class Expression : AstNode {
    // The factory method to create the binary expression.
    public static BinaryExpression Binary(Expression left, BinaryOperator op, Expression right) {
      return new BinaryExpression(left, op, right);
    }

    // The factory method to create the number constant expression.
    public static NumberConstantExpression Number(double value) {
      return new NumberConstantExpression(value);
    }

    // The factory method to create the string constant expression.
    public static StringConstantExpression String(string value) {
      return new StringConstantExpression(value);
    }
  }

  public class BinaryExpression : Expression {
    public Expression Left { get; set; }
    public BinaryOperator Op { get; set; }
    public Expression Right { get; set; }

    internal BinaryExpression(Expression left, BinaryOperator op, Expression right) {
      Left = left;
      Op = op;
      Right = right;
    }
  }

  public class NumberConstantExpression : Expression {
    public double Value { get; set; }

    internal NumberConstantExpression(double value) {
      Value = value;
    }
  }

  public class StringConstantExpression : Expression {
    public string Value { get; set; }

    internal StringConstantExpression(string value) {
      Value = value;
    }
  }
}
