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

using Xunit;

namespace SeedLang.Common.Tests {
  public class DiagnosticTests {
    [Fact]
    public void TestDiagnosticToString() {
      string ts = Utils.Timestamp();
      var diagnostic = new Diagnostic(null, Severity.Error, null, null, Message.Okay, null);
      Assert.Equal("Error () <> [] 0: ", diagnostic.ToString().Substring(ts.Length + 1));
      diagnostic = new Diagnostic(SystemReporters.SeedAst, Severity.Info,
                                  "main", null, Message.Okay, "A sample message.");
      Assert.Equal("Info (SeedLang.Ast) <main> [] 0: A sample message.",
                   diagnostic.ToString().Substring(ts.Length + 1));
      diagnostic = new Diagnostic(SystemReporters.SeedX, Severity.Info,
                                  "main", new TextRange(1, 1, 1, 3),
                                  Message.Okay, "A sample message.");
      Assert.Equal("Info (SeedLang.X) <main> [Ln 1, Col 1 - Ln 1, Col 3] 0: A sample message.",
                   diagnostic.ToString().Substring(ts.Length + 1));
    }
  }
}
