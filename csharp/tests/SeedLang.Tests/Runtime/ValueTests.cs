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

using SeedLang.Runtime;
using Xunit;

namespace SeedLang.Interpreter.Tests {
  public class ValueTests {
    private readonly string _expectedFalseString = "False";
    private readonly string _expectedTrueString = "True";

    [Fact]
    public void TestNoneValue() {
      var none = new Value();
      Assert.False(none.Boolean);
      Assert.Equal(0, none.Number);
      Assert.Equal("None", none.String);
      Assert.Equal("None", none.ToString());
    }

    [Fact]
    public void TestBooleanValue() {
      var boolean = new Value(false);
      Assert.False(boolean.Boolean);
      Assert.Equal(0, boolean.Number);
      Assert.Equal(_expectedFalseString, boolean.String);
      Assert.Equal(_expectedFalseString, boolean.ToString());

      boolean = new Value(true);
      Assert.True(boolean.Boolean);
      Assert.Equal(1, boolean.Number);
      Assert.Equal(_expectedTrueString, boolean.String);
      Assert.Equal(_expectedTrueString, boolean.ToString());
    }

    [Fact]
    public void TestNumberValue() {
      var number = new Value(1);
      Assert.True(number.Boolean);
      Assert.Equal(1, number.Number);
      Assert.Equal("1", number.String);
      Assert.Equal("1", number.ToString());

      number = new Value(2.5);
      Assert.True(number.Boolean);
      Assert.Equal(2.5, number.Number);
      Assert.Equal("2.5", number.String);
      Assert.Equal("2.5", number.ToString());
    }

    [Fact]
    public void TestStringValue() {
      var str = new Value("");
      Assert.False(str.Boolean);
      Assert.Equal(0, str.Number);
      Assert.Equal("", str.String);
      Assert.Equal("", str.ToString());

      str = new Value(_expectedFalseString);
      Assert.True(str.Boolean);
      Assert.Equal(0, str.Number);
      Assert.Equal(_expectedFalseString, str.String);
      Assert.Equal(_expectedFalseString, str.ToString());

      str = new Value(_expectedTrueString);
      Assert.True(str.Boolean);
      Assert.Equal(0, str.Number);
      Assert.Equal(_expectedTrueString, str.String);
      Assert.Equal(_expectedTrueString, str.ToString());
    }

    [Fact]
    public void TestVMValueEquality() {
      Assert.NotEqual(new Value(), new Value(false));
      Assert.NotEqual(new Value(), new Value(0));
      Assert.NotEqual(new Value(), new Value(""));
      Assert.Equal(new Value(), new Value());

      Assert.NotEqual(new Value(false), new Value());
      Assert.NotEqual(new Value(false), new Value(""));
      Assert.Equal(new Value(false), new Value(false));
      Assert.Equal(new Value(true), new Value(true));
      Assert.NotEqual(new Value(false), new Value(true));

      Assert.NotEqual(new Value(false), new Value(1));
      Assert.NotEqual(new Value(false), new Value(2));
      Assert.NotEqual(new Value(true), new Value(0));
      Assert.NotEqual(new Value(true), new Value(2));
      Assert.Equal(new Value(false), new Value(0));
      Assert.Equal(new Value(true), new Value(1));

      Assert.NotEqual(new Value(0), new Value());
      Assert.NotEqual(new Value(0), new Value(""));

      Assert.NotEqual(new Value(1), new Value(false));
      Assert.NotEqual(new Value(2), new Value(false));
      Assert.NotEqual(new Value(0), new Value(true));
      Assert.NotEqual(new Value(2), new Value(true));
      Assert.Equal(new Value(0), new Value(false));
      Assert.Equal(new Value(1), new Value(true));

      Assert.NotEqual(new Value(""), new Value());
      Assert.NotEqual(new Value(""), new Value(false));
      Assert.NotEqual(new Value(""), new Value(0));
      Assert.NotEqual(new Value("0"), new Value("1"));
      Assert.Equal(new Value("1"), new Value("1"));
    }
  }
}
