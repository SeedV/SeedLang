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

namespace SeedLang.Block {
  // The in-memory representation of a SeedBlock program.
  //
  // A program is a set of SeedBlock modules. Each module can be serialized to a JSON string/file in
  // the Block Exchange Format (BXF). All the modules of a program can be serialized to a set of BXF
  // JSON files then wrapped into a ZIP package.
  //
  // This is a singleton class. There can be only one program instance in the memory.
  public class BlockProgram {
    private static readonly object _padlock = new object();
    private static BlockProgram _instance = null;
    private readonly List<Module> _modules;

    // A readonly interface for the client to access the contents of the program.
    public IReadOnlyList<Module> Modules => _modules;

    // The thread-safe singleton pattern.
    public static BlockProgram Instance {
      get {
        lock (_padlock) {
          if (_instance == null) {
            _instance = new BlockProgram();
          }
          return _instance;
        }
      }
    }

    BlockProgram() {
      _modules = new List<Module>();
    }

    // Clears the in-memory program.
    public void Clear() {
      _modules.Clear();
    }

    // Parses a module string in the BXF format and loads the module to the program.
    public void LoadModuleFromString(string bxfJson) {
      var module = BxfParser.ParseFromString(bxfJson);
      if (module != null) {
        _modules.Add(module);
      }
    }
  }
}
