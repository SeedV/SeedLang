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
using System.Text;
using SeedLang.Common;
using SeedLang.Runtime;

namespace SeedLang.Visualization {

  public abstract class AbstractEvent {
    // The range of source code.
    public TextRange Range { get; }

    public AbstractEvent(TextRange range) {
      Range = range;
    }
  }

  public static class Event {
    // An event which is triggered when a value is assigned to a variable or an element of a
    // container.
    public class Assignment : AbstractEvent {
      public Variable Variable { get; }
      public Value Value { get; }

      public Assignment(Variable variable, Value value, TextRange range) : base(range) {
        Variable = variable;
        Value = value;
      }

      public override string ToString() {
        var sb = new StringBuilder();
        sb.Append($"{Range} ");
        if (Variable.IsElementOfContainer) {
          sb.Append('(');
        }
        sb.Append($"{Variable.Name}: {Variable.Type}");
        if (Variable.IsElementOfContainer) {
          var keys = string.Join("][", Variable.Keys);
          sb.Append($")[{keys}]");
        }
        sb.Append($" = {Value}");
        return sb.ToString();
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

      public override string ToString() {
        return $"{Range} {Left} {Op} {Right} = {Result}";
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

      public override string ToString() {
        var sb = new StringBuilder();
        sb.Append($"{Range} {First} ");
        for (int i = 0; i < Ops.Count; ++i) {
          sb.Append($"{Ops[i]} {Values[i]} ");
        }
        sb.Append($"= {Result}");
        return sb.ToString();
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

      public override string ToString() {
        return $"{Range} FuncCalled: {Name}({string.Join(", ", Args)})";
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

      public override string ToString() {
        return $"{Range} FuncReturned: {Name} {Result}";
      }
    }

    // An event which is triggered when each statement is starting to execute.
    public class SingleStep : AbstractEvent {
      public SingleStep(TextRange range) : base(range) { }

      public override string ToString() {
        return $"{Range} SingleStep";
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

      public override string ToString() {
        return $"{Range} {Op} {Value} = {Result}";
      }
    }

    // An event which is triggered when a variable is defined.
    public class VariableDefined : AbstractEvent {
      public string Name { get; }
      public VariableType Type { get; }

      public VariableDefined(string name, VariableType type, TextRange range) : base(range) {
        Name = name;
        Type = type;
      }

      public override string ToString() {
        return $"{Range} VariableDefined: {Name}: {Type}";
      }
    }

    // An event which is triggered when a variable is deleted.
    public class VariableDeleted : AbstractEvent {
      public string Name { get; }
      public VariableType Type { get; }

      public VariableDeleted(string name, VariableType type, TextRange range) : base(range) {
        Name = name;
        Type = type;
      }

      public override string ToString() {
        return $"{Range} VariableDeleted: {Name}: {Type}";
      }
    }

    // An event which is triggered when a VTag scope is entered.
    public class VTagEntered : AbstractEvent {
      public IReadOnlyList<VTagInfo> VTags { get; }

      public VTagEntered(IReadOnlyList<VTagInfo> vTags, TextRange range) : base(range) {
        VTags = vTags;
      }

      public override string ToString() {
        return $"{Range} VTagEntered: {string.Join(", ", VTags)}";
      }
    }

    // An event which is triggered when a VTag scope is exited.
    public class VTagExited : AbstractEvent {
      public IReadOnlyList<VTagInfo> VTags { get; }

      public VTagExited(IReadOnlyList<VTagInfo> vTags, TextRange range) : base(range) {
        VTags = vTags;
      }

      public override string ToString() {
        return $"{Range} VTagExited: {string.Join(", ", VTags)}";
      }
    }
  }
}
