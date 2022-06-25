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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SeedLang.Visualization {
  public enum VariableType {
    Global,
    Local,
    Upvalue,
  }

  // The LValue class. It could be a variable or an element of containers. The Keys field is null if
  // it's a variable.
  public class LValue {
    public bool IsElement => !IsVariable;
    public bool IsVariable => Keys is null;

    public Variable Variable { get; }
    public IReadOnlyList<Value> Keys { get; }

    public LValue(Variable variable, IReadOnlyList<Value> keys) {
      Variable = variable;
      Keys = keys;
    }

    public LValue(Variable variable) {
      Variable = variable;
      Keys = null;
    }

    public override string ToString() {
      return IsElement ? $"{Variable}[{string.Join("][", Keys)}]" : $"{Variable}";
    }
  }

  // The RValue class. It could be a variable, an element of containers or a temporary value. The
  // Keys field is null if it's a variable. The Variable and Keys fields are null if it's a
  // temporary value.
  public class RValue {
    public bool IsElement => !(Variable is null) && !(Keys is null);
    public bool IsTemporary => Variable is null && Keys is null;
    public bool IsVariable => !(Variable is null) && Keys is null;

    public Variable Variable { get; }
    public IReadOnlyList<Value> Keys { get; }
    public Value Value { get; }

    public RValue(Variable variable, IReadOnlyList<Value> keys, Value value) {
      Variable = variable;
      Keys = keys;
      Value = value;
    }

    public RValue(Value value) {
      Variable = null;
      Keys = null;
      Value = value;
    }

    public RValue(Variable variable, Value value) {
      Variable = variable;
      Keys = null;
      Value = value;
    }

    public override string ToString() {
      return IsElement ? $"{Variable}[{string.Join("][", Keys)}] {Value}" :
                         (IsTemporary ? $"{Value}" : $"{Variable} {Value}");
    }
  }

  public class Variable {
    public string Name { get; }
    public VariableType Type { get; }

    public Variable(string name, VariableType type) {
      Name = name;
      Type = type;
    }

    public override string ToString() {
      return $"{Name}:{Type}";
    }
  }

  public class VTagInfo {
    public string Name { get; }
    public IReadOnlyList<string> Args { get; }
    public IReadOnlyList<Value> Values { get; }

    public VTagInfo(string name, IReadOnlyList<string> args, IReadOnlyList<Value> values) {
      Debug.Assert(args.Count == values.Count);
      Name = name;
      Args = args;
      Values = values;
    }

    public override string ToString() {
      var sb = new StringBuilder();
      sb.Append(Name);
      sb.Append('(');
      for (int i = 0; i < Args.Count; i++) {
        sb.Append(i > 0 ? ", " : "");
        sb.Append($"{Args[i]}: {Values[i]}");
      }
      sb.Append(')');
      return sb.ToString();
    }
  }
}
