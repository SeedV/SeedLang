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

namespace SeedLang.Runtime {
  public enum VariableType {
    Global,
    Local,
  }

  public abstract class AbstractEvent {
    // The range of source code.
    public Range Range { get; }

    public AbstractEvent(Range range) {
      Range = range;
    }
  }

  public static class Event {
    // An event which is triggered when a binary expression is evaluated.
    public class Binary : AbstractEvent {
      public IValue Left { get; }
      public BinaryOperator Op { get; }
      public IValue Right { get; }
      public IValue Result { get; }

      public Binary(IValue left, BinaryOperator op, IValue right, IValue result, Range range) :
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
    public class Boolean : AbstractEvent {
      public BooleanOperator Op { get; }
      public IReadOnlyList<IValue> Values { get; }
      public IValue Result { get; }

      public Boolean(BooleanOperator op, IReadOnlyList<IValue> values, IValue result,
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
    public class Comparison : AbstractEvent {
      public IValue First { get; }
      public IReadOnlyList<ComparisonOperator> Ops { get; }
      public IReadOnlyList<IValue> Values { get; }
      public IValue Result { get; }

      public Comparison(IValue first, IReadOnlyList<ComparisonOperator> ops,
                             IReadOnlyList<IValue> values, IValue result, Range range) : base(range) {
        Debug.Assert(ops.Count > 0 && ops.Count == values.Count);
        First = first;
        Values = values;
        Ops = ops;
        Result = result;
      }
    }

    // An event which is triggered when an unary expression is executed.
    public class Unary : AbstractEvent {
      public UnaryOperator Op { get; }
      public IValue Value { get; }
      public IValue Result { get; }

      public Unary(UnaryOperator op, IValue value, IValue result, Range range) :
          base(range) {
        Op = op;
        Value = value;
        Result = result;
      }
    }

    // An event which is triggered when an assignment statement is executed.
    public class Assignment : AbstractEvent {
      public string Name { get; }
      public VariableType Type { get; }
      public IValue Value { get; }

      public Assignment(string name, VariableType type, IValue value, Range range) :
          base(range) {
        Name = name;
        Type = type;
        Value = value;
      }
    }

    // An event which is triggered when a VTag scope is entered.
    public class VTagEntered : AbstractEvent {
      public class VTagInfo {
        public string Name { get; }
        public IReadOnlyList<string> ArgTexts { get; }

        public VTagInfo(string name, IReadOnlyList<string> argTexts) {
          Name = name;
          ArgTexts = argTexts;
        }

        public override string ToString() {
          return $"{Name}({string.Join(",", ArgTexts)})";
        }
      }

      public IReadOnlyList<VTagInfo> VTags { get; }

      public VTagEntered(IReadOnlyList<VTagInfo> vTags, Range range) : base(range) {
        VTags = vTags;
      }
    }

    // An event which is triggered when a VTag scope is exited.
    public class VTagExited : AbstractEvent {
      public class VTagInfo {
        public string Name { get; }
        public IReadOnlyList<IValue> Args { get; }

        public VTagInfo(string name, IReadOnlyList<IValue> args) {
          Name = name;
          Args = args;
        }

        public override string ToString() {
          return $"{Name}({string.Join(",", Args)})";
        }
      }

      public IReadOnlyList<VTagInfo> VTags { get; }

      public VTagExited(IReadOnlyList<VTagInfo> vTags, Range range) : base(range) {
        VTags = vTags;
      }
    }
  }
}
