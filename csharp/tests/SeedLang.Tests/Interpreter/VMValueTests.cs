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

using Xunit;

namespace SeedLang.Interpreter.Tests {
  public class VMValueTests {
    private readonly string _expectedFalseString = "False";
    private readonly string _expectedTrueString = "True";

    [Fact]
    public void TestNullValue() {
      var @null = new VMValue();
      Assert.False(@null.Boolean);
      Assert.Equal(0, @null.Number);
      Assert.Equal("", @null.String);
      Assert.Equal("", @null.ToString());
    }

    [Fact]
    public void TestBooleanValue() {
      var boolean = new VMValue(false);
      Assert.False(boolean.Boolean);
      Assert.Equal(0, boolean.Number);
      Assert.Equal(_expectedFalseString, boolean.String);
      Assert.Equal(_expectedFalseString, boolean.ToString());

      boolean = new VMValue(true);
      Assert.True(boolean.Boolean);
      Assert.Equal(1, boolean.Number);
      Assert.Equal(_expectedTrueString, boolean.String);
      Assert.Equal(_expectedTrueString, boolean.ToString());
    }

    [Fact]
    public void TestNumberValue() {
      var number = new VMValue(1);
      Assert.True(number.Boolean);
      Assert.Equal(1, number.Number);
      Assert.Equal("1", number.String);
      Assert.Equal("1", number.ToString());

      number = new VMValue(2.5);
      Assert.True(number.Boolean);
      Assert.Equal(2.5, number.Number);
      Assert.Equal("2.5", number.String);
      Assert.Equal("2.5", number.ToString());
    }

    [Fact]
    public void TestStringValue() {
      var str = new VMValue("");
      Assert.False(str.Boolean);
      Assert.Equal(0, str.Number);
      Assert.Equal("", str.String);
      Assert.Equal("", str.ToString());

      str = new VMValue(_expectedFalseString);
      Assert.True(str.Boolean);
      Assert.Equal(0, str.Number);
      Assert.Equal(_expectedFalseString, str.String);
      Assert.Equal(_expectedFalseString, str.ToString());

      str = new VMValue(_expectedTrueString);
      Assert.True(str.Boolean);
      Assert.Equal(0, str.Number);
      Assert.Equal(_expectedTrueString, str.String);
      Assert.Equal(_expectedTrueString, str.ToString());
    }
  }
}
