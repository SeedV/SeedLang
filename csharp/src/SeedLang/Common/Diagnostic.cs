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
using System.Text;

namespace SeedLang.Common {
  // Pre-defined diagnostic reporter names for the system components.
  public static class SystemReporters {
    public const string SeedAst = "SeedLang.Ast";
    public const string SeedBlock = "SeedLang.Block";
    public const string SeedRuntime = "SeedLang.Runtime";
    public const string SeedX = "SeedLang.X";
    public const string SeedVM = "SeedLang.VM";
  }

  // A dignostic info, such as a compiler error or warning.
  public sealed class Diagnostic {
    // The name of the reporter.
    //
    // SeedLang system components that report diagnostics are named as "SeedLang.ComponentName". See
    // SystemReporters for pre-defined names.
    //
    // Third-party extensions that report diagnostics will be named as "Contributor.ExtensionName".
    // There will be an ExtensionHelper to help format reporter names.
    public string Reporter { get; set; }
    // The time that the diagnostic is reported.
    public string Timestamp { get; set; }
    // The severity of the diagnostic.
    public Severity Severity { get; set; }
    // The name of the source code module.
    public string Module { get; set; }
    // The corresponding code range of the diagnostic.
    public Range Range { get; set; }
    // The ID of the message. For now, message IDs are the values of the enum type Message. The ID
    // of a message is unique but not stable, since the messages in the enum is arranged in
    // alphabetical order.
    //
    // TODO: Do we need stable message IDs?
    public Message MessageId { get; set; }
    // The string message. This message should be localized and formatted by the
    // SeedLang.Common.Message type.
    public string LocalizedMessage { get; set; }

    public Diagnostic(string reporter,
                      Severity severity,
                      string module,
                      Range range,
                      Message messageId,
                      string localizedMessage) {
      Reporter = reporter;
      Timestamp = Utils.Timestamp();
      Severity = severity;
      Module = module;
      Range = range;
      MessageId = messageId;
      LocalizedMessage = localizedMessage;
    }

    public override string ToString() {
      var sb = new StringBuilder();
      sb.Append(Timestamp);
      sb.AppendFormat(" {0}", Enum.GetName(typeof(Severity), Severity));
      sb.AppendFormat(" ({0}) <{1}> ", Reporter, Module);
      sb.Append(Range is null ? "[]" : Range.ToString());
      sb.AppendFormat(" {0}: {1}", (int)MessageId, LocalizedMessage);
      return sb.ToString();
    }
  }
}
