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
  public class DiagnosticExceptionTests {
    [Fact]
    public void TestDiagnosticException() {
      try {
        throw new DiagnosticException(SystemReporters.SeedBlock, Severity.Error, "Main",
                                      new BlockRange("1"), Message.Okay);
      } catch (DiagnosticException e) {
        Assert.Equal(e.Diagnostic.Range, new BlockRange("1"));
        Assert.Equal(Message.Okay, e.Diagnostic.MessageId);
      }

      try {
        throw new DiagnosticException(SystemReporters.SeedBlock, Severity.Error, "Main",
                                      new BlockRange("2"),
                                      Message.ExampleMessageWithOneArgument1, "arg1");
      } catch (DiagnosticException e) {
        Assert.Contains("arg1", e.Diagnostic.LocalizedMessage);
      }
    }
  }
}
