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

namespace SeedLang.Runtime {
  // The interface to provide context information for native functions.
  internal interface INativeContext {
    // The standard output that is used for print() and expression statements.
    TextWriter Stdout { get; }

    // Returns the names in current module namespace.
    VMValue Dir();

    // Returns the names in the module namespace, if the given value is a module object. Otherwise
    // returns an empty list.
    VMValue Dir(VMValue value);
  }
}
