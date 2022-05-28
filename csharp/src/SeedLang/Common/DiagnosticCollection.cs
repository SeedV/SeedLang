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

using System.Collections.Generic;

namespace SeedLang.Common {
  // A collection to maintain a set of diagnostics.
  //
  // A SeedLang environment may have one or more DiagnosticCollection instances. For example, a
  // single global DiagnosticCollection instance might be suitable for many simple circumstances,
  // while the following cases may want more than one DiagnosticCollection instances:
  //
  // 1) An incremental compile-then-evaluate environment may want to keep a separate
  //    DiagnosticCollection for each executable code part, for convenience purposes.
  //
  // 2) The editor/IDE may use a separate DiagnosticCollection to maintain a temporary state to
  //    support the features like temporary editing and validating, static checking, partial
  //    execution, editing during debugging, etc.
  public class DiagnosticCollection {
    private readonly List<Diagnostic> _diagnostics = new List<Diagnostic>();

    // The readonly view of the collection. Client code can use C#'s LINQ queries to search on the
    // collection or group the results. For example:
    //
    // var collection = new DiagnosticCollection();
    // var query = from diagnostic in collection.Diagnostics
    //             where diagnostic.Reporter == SystemReporters.SeedAst &&
    //                   diagnostic.Severity == Severity.Error
    //             select diagnostic;
    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;

    public DiagnosticCollection() {
    }

    // Reports a new diagnostic.
    public void Report(Diagnostic diagnostic) {
      _diagnostics.Add(diagnostic);
    }

    // Reports a new diagnostic, with a message that requires no arguments.
    public void Report(string reporter,
                       Severity severity,
                       string module,
                       TextRange range,
                       Message messageId) {
      _diagnostics.Add(
        new Diagnostic(reporter, severity, module, range, messageId, messageId.Format()));
    }

    // Reports a new diagnostic, with a message that requires one or more arguments to be formatted.
    public void Report(string reporter,
                       Severity severity,
                       string module,
                       TextRange range,
                       Message messageId,
                       params string[] arguments) {
      _diagnostics.Add(
        new Diagnostic(reporter, severity, module, range, messageId, messageId.Format(arguments)));
    }
  }
}
