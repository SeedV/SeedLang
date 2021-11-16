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
      var @null = new NullValue();
      Assert.False(@null.Boolean);
      Assert.Equal(0, @null.Number);
      Assert.Equal("", @null.String);
      Assert.Equal("", @null.ToString());
    }

    [Fact]
    public void TestBooleanValue() {
      var boolean = new BooleanValue(false);
      Assert.False(boolean.Boolean);
      Assert.Equal(0, boolean.Number);
      Assert.Equal(_expectedFalseString, boolean.String);
      Assert.Equal(_expectedFalseString, boolean.ToString());

      boolean = new BooleanValue(true);
      Assert.True(boolean.Boolean);
      Assert.Equal(1, boolean.Number);
      Assert.Equal(_expectedTrueString, boolean.String);
      Assert.Equal(_expectedTrueString, boolean.ToString());
    }

    [Fact]
    public void TestNumberValue() {
      var number = new NumberValue(1);
      Assert.True(number.Boolean);
      Assert.Equal(1, number.Number);
      Assert.Equal("1", number.String);
      Assert.Equal("1", number.ToString());

      number = new NumberValue(2.5);
      Assert.True(number.Boolean);
      Assert.Equal(2.5, number.Number);
      Assert.Equal("2.5", number.String);
      Assert.Equal("2.5", number.ToString());
    }

    [Fact]
    public void TestStringValue() {
      var str = new StringValue("");
      Assert.False(str.Boolean);
      Assert.Equal(0, str.Number);
      Assert.Equal("", str.String);
      Assert.Equal("", str.ToString());

      str = new StringValue(_expectedFalseString);
      Assert.True(str.Boolean);
      Assert.Equal(0, str.Number);
      Assert.Equal(_expectedFalseString, str.String);
      Assert.Equal(_expectedFalseString, str.ToString());

      str = new StringValue(_expectedTrueString);
      Assert.True(str.Boolean);
      Assert.Equal(0, str.Number);
      Assert.Equal(_expectedTrueString, str.String);
      Assert.Equal(_expectedTrueString, str.ToString());
    }

    [Fact]
    public void TestValueEquality() {
      Assert.True(null as IValue == null);
      Assert.False(new NullValue() == null);
      Assert.False(new BooleanValue(false) == null);
      Assert.False(new NumberValue(0) == null);
      Assert.False(new StringValue("") == null);

      Assert.NotEqual<IValue>(new NullValue(), new BooleanValue(false));
      Assert.NotEqual<IValue>(new NullValue(), new NumberValue(0));
      Assert.NotEqual<IValue>(new NullValue(), new StringValue(""));
      Assert.Equal(new NullValue(), new NullValue());

      Assert.NotEqual<IValue>(new BooleanValue(false), new NullValue());
      Assert.NotEqual<IValue>(new BooleanValue(false), new StringValue(""));
      Assert.Equal(new BooleanValue(false), new BooleanValue(false));
      Assert.Equal(new BooleanValue(true), new BooleanValue(true));
      Assert.NotEqual(new BooleanValue(false), new BooleanValue(true));

      Assert.NotEqual<IValue>(new BooleanValue(false), new NumberValue(1));
      Assert.NotEqual<IValue>(new BooleanValue(false), new NumberValue(2));
      Assert.NotEqual<IValue>(new BooleanValue(true), new NumberValue(0));
      Assert.NotEqual<IValue>(new BooleanValue(true), new NumberValue(2));
      Assert.Equal<IValue>(new BooleanValue(false), new NumberValue(0));
      Assert.Equal<IValue>(new BooleanValue(true), new NumberValue(1));

      Assert.NotEqual<IValue>(new NumberValue(0), new NullValue());
      Assert.NotEqual<IValue>(new NumberValue(0), new StringValue(""));

      Assert.NotEqual<IValue>(new NumberValue(1), new BooleanValue(false));
      Assert.NotEqual<IValue>(new NumberValue(2), new BooleanValue(false));
      Assert.NotEqual<IValue>(new NumberValue(0), new BooleanValue(true));
      Assert.NotEqual<IValue>(new NumberValue(2), new BooleanValue(true));
      Assert.Equal<IValue>(new NumberValue(0), new BooleanValue(false));
      Assert.Equal<IValue>(new NumberValue(1), new BooleanValue(true));

      Assert.NotEqual<IValue>(new StringValue(""), new NullValue());
      Assert.NotEqual<IValue>(new StringValue(""), new BooleanValue(false));
      Assert.NotEqual<IValue>(new StringValue(""), new NumberValue(0));
      Assert.NotEqual<IValue>(new StringValue("0"), new StringValue("1"));
      Assert.Equal<IValue>(new StringValue("1"), new StringValue("1"));
    }
  }
}
