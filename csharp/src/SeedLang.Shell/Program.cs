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
using SeedLang.Common;

namespace SeedLang.Shell {
  class Program {
    static void Main(string[] _) {
      // TODO: add a REPL class to encapsulate the input and execution process.
      while (true) {
        Console.Write("> ");
        string line = Console.ReadLine();
        if (line == "quit") {
          break;
        }
        DiagnosticCollection collection = Engine.RunStatement(line + "\n");
        if (collection.Diagnostics.Count > 0) {
          foreach (var diagnostic in collection.Diagnostics) {
            Console.WriteLine(diagnostic);
          }
        }
      }
    }
  }
}
