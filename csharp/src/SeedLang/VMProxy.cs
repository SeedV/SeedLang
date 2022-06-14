using System.Collections.Immutable;
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
using SeedLang.Ast;
using SeedLang.Common;
using SeedLang.Interpreter;
using SeedLang.Runtime;
using SeedLang.Visualization;
using SeedLang.X;

namespace SeedLang {
  // The proxy class of SeedVM to implement IVM interface.
  internal class VMProxy : IVMProxy {
    // The environment class to extract global and local variables information from SeedVM and
    // provide to statement and expression executors.
    private class Environment : IEnvironment {
      private readonly IImmutableDictionary<string, VMValue> _globals;
      private readonly IImmutableDictionary<string, VMValue> _locals;

      internal Environment(IReadOnlyList<IVM.VariableInfo> globals,
                           IReadOnlyList<IVM.VariableInfo> locals) {
        _globals = globals.ToImmutableDictionary(info => info.Name,
                                                 info => info.Value.GetRawValue());
        _locals = locals.ToImmutableDictionary(info => info.Name, info => info.Value.GetRawValue());
      }

      public bool TryGetValueOfVariable(string name, out VMValue value) {
        if (_locals.TryGetValue(name, out value) || _globals.TryGetValue(name, out value)) {
          return true;
        }
        return false;
      }
    }

    private readonly SeedXLanguage _language;
    private VM _vm;

    internal VMProxy(SeedXLanguage language, VM vm) {
      _language = language;
      _vm = vm;
    }

    public bool GetGlobals(out IReadOnlyList<IVM.VariableInfo> globals) {
      if (_vm is null) {
        globals = new List<IVM.VariableInfo>();
        return false;
      }
      return _vm.GetGlobals(out globals);
    }

    public bool GetLocals(out IReadOnlyList<IVM.VariableInfo> locals) {
      if (_vm is null) {
        locals = new List<IVM.VariableInfo>();
        return false;
      }
      return _vm.GetLocals(out locals);
    }

    public void Pause() {
      _vm?.Pause();
    }

    public void Stop() {
      _vm?.Stop();
    }

    public bool Eval(string source, out Value result, DiagnosticCollection collection = null) {
      if (string.IsNullOrEmpty(source)) {
        result = default;
        return false;
      }
      BaseParser parser = Engine.MakeParser(_language);
      if (parser.Parse(source, "", collection ?? new DiagnosticCollection(),
                       out Statement statement, out _)) {
        try {
          if (GetGlobals(out IReadOnlyList<IVM.VariableInfo> globals) &&
              GetLocals(out IReadOnlyList<IVM.VariableInfo> locals)) {
            var executorResult = new ExpressionExecutor.Result();
            var env = new Environment(globals, locals);
            new StatementExecutor(env).Visit(statement, executorResult);
            result = new Value(executorResult.Value);
            return true;
          }
        } catch (DiagnosticException ex) {
          collection?.Report(ex.Diagnostic);
        }
      }
      result = default;
      return false;
    }

    public void Invalid() {
      _vm = null;
    }
  }
}
