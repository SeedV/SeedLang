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
using Xunit;

namespace SeedLang.Common.Tests {
  public class MessageTests {
    [Fact]
    public void TestDevStrings() {
      Assert.Equal("Yes", Message.Yes.Get());
      Assert.Equal("No", Message.No.Get());
      Assert.Equal("Okay", Message.Okay.Get());
      Assert.Equal("ExampleMessageWithOneArgument {0}",
                   Message.ExampleMessageWithOneArgument1.Get());
      Assert.Equal("ExampleMessageWithTwoArguments {0} {1}",
                   Message.ExampleMessageWithTwoArguments2.Get());
      Assert.Equal("ExampleMessageWithNineArguments {0} {1} {2} {3} {4} {5} {6} {7} {8}",
                   Message.ExampleMessageWithNineArguments9.Get());

      Assert.Equal("Yes", Message.Yes.Format());
      Assert.Equal("Yes", Message.Yes.Format("Unused Argument"));
      Assert.Equal("ExampleMessageWithOneArgument Hello",
                   Message.ExampleMessageWithOneArgument1.Format("Hello"));
      Assert.Throws<ArgumentException>(() => Message.ExampleMessageWithOneArgument1.Format());
      Assert.Equal("ExampleMessageWithTwoArguments Hello 3.14",
                   Message.ExampleMessageWithTwoArguments2.Format("Hello", (3.14).ToString()));
    }
  }
}
