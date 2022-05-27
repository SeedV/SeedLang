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

using System;

namespace SeedLang.Common {
  // An exception that contains a diagnostic info.
  //
  // This exception is only used for throwing exceptions among underlying components.
  //
  // When reporting diagnostics via SeedLang's public API, please let users to pass in a
  // DiagnosticCollection instance. For example:
  //
  // public bool DoSomething(int someParameter, DiagnosticCollection collection) {
  //   try {
  //     DoSomethingInternally();
  //   } catch (DiagnosticException e) {
  //     collection.Report(e.Diagnostic);
  //     return false;
  //   }
  //   return true;
  // }
  internal class DiagnosticException : Exception {
    public Diagnostic Diagnostic { get; private set; }

    public DiagnosticException(string reporter,
                               Severity severity,
                               string module,
                               TextRange range,
                               Message messageId) {
      Diagnostic =
          new Diagnostic(reporter, severity, module, range, messageId, messageId.Format());
    }

    public DiagnosticException(string reporter,
                               Severity severity,
                               string module,
                               TextRange range,
                               Message messageId,
                               params string[] arguments) {
      Diagnostic =
          new Diagnostic(reporter, severity, module, range, messageId, messageId.Format(arguments));
    }
  }
}
