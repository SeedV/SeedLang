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

using System.Linq;
using Xunit;

namespace SeedLang.Common.Tests {
  public class DiagnosticCollectionTests {
    [Fact]
    public void TestReport() {
      var collection = new DiagnosticCollection();
      Assert.Empty(collection.Diagnostics);
      collection.Report(
          new Diagnostic(SystemReporters.SeedAst,
                         Severity.Error,
                         "module1",
                         null,
                         Message.Okay,
                         Message.Okay.Get()));
      Assert.Single(collection.Diagnostics);
      collection.Report(SystemReporters.SeedBlock,
                        Severity.Warning,
                        "module2",
                        null,
                        Message.Okay);
      Assert.Equal(2, collection.Diagnostics.Count);
      collection.Report(SystemReporters.SeedBlock,
                        Severity.Warning,
                        "module3",
                        null,
                        Message.ExampleMessageWithOneArgument1,
                        "arg1");
      Assert.Equal(3, collection.Diagnostics.Count);
      Assert.Contains("arg1", collection.Diagnostics[2].LocalizedMessage);
      collection.Report(SystemReporters.SeedBlock,
                        Severity.Warning,
                        "module4",
                        null,
                        Message.ExampleMessageWithTwoArguments2,
                        "arg1",
                        "arg2");
      Assert.Equal(4, collection.Diagnostics.Count);
      Assert.Contains("arg1", collection.Diagnostics[3].LocalizedMessage);
      Assert.Contains("arg2", collection.Diagnostics[3].LocalizedMessage);
    }

    [Fact]
    public void TestLinqQueries() {
      var collection = new DiagnosticCollection();
      collection.Report(SystemReporters.SeedAst, Severity.Error,
                        "module1", null,
                        Message.Okay);
      collection.Report(SystemReporters.SeedAst, Severity.Info,
                        "module2", null,
                        Message.Okay);
      collection.Report(SystemReporters.SeedBlock, Severity.Fatal,
                        "module3", null,
                        Message.Okay);
      collection.Report(SystemReporters.SeedBlock, Severity.Warning,
                        "module4", null,
                        Message.Okay);
      var query1 = from diagnostic in collection.Diagnostics
                   where diagnostic.Reporter == SystemReporters.SeedAst &&
                         diagnostic.Severity == Severity.Error
                   select diagnostic;
      var list1 = query1.ToList();
      Assert.Single(list1);
      Assert.Equal("module1", list1[0].Module);
      var query2 = from diagnostic in collection.Diagnostics
                   where diagnostic.Module == "module4"
                   select diagnostic;
      var list2 = query2.ToList();
      Assert.Single(list2);
      Assert.Equal("module4", list2[0].Module);
      var query3 = from diagnostic in collection.Diagnostics
                   group diagnostic by diagnostic.Reporter;
      var list3 = query3.ToList();
      Assert.Equal(2, list3.Count);
      var list31 = list3[0].ToList();
      Assert.Equal(2, list31.Count);
      Assert.Equal(SystemReporters.SeedAst, list31[0].Reporter);
      Assert.Equal(SystemReporters.SeedAst, list31[1].Reporter);
      var list32 = list3[1].ToList();
      Assert.Equal(2, list32.Count);
      Assert.Equal(SystemReporters.SeedBlock, list32[0].Reporter);
      Assert.Equal(SystemReporters.SeedBlock, list32[1].Reporter);
    }
  }
}
