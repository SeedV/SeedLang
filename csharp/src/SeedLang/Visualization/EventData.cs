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

namespace SeedLang.Visualization {
  public enum VariableType {
    Global,
    Local,
    Upvalue,
  }

  public class Operand {
    public bool IsVariable => !(Variable is null);

    public Variable Variable { get; }
    public Value Value { get; }

    public Operand(Variable variable, Value value) {
      Variable = variable;
      Value = value;
    }

    public Operand(Value value) {
      Variable = null;
      Value = value;
    }

    public override string ToString() {
      string variableString = IsVariable ? $"{Variable} " : "";
      return $"{variableString}{Value}";
    }
  }

  public class Variable {
    public bool IsElementOfContainer => Keys.Count > 0;

    public string Name { get; }
    public VariableType Type { get; }
    public IReadOnlyList<Value> Keys { get; }

    public Variable(string name, VariableType type, IReadOnlyList<Value> keys) {
      Name = name;
      Type = type;
      Keys = keys;
    }

    public override string ToString() {
      var sb = new StringBuilder();
      sb.Append($"{Name}:{Type}");
      if (IsElementOfContainer) {
        var keys = string.Join("][", Keys);
        sb.Append($"[{keys}]");
      }
      return sb.ToString();
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
