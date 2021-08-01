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

using System.Collections;
using System.Collections.Generic;

namespace SeedLang.Common {
  // An immutable collection class to hold a set of diagnostics.
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
  public class DiagnosticCollection : IEnumerable {
    private readonly List<Diagnostic> _diagnostics = new List<Diagnostic>();

    public Diagnostic this[int index] {
      get {
        return _diagnostics[index];
      }
    }

    public int Count {
      get {
        return _diagnostics.Count;
      }
    }

    public DiagnosticCollection() {
    }

    // Reports a new diagnostic.
    public void Report(Diagnostic diagnostic) {
      _diagnostics.Add(diagnostic);
    }

    public IEnumerator GetEnumerator() {
      return _diagnostics.GetEnumerator();
    }

    // TODO: implement sorting, searching and grouping features.
  }
}
