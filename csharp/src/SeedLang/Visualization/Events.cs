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
    // An event which is triggered when a value is assigned to a variable or an element of
    // containers.
    public class Assignment : AbstractEvent {
      public LValue Target { get; }
      public RValue Value { get; }

      public Assignment(LValue target, RValue value, TextRange range) : base(range) {
        Target = target;
        Value = value;
      }

      public override string ToString() {
        return $"{Range} {Target} = {Value}";
      }
    }

    // An event which is triggered when a binary expression is evaluated.
    public class Binary : AbstractEvent {
      public RValue Left { get; }
      public BinaryOperator Op { get; }
      public RValue Right { get; }
      public Value Result { get; }

      public Binary(RValue left, BinaryOperator op, RValue right, Value result, TextRange range) :
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
    public class Comparison : AbstractEvent {
      public RValue Left { get; }
      public ComparisonOperator Op { get; }
      public RValue Right { get; }
      public Value Result { get; }

      public Comparison(RValue left, ComparisonOperator op, RValue right, Value result,
                        TextRange range) : base(range) {
        Left = left;
        Op = op;
        Right = right;
        Result = result;
      }

      public override string ToString() {
        return $"{Range} {Left} {Op} {Right} = {Result}";
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

    // An event which is triggered when a variable is defined.
    public class VariableDefined : AbstractEvent {
      public string Name { get; }
      public VariableType Type { get; }

      public VariableDefined(string name, VariableType type, TextRange range) : base(range) {
        Name = name;
        Type = type;
      }

      public override string ToString() {
        return $"{Range} VariableDefined: {Name} ({Type})";
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
        return $"{Range} VariableDeleted: {Name} ({Type})";
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
