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
using SeedLang.X;

namespace SeedLang {
  // A facade class to provide an unified interface for the SeedLang system.
  //
  // This is a singleton class. The interfaces include validating and running a SeedBlock or SeedX
  // program, registering observers to visualize program execution, etc.
  public sealed class Engine : IEngine {
    private static readonly Lazy<IEngine> _engine = new Lazy<IEngine>(() => new Engine());

    public static IEngine Instance {
      get {
        return _engine.Value;
      }
    }

    public IEnumerable<string> BinaryOperators { get { return new List<string>(); } }

    private Engine() {
    }

    public bool Dryrun(string source,
                   string module = "",
                   ParseRule rule = ParseRule.Statement,
                   Language language = Language.Python,
                   DiagnosticCollection errors = null) {
      // TODO: need a method to choose the corresponding parser (SeedBlock or SeedX) of this source
      // code.
      PythonParser.Dryrun(source, rule, errors);
      return errors.Diagnostics.Count == 0;
    }

    public DiagnosticCollection Run(string source,
                                    string module = "",
                                    ParseRule rule = ParseRule.Statement,
                                    Language language = Language.Python) {
      throw new NotImplementedException();
    }
  }
}
