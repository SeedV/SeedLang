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
using System.Globalization;
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
      Assert.Throws<FormatException>(() => Message.ExampleMessageWithOneArgument1.Format());
      Assert.Equal("ExampleMessageWithTwoArguments Hello 3.14",
                   Message.ExampleMessageWithTwoArguments2.Format("Hello", (3.14).ToString()));
    }

    [Theory]
    [InlineData(Message.Okay, "en", "Okay")]
    [InlineData(Message.Okay, "zh-CN", "好")]
    [InlineData(Message.Okay, "en-US", "Okay")]  // "en-US" is able to fall back to "en".
    [InlineData(Message.Okay, "", "Okay")]       // Empty locale. Falls back to "en".
    [InlineData(Message.Okay, "ab", "Okay")]     // Very rare locale. Falls back to "en".
    public void TestLocalizedMessages(Message message, string locale, string localizedString) {
      Assert.Equal(localizedString, message.Format(new CultureInfo(locale)));
    }

    [Theory]
    [InlineData(Message.SyntaxErrorMissingToken2,
                "en",
                "Missing token. Found token: X. Expected token: Y",
                "X",
                "Y")]
    [InlineData(Message.SyntaxErrorMissingToken2,
                "en-US",
                "Missing token. Found token: X. Expected token: Y",
                "X",
                "Y")]
    [InlineData(Message.SyntaxErrorMissingToken2,
                "zh-CN",
                "缺少 token. 遇到的 token: X. 期望的 token: Y",
                "X",
                "Y")]
    public void TestLocalizedMessagesWithTwoArguments(Message message,
                                                       string locale,
                                                       string localizedString,
                                                       string arg1,
                                                       string arg2) {
      Assert.Equal(localizedString, message.Format(new CultureInfo(locale), arg1, arg2));
    }

    [Fact]
    public void TestChangeCurrentCulture() {
      Assert.Equal("Yes", Message.Yes.Format());
      CultureInfo.CurrentCulture = new CultureInfo("zh-CN");
      Assert.Equal("是", Message.Yes.Format());
      CultureInfo.CurrentCulture = new CultureInfo("en-US");
      Assert.Equal("Yes", Message.Yes.Format());
      CultureInfo.CurrentCulture = new CultureInfo("en");
      Assert.Equal("Yes", Message.Yes.Format());
      CultureInfo.CurrentCulture = new CultureInfo("ab");
      Assert.Equal("Yes", Message.Yes.Format());
    }
  }
}
