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
using SeedLang.Runtime;

namespace SeedLang.Ast {
  internal class Environment {
    private class Frame {
      private readonly Dictionary<string, Value> _variables = new Dictionary<string, Value>();

      internal bool ContainsVariable(string name) {
        return _variables.ContainsKey(name);
      }

      internal Value GetVariable(string name) {
        Debug.Assert(_variables.ContainsKey(name));
        return _variables[name];
      }

      internal void SetVariable(string name, in Value value) {
        _variables[name] = value;
      }
    }

    private readonly LinkedList<Frame> _frames = new LinkedList<Frame>();

    internal Environment() {
      // Enters the global scope.
      EnterScope();
    }

    internal void EnterScope() {
      _frames.AddFirst(new Frame());
    }

    internal void ExitScope() {
      _frames.RemoveFirst();
    }

    internal bool ContainsVariable(string name) {
      foreach (var frame in _frames) {
        if (frame.ContainsVariable(name)) {
          return true;
        }
      }
      return false;
    }

    internal Value GetVariable(string name) {
      foreach (var frame in _frames) {
        if (frame.ContainsVariable(name)) {
          return frame.GetVariable(name);
        }
      }
      throw new DiagnosticException(SystemReporters.SeedAst, Severity.Fatal, "", null,
                                    Message.RuntimeErrorVariableNotDefined);
    }

    internal void SetVariable(string name, in Value value) {
      _frames.First.Value.SetVariable(name, value);
    }
  }
}
