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

namespace SeedLang.Visualization {
  // The type of variables.
  public enum VariableType {
    Global,
    Local,
  }

  public abstract class AbstractEvent {
    // The range of source code.
    public TextRange Range { get; }

    public AbstractEvent(TextRange range) {
      Range = range;
    }
  }

  public static class Event {
    // An event which is triggered when a value is assigned to a variable.
    public class Assignment : AbstractEvent {
      // The name of the assigned variable.
      public string Name { get; }
      // The type of the assigned variable.
      public VariableType Type { get; }
      public Value Value { get; }

      public Assignment(string name, VariableType type, Value value, TextRange range) :
          base(range) {
        Name = name;
        Type = type;
        Value = value;
      }
    }

    // An event which is triggered when a binary expression is evaluated.
    public class Binary : AbstractEvent {
      public Value Left { get; }
      public BinaryOperator Op { get; }
      public Value Right { get; }
      public Value Result { get; }

      public Binary(Value left, BinaryOperator op, Value right, Value result, TextRange range) :
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
      public IReadOnlyList<Value> Values { get; }
      public Value Result { get; }

      public Boolean(BooleanOperator op, IReadOnlyList<Value> values, Value result, TextRange range)
          : base(range) {
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
      public Value First { get; }
      public IReadOnlyList<ComparisonOperator> Ops { get; }
      public IReadOnlyList<Value> Values { get; }
      public Value Result { get; }

      public Comparison(Value first, IReadOnlyList<ComparisonOperator> ops,
                        IReadOnlyList<Value> values, Value result, TextRange range) : base(range) {
        Debug.Assert(ops.Count > 0 && ops.Count == values.Count);
        First = first;
        Values = values;
        Ops = ops;
        Result = result;
      }
    }

    // An event which is triggered when a function is called.
    public class FuncCalled : AbstractEvent {
      public string Name { get; }
      public IReadOnlyList<Value> Args { get; }

      public FuncCalled(string name, IReadOnlyList<Value> args, TextRange range) : base(range) {
        Name = name;
        Args = args;
      }
    }

    // An event which is triggered when a function is returned.
    public class FuncReturned : AbstractEvent {
      public string Name { get; }
      public Value Result { get; }

      public FuncReturned(string name, Value result, TextRange range) : base(range) {
        Name = name;
        Result = result;
      }
    }

    // An event which is triggered when each statement is starting to execute.
    public class SingleStep : AbstractEvent {
      public SingleStep(TextRange range) : base(range) { }
    }

    // An event which is triggered when a value is assigned to an element of a container.
    //
    // The container might be unnamed variables in following cases. The name of the container is
    // null and the type is undefined in these cases.
    // 1) Sets the element of a temporary container: [1, 2, 3][1] = 5
    // 2) Sets the element of a intermediate container: a[1][2] = 5
    public class SubscriptAssignment : AbstractEvent {
      public Value Container { get; }
      // The variable name of the container.
      public string Name { get; }
      // The variable type of the container.
      public VariableType Type { get; }
      public Value Key { get; }
      public Value Value { get; }

      public SubscriptAssignment(Value container, string name, VariableType type, Value key,
                                 Value value, TextRange range) : base(range) {
        Container = container;
        Name = name;
        Type = type;
        Key = key;
        Value = value;
      }
    }

    // An event which is triggered when an unary expression is executed.
    public class Unary : AbstractEvent {
      public UnaryOperator Op { get; }
      public Value Value { get; }
      public Value Result { get; }

      public Unary(UnaryOperator op, Value value, Value result, TextRange range) : base(range) {
        Op = op;
        Value = value;
        Result = result;
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
          return Name + (ArgTexts.Count > 0 ? $"({string.Join(",", ArgTexts)})" : "");
        }
      }

      public IReadOnlyList<VTagInfo> VTags { get; }

      public VTagEntered(IReadOnlyList<VTagInfo> vTags, TextRange range) : base(range) {
        VTags = vTags;
      }
    }

    // An event which is triggered when a VTag scope is exited.
    public class VTagExited : AbstractEvent {
      public class VTagInfo {
        public string Name { get; }
        public IReadOnlyList<Value> Args { get; }

        public VTagInfo(string name, IReadOnlyList<Value> args) {
          Name = name;
          Args = args;
        }

        public override string ToString() {
          return Name + (Args.Count > 0 ? $"({string.Join(",", Args)})" : "");
        }
      }

      public IReadOnlyList<VTagInfo> VTags { get; }

      public VTagExited(IReadOnlyList<VTagInfo> vTags, TextRange range) : base(range) {
        VTags = vTags;
      }
    }
  }
}
