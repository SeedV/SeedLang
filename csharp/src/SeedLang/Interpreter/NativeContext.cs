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

using System.IO;
using SeedLang.Runtime;

namespace SeedLang.Interpreter {
  // The class to provide context information to native functions.
  internal class NativeContext : INativeContext {
    public TextWriter Stdout => _vm.Stdout;

    private readonly VM _vm;

    internal NativeContext(VM vm) {
      _vm = vm;
    }

    public VMValue ModuleDir() {
      return _vm.ModuleDir();
    }

    public VMValue ModuleDir(VMValue value) {
      return VM.ModuleDir(value);
    }
  }
}
