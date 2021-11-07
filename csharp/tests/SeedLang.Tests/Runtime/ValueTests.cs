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

namespace SeedLang.Runtime.Tests {
  public class ValueTests {
    private readonly string _expectedFalseString = "False";
    private readonly string _expectedTrueString = "True";

    [Fact]
    public void TestNullValue() {
      var boolean = new NullValue();
      Assert.False(boolean.Boolean);
      Assert.Equal(0, boolean.Number);
      Assert.Equal("", boolean.String);
    }

    [Fact]
    public void TestBooleanValue() {
      var boolean = new BooleanValue();
      Assert.False(boolean.Boolean);
      Assert.Equal(0, boolean.Number);
      Assert.Equal(_expectedFalseString, boolean.String);

      boolean = new BooleanValue(false);
      Assert.False(boolean.Boolean);
      Assert.Equal(0, boolean.Number);
      Assert.Equal(_expectedFalseString, boolean.String);

      boolean = new BooleanValue(true);
      Assert.True(boolean.Boolean);
      Assert.Equal(1, boolean.Number);
      Assert.Equal(_expectedTrueString, boolean.String);
    }

    [Fact]
    public void TestNumberValue() {
      var number = new NumberValue();
      Assert.False(number.Boolean);
      Assert.Equal(0, number.Number);
      Assert.Equal("0", number.String);

      number = new NumberValue(1);
      Assert.True(number.Boolean);
      Assert.Equal(1, number.Number);
      Assert.Equal("1", number.String);

      number = new NumberValue(2.5);
      Assert.True(number.Boolean);
      Assert.Equal(2.5, number.Number);
      Assert.Equal("2.5", number.String);
    }

    [Fact]
    public void TestStringValue() {
      var str = new StringValue();
      Assert.False(str.Boolean);
      Assert.Equal(0, str.Number);
      Assert.Equal("", str.String);

      str = new StringValue("1");
      Assert.False(str.Boolean);
      Assert.Equal(1, str.Number);
      Assert.Equal("1", str.String);

      str = new StringValue(_expectedTrueString);
      Assert.True(str.Boolean);
      Assert.Equal(0, str.Number);
      Assert.Equal(_expectedTrueString, str.String);
    }
  }
}
