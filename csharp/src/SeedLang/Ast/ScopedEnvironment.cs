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
  // The environment to store scoped variables. A new frame is pushed into the stack when a new
  // scope is entered.
  //
  // TODO: ScopedEnvironment might not be a good name for this class, choose a proper name for it.
  internal class ScopedEnvironment {
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

    private readonly Stack<Frame> _frames = new Stack<Frame>();

    internal ScopedEnvironment() {
      // Enters the first (global) scope.
      EnterScope();
    }

    internal void EnterScope() {
      _frames.Push(new Frame());
    }

    internal void ExitScope() {
      _frames.Pop();
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
      _frames.Peek().SetVariable(name, value);
    }
  }
}
