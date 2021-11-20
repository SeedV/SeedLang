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

using System.Diagnostics;
using SeedLang.Common;
using SeedLang.Runtime;

namespace SeedLang.Ast {
  // The base class of all expression nodes.
  internal abstract class Expression : AstNode {
    // The factory method to create a binary expression.
    internal static BinaryExpression Binary(Expression left, BinaryOperator op, Expression right,
                                            Range range) {
      return new BinaryExpression(left, op, right, range);
    }

    // The factory method to create a boolean expression.
    internal static BooleanExpression Boolean(BooleanOperator op, Expression[] exprs, Range range) {
      return new BooleanExpression(op, exprs, range);
    }

    // The factory method to create a comparison expression.
    internal static ComparisonExpression Comparison(Expression first, ComparisonOperator[] ops,
                                                    Expression[] exprs, Range range) {
      return new ComparisonExpression(first, ops, exprs, range);
    }

    // The factory method to create a unary expression.
    internal static UnaryExpression Unary(UnaryOperator op, Expression expr, Range range) {
      return new UnaryExpression(op, expr, range);
    }

    // The factory method to create an identifier expression.
    internal static IdentifierExpression Identifier(string name, Range range) {
      return new IdentifierExpression(name, range);
    }

    // The factory method to create a boolean constant expression from a string.
    internal static BooleanConstantExpression BooleanConstant(bool value, Range range) {
      return new BooleanConstantExpression(value, range);
    }

    // The factory method to create a none constant expression from a string.
    internal static NoneConstantExpression NoneConstant(Range range) {
      return new NoneConstantExpression(range);
    }

    // The factory method to create a number constant expression.
    internal static NumberConstantExpression NumberConstant(double value, Range range) {
      // Throws a runtime overflow exception if the constant number is overflowed.
      ValueHelper.CheckOverflow(value, range);
      return new NumberConstantExpression(value, range);
    }

    // The factory method to create a number constant expression from a string.
    internal static NumberConstantExpression NumberConstant(string value, Range range) {
      try {
        return NumberConstant(double.Parse(value), range);
      } catch (System.Exception) {
        return NumberConstant(0, range);
      }
    }

    // The factory method to create a string constant expression.
    internal static StringConstantExpression StringConstant(string value, Range range) {
      return new StringConstantExpression(value, range);
    }

    internal Expression(Range range) : base(range) {
    }
  }

  internal class BinaryExpression : Expression {
    public Expression Left { get; }
    public BinaryOperator Op { get; }
    public Expression Right { get; }

    internal BinaryExpression(Expression left, BinaryOperator op, Expression right,
                              Range range) : base(range) {
      Left = left;
      Op = op;
      Right = right;
    }
  }

  internal class BooleanExpression : Expression {
    public BooleanOperator Op { get; }
    public Expression[] Exprs { get; }

    internal BooleanExpression(BooleanOperator op, Expression[] exprs, Range range) : base(range) {
      Op = op;
      Exprs = exprs;
    }
  }

  internal class ComparisonExpression : Expression {
    public Expression First { get; }
    public ComparisonOperator[] Ops { get; }
    public Expression[] Exprs { get; }

    internal ComparisonExpression(Expression first, ComparisonOperator[] ops, Expression[] exprs,
                                  Range range) : base(range) {
      Debug.Assert(ops.Length > 0 && ops.Length == exprs.Length);
      First = first;
      Ops = ops;
      Exprs = exprs;
    }
  }

  internal class UnaryExpression : Expression {
    public UnaryOperator Op { get; }
    public Expression Expr { get; }

    internal UnaryExpression(UnaryOperator op, Expression expr, Range range) : base(range) {
      Op = op;
      Expr = expr;
    }
  }

  internal class IdentifierExpression : Expression {
    public string Name { get; }

    internal IdentifierExpression(string name, Range range) : base(range) {
      Name = name;
    }
  }

  internal class BooleanConstantExpression : Expression {
    public bool Value { get; }

    internal BooleanConstantExpression(bool value, Range range) : base(range) {
      Value = value;
    }
  }

  internal class NoneConstantExpression : Expression {
    internal NoneConstantExpression(Range range) : base(range) {
    }
  }

  internal class NumberConstantExpression : Expression {
    public double Value { get; }

    internal NumberConstantExpression(double value, Range range) : base(range) {
      Value = value;
    }
  }

  internal class StringConstantExpression : Expression {
    public string Value { get; }

    internal StringConstantExpression(string value, Range range) : base(range) {
      Value = value;
    }
  }
}
