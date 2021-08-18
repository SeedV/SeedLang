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
using SeedLang.Common;

namespace SeedLang {
  // The interface of the SeedLang engine.
  public interface IEngine {
    // Registers a visualizer into the visualizer center of the engine.
    //
    // The visualizer can implement one or more IVisualizer<Event> interfaces to visualize SeedLang
    // execution events.
    void Register<Visualizer>(Visualizer visualizer);

    // Unregisters a visualizer from the visualizer center of the engine.
    void Unregister<Visualizer>(Visualizer visualizer);

    // Runs source code with the specified module name, programming language, target parsing rule
    // and runing type.
    //
    // There are three methods to run the source code.
    // 1) Dryrun: parses and validates the source code without runing.
    // 2) Ast: parses the source code into an AST tree, and runs it by traversing the AST tree.
    // 3) Bytecode: parses and compiles the source code into bytecode, and runs it in a VM.
    //
    // The parsing and running diagnostic information will be collected into the diagnostic
    // collection.
    bool Run(string source, string module, ProgrammingLanguage language, ParseRule rule,
             RunType runType, DiagnosticCollection collection = null);
  }
}
