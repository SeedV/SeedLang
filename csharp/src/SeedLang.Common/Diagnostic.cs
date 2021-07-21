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
  // An immutable dignostic info, such as a compiler error or warning.
  public class Diagnostic {
    // The name or ID of the reporter.
    public string Reporter { get; }
    // The time that the diagnostic is reported.
    public string Timestamp { get; }
    // The severity of the diagnostic.
    public Severity Severity { get; }
    // The corresponding code range of the diagnostic.
    public Range Range { get; }
    // The string message. This message should be localized and formatted by the
    // SeedLang.Common.Message type.
    public string LocalizedMessage { get; }

    public Diagnostic(string reporter, Severity severity, Range range, string localizedMessage) {
      Reporter = reporter;
      Timestamp = Utils.Timestamp();
      Severity = severity;
      Range = range;
      LocalizedMessage = localizedMessage;
    }
  }
}
