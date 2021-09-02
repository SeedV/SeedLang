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

namespace SeedLang.Runtime {
  // The running type of SeedBlock and SeedX source code.
  public enum RunType {
    // Parses the source code into an AST tree, and runs it by traversing the AST tree.
    Ast,
    // Parses and compiles the source code into bytecode, and runs it in a VM.
    Bytecode,
  }
}
