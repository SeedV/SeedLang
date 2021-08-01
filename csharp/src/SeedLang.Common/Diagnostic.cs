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

namespace SeedLang.Common {
  // Pre-defined diagnostic reporter names for the system components.
  public static class SystemReporters {
    public const string SeedBlock = "SeedLang.Block";
    public const string SeedX = "SeedLang.X";
    public const string SeedAst = "SeedLang.Ast";
    public const string SeedVM = "SeedLang.VM";
  }

  // An immutable dignostic info, such as a compiler error or warning.
  public class Diagnostic {
    // The name of the reporter.
    //
    // SeedLang system components that report diagnostics are named as "SeedLang.ComponentName". See
    // SystemReporters for pre-defined names.
    //
    // Third-party extensions that report diagnostics will be named as "Contributor.ExtensionName".
    // There will be an ExtensionHelper to help format reporter names.
    public string Reporter { get; }
    // The time that the diagnostic is reported.
    public string Timestamp { get; }
    // The severity of the diagnostic.
    public Severity Severity { get; }
    // The name of the source code module.
    public string Module { get; }
    // The corresponding code range of the diagnostic.
    public Range Range { get; }
    // The string message. This message should be localized and formatted by the
    // SeedLang.Common.Message type.
    public string LocalizedMessage { get; }

    public Diagnostic(string reporter, Severity severity, string module, Range range, string localizedMessage) {
      Reporter = reporter;
      Timestamp = Utils.Timestamp();
      Severity = severity;
      Module = module;
      Range = range;
      LocalizedMessage = localizedMessage;
    }
  }
}
