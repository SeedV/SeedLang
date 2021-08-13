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

using System;
using System.Collections.Generic;
using SeedLang.Common;

namespace SeedLang.Block {
  // The in-memory representation of a SeedBlock program.
  //
  // A program is a set of SeedBlock modules. Each module can be serialized to a JSON string/file in
  // the Block Exchange Format (BXF). All the modules of a program can be serialized to a set of BXF
  // JSON files then wrapped into a ZIP package.
  //
  // This is a singleton class. There can be only one program instance in the memory.
  public sealed class Program {
    private static readonly Lazy<Program> _lazyInstance = new Lazy<Program>(() => new Program());

    private readonly List<Module> _modules = new List<Module>();

    // A readonly interface for the client to access the contents of the program.
    public IReadOnlyList<Module> Modules => _modules;

    public static Program Instance => _lazyInstance.Value;

    private Program() {
    }

    // Adds a new module.
    public void Add(Module module) {
      _modules.Add(module);
    }

    // Clears the in-memory program.
    public void Clear() {
      _modules.Clear();
    }

    // Parses a module string in the BXF format and loads the module to the program.
    public void LoadModuleFromString(string bxfJson) {
      var diagnosticCollection = new DiagnosticCollection();
      var module = BxfReader.ReadFromString(bxfJson, diagnosticCollection);
      if (module == null) {
        // TODO: Deal with error messages.
      } else {
        _modules.Add(module);
      }
    }

    // Parses a module file in the BXF format and loads the module to the program.
    public void LoadModuleFromFile(string path) {
      var diagnosticCollection = new DiagnosticCollection();
      var module = BxfReader.ReadFromFile(path, diagnosticCollection);
      if (module == null) {
        // TODO: Deal with error messages.
      } else {
        _modules.Add(module);
      }
    }
  }
}
