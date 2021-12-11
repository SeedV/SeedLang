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

using System.Collections.Generic;
using System.Diagnostics;
using SeedLang.Common;

namespace SeedLang.Runtime {
  public abstract class AbstractEvent {
    // The range of source code.
    public Range Range { get; }

    public AbstractEvent(Range range) {
      Range = range;
    }
  }

  // An event which is triggered when a binary expression is evaluated.
  public class BinaryEvent : AbstractEvent {
    public IValue Left { get; }
    public BinaryOperator Op { get; }
    public IValue Right { get; }
    public IValue Result { get; }

    public BinaryEvent(IValue left, BinaryOperator op, IValue right, IValue result, Range range) :
        base(range) {
      Left = left;
      Op = op;
      Right = right;
      Result = result;
    }
  }

  // An event which is triggered when a boolean expression is evaluated.
  //
  // Not all the expressions are evaluated due to short circuit. The values without evaluated are
  // filled as null.
  public class BooleanEvent : AbstractEvent {
    public BooleanOperator Op { get; }
    public IReadOnlyList<IValue> Values { get; }
    public IValue Result { get; }

    public BooleanEvent(BooleanOperator op, IReadOnlyList<IValue> values, IValue result,
                        Range range) : base(range) {
      Debug.Assert(values.Count > 1);
      Op = op;
      Values = values;
      Result = result;
    }
  }

  // An event which is triggered when a comparison expression is evaluated.
  //
  // The count of values is as same as Ops. But not all the expressions are evaluated due to short
  // circuit. The values without evaluated are filled as null.
  public class ComparisonEvent : AbstractEvent {
    public IValue First { get; }
    public IReadOnlyList<ComparisonOperator> Ops { get; }
    public IReadOnlyList<IValue> Values { get; }
    public IValue Result { get; }

    public ComparisonEvent(IValue first, IReadOnlyList<ComparisonOperator> ops,
                           IReadOnlyList<IValue> values, IValue result, Range range) : base(range) {
      Debug.Assert(ops.Count > 0 && ops.Count == values.Count);
      First = first;
      Values = values;
      Ops = ops;
      Result = result;
    }
  }

  // An event which is triggered when an unary expression is executed.
  public class UnaryEvent : AbstractEvent {
    public UnaryOperator Op { get; }
    public IValue Value { get; }
    public IValue Result { get; }

    public UnaryEvent(UnaryOperator op, IValue value, IValue result, Range range) :
        base(range) {
      Op = op;
      Value = value;
      Result = result;
    }
  }

  // An event which is triggered when an assignment statement is executed.
  public class AssignmentEvent : AbstractEvent {
    public string Identifier { get; }
    public IValue Value { get; }

    public AssignmentEvent(string identifier, IValue value, Range range) : base(range) {
      Identifier = identifier;
      Value = value;
    }
  }

  // An event which is triggered when an expression statement is executed.
  public class EvalEvent : AbstractEvent {
    public IValue Value { get; }

    public EvalEvent(IValue value, Range range) : base(range) {
      Value = value;
    }
  }
}
