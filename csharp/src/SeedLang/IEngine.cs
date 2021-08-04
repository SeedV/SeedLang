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
  public enum Language {
    Block,
    Python
  }

  public interface IEngine {
    IEnumerable<string> BinaryOperators { get; }

    bool Dryrun(string source,
                string module = "",
                ParseRule rule = ParseRule.Statement,
                Language language = Language.Python,
                DiagnosticCollection errors = null);

    DiagnosticCollection Run(string source,
                             string module = "",
                             ParseRule rule = ParseRule.Statement,
                             Language language = Language.Python);
  }
}
