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

  // An event which is triggered when a compare expression is evaluated.
  public class CompareEvent : AbstractEvent {
    public IValue First { get; }
    public CompareOperator[] Ops { get; }
    public IValue[] Exprs { get; }
    public IValue Result { get; }

    public CompareEvent(IValue first, CompareOperator[] ops, IValue[] exprs, IValue result,
                        Range range) : base(range) {
      Debug.Assert(ops.Length > 0 && ops.Length == exprs.Length);
      First = first;
      Exprs = exprs;
      Ops = ops;
      Result = result;
    }
  }

  // An event which is triggered when an assignment statement is executed.
  public class AssignmentEvent {
    public string Identifier { get; }
    public IValue Value { get; }
    // The source code range of the assignment statement.
    public Range Range { get; }

    public AssignmentEvent(string identifier, IValue value, Range range) {
      Identifier = identifier;
      Value = value;
      Range = range;
    }
  }

  // An event which is triggered when an expression statement is executed.
  public class EvalEvent {
    public IValue Value { get; }
    // The source code range of the eval statement.
    public Range Range { get; }

    public EvalEvent(IValue value, Range range) {
      Value = value;
      Range = range;
    }
  }
}
