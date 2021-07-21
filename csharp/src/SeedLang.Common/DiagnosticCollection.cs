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

namespace SeedLang.Common {
  // The collection class to maintain a set of diagnostics.
  //
  // A SeedLang environment may have one or more DiagnosticCollection instances. For example, a
  // single global DiagnosticCollection instance might be suitable for many simple circumstances,
  // while the following cases may want more than one DiagnosticCollection instances:
  //
  // 1) An incremental compile-then-evaluate environment may want to keep a separate
  //    DiagnosticCollection for each executable code part, for convenience purposes.
  //
  // 2) The editor/IDE may use a separate DiagnosticCollection to maintain a temporary state to
  //    supprot the features like temporary editing and validating, static checking, partial
  //    execution, editing during debugging, etc.
  public class DiagnosticCollection {
    private readonly List<Diagnostic> diagnostics = new List<Diagnostic>();

    public IReadOnlyList<Diagnostic> Diagnostics => diagnostics;

    public DiagnosticCollection() {
    }

    // Reports a new diagnostic.
    public void Report(Diagnostic diagnostic) {
      diagnostics.Add(diagnostic);
    }

    // Deletes a diagnostic from the collection.
    public void Delete(Diagnostic diagnostic) {
      Delete(diagnostic.Reporter, diagnostic.Timestamp);
    }

    // Deletes a diagnostic that the specified reporter reported at the given timestamp.
    public void Delete(string reporter, string timestamp) {
      // TODO: Implement the internal HashTable to support deleting by reporter/timestamp.
    }

    // Clears the collection.
    public void Clear() {
      diagnostics.Clear();
    }
  }
}
