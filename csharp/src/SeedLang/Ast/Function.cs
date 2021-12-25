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

using SeedLang.Runtime;

namespace SeedLang.Ast {
  // A function value type that is only used in SeedAst component to encapsulate AST function
  // declearation statement.
  internal class Function : IFunction {
    private readonly FunctionStatement _func;
    private readonly Executor _executor;

    internal Function(FunctionStatement func, Executor executor) {
      _func = func;
      _executor = executor;
    }

    public Value Call(Value[] arguments) {
      return _executor.Call(_func, arguments);
    }
  }
}
