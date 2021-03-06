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
using System.Diagnostics;
using SeedLang.Common;
using SeedLang.Runtime;

namespace SeedLang.Ast {
  // The base class of all expression nodes.
  internal abstract class Expression : AstNode {
    // The factory method to create a binary expression.
    internal static BinaryExpression Binary(Expression left, BinaryOperator op, Expression right,
                                            TextRange range) {
      return new BinaryExpression(left, op, right, range);
    }

    // The factory method to create a boolean expression.
    internal static BooleanExpression Boolean(BooleanOperator op, Expression[] exprs,
                                              TextRange range) {
      return new BooleanExpression(op, exprs, range);
    }

    // The factory method to create a comparison expression.
    internal static ComparisonExpression Comparison(Expression first, ComparisonOperator[] ops,
                                                    Expression[] exprs, TextRange range) {
      return new ComparisonExpression(first, ops, exprs, range);
    }

    // The factory method to create a unary expression.
    internal static UnaryExpression Unary(UnaryOperator op, Expression expr, TextRange range) {
      return new UnaryExpression(op, expr, range);
    }

    // The factory method to create an identifier expression.
    internal static IdentifierExpression Identifier(string name, TextRange range) {
      return new IdentifierExpression(name, range);
    }

    // The factory method to create a boolean constant expression from a string.
    internal static BooleanConstantExpression BooleanConstant(bool value, TextRange range) {
      return new BooleanConstantExpression(value, range);
    }

    // The factory method to create a nil constant expression.
    internal static NilConstantExpression NilConstant(TextRange range) {
      return new NilConstantExpression(range);
    }

    // The factory method to create a number constant expression.
    internal static NumberConstantExpression NumberConstant(double value, TextRange range) {
      Debug.Assert(!double.IsInfinity(value) && !double.IsNaN(value));
      return new NumberConstantExpression(value, range);
    }

    // The factory method to create a string constant expression.
    internal static StringConstantExpression StringConstant(string value, TextRange range) {
      return new StringConstantExpression(ValueHelper.Unescape(value), range);
    }

    // The factory method to create a dictionary expression.
    internal static DictExpression Dict(KeyValuePair<Expression, Expression>[] keyValues,
                                        TextRange range) {
      return new DictExpression(keyValues, range);
    }

    // The factory method to create a list expression.
    internal static ListExpression List(Expression[] exprs, TextRange range) {
      return new ListExpression(exprs, range);
    }

    // The factory method to create a tuple expression.
    internal static TupleExpression Tuple(Expression[] exprs, TextRange range) {
      return new TupleExpression(exprs, range);
    }

    // The factory method to create a subscript expression.
    internal static SubscriptExpression Subscript(Expression container, Expression key,
                                                  TextRange range) {
      return new SubscriptExpression(container, key, range);
    }

    // The factory method to create a slice expression.
    internal static SliceExpression Slice(Expression start, Expression stop, Expression step,
                                          TextRange range) {
      return new SliceExpression(start, stop, step, range);
    }

    // The factory method to create a call expression.
    internal static CallExpression Call(Expression func, Expression[] arguments, TextRange range) {
      return new CallExpression(func, arguments, range);
    }

    // The factory method to create an attribute expression.
    internal static AttributeExpression Attribute(Expression expr, IdentifierExpression attr,
                                                  TextRange range) {
      return new AttributeExpression(expr, attr, range);
    }

    internal Expression(TextRange range) : base(range) { }
  }

  internal class BinaryExpression : Expression {
    public Expression Left { get; }
    public BinaryOperator Op { get; }
    public Expression Right { get; }

    internal BinaryExpression(Expression left, BinaryOperator op, Expression right,
                              TextRange range) : base(range) {
      Left = left;
      Op = op;
      Right = right;
    }
  }

  internal class BooleanExpression : Expression {
    public BooleanOperator Op { get; }
    public Expression[] Exprs { get; }

    internal BooleanExpression(BooleanOperator op, Expression[] exprs, TextRange range) :
        base(range) {
      Debug.Assert(exprs.Length > 1);
      Op = op;
      Exprs = exprs;
    }
  }

  internal class ComparisonExpression : Expression {
    public Expression First { get; }
    public ComparisonOperator[] Ops { get; }
    public Expression[] Exprs { get; }

    internal ComparisonExpression(Expression first, ComparisonOperator[] ops, Expression[] exprs,
                                  TextRange range) : base(range) {
      Debug.Assert(ops.Length > 0 && ops.Length == exprs.Length);
      First = first;
      Ops = ops;
      Exprs = exprs;
    }
  }

  internal class UnaryExpression : Expression {
    public UnaryOperator Op { get; }
    public Expression Expr { get; }

    internal UnaryExpression(UnaryOperator op, Expression expr, TextRange range) : base(range) {
      Op = op;
      Expr = expr;
    }
  }

  internal class IdentifierExpression : Expression {
    public string Name { get; }

    internal IdentifierExpression(string name, TextRange range) : base(range) {
      Name = name;
    }
  }

  internal class BooleanConstantExpression : Expression {
    public bool Value { get; }

    internal BooleanConstantExpression(bool value, TextRange range) : base(range) {
      Value = value;
    }
  }

  internal class NilConstantExpression : Expression {
    internal NilConstantExpression(TextRange range) : base(range) { }
  }

  internal class NumberConstantExpression : Expression {
    public double Value { get; }

    internal NumberConstantExpression(double value, TextRange range) : base(range) {
      Value = value;
    }
  }

  internal class StringConstantExpression : Expression {
    public string Value { get; }

    internal StringConstantExpression(string value, TextRange range) : base(range) {
      Value = value;
    }
  }

  internal class DictExpression : Expression {
    public KeyValuePair<Expression, Expression>[] KeyValues { get; }

    internal DictExpression(KeyValuePair<Expression, Expression>[] keyValues, TextRange range) :
        base(range) {
      KeyValues = keyValues;
    }
  }

  internal class ListExpression : Expression {
    public Expression[] Exprs { get; }

    internal ListExpression(Expression[] exprs, TextRange range) : base(range) {
      Exprs = exprs;
    }
  }

  internal class TupleExpression : Expression {
    public Expression[] Exprs { get; }

    internal TupleExpression(Expression[] exprs, TextRange range) : base(range) {
      Exprs = exprs;
    }
  }

  internal class SubscriptExpression : Expression {
    public Expression Container { get; }
    public Expression Key { get; }

    internal SubscriptExpression(Expression container, Expression key, TextRange range) :
        base(range) {
      Container = container;
      Key = key;
    }
  }

  internal class SliceExpression : Expression {
    // The start index of the slice. It can be null to indicate the beginning of the container.
    public Expression Start { get; }
    // The exclusive stop index of the slice. It can be null to indicate the ending of the
    // container.
    public Expression Stop { get; }
    // The step of the slice. It can be null to indicate the default 1-step.
    public Expression Step { get; }

    internal SliceExpression(Expression start, Expression stop, Expression step, TextRange range)
        : base(range) {
      Start = start;
      Stop = stop;
      Step = step;
    }
  }

  internal class CallExpression : Expression {
    public Expression Func { get; }
    public Expression[] Arguments { get; }

    internal CallExpression(Expression func, Expression[] arguments, TextRange range) :
        base(range) {
      Func = func;
      Arguments = arguments;
    }
  }

  internal class AttributeExpression : Expression {
    public Expression Value { get; }
    public IdentifierExpression Attr { get; }

    internal AttributeExpression(Expression value, IdentifierExpression attr, TextRange range) :
        base(range) {
      Value = value;
      Attr = attr;
    }
  }
}
