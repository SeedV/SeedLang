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
using System.Collections.Generic;
using SeedLang.Runtime;
using Xunit;

namespace SeedLang.Interpreter.Tests {
  public class ValueTests {
    private readonly string _expectedFalseString = "False";
    private readonly string _expectedTrueString = "True";

    [Fact]
    public void TestNoneValue() {
      var none = Value.None();
      Assert.True(none.IsNone());
      Assert.False(none.AsBoolean());
      Assert.Equal(0, none.AsNumber());
      Assert.Equal("None", none.AsString());
      Assert.Equal("None", none.ToString());

      Assert.Throws<NotImplementedException>(() => none.Count());
      Assert.Throws<NotImplementedException>(() => none[0]);
    }

    [Fact]
    public void TestBooleanValue() {
      var boolean = Value.Boolean(false);
      Assert.True(boolean.IsBoolean());
      Assert.False(boolean.AsBoolean());
      Assert.Equal(0, boolean.AsNumber());
      Assert.Equal(_expectedFalseString, boolean.AsString());
      Assert.Equal(_expectedFalseString, boolean.ToString());

      boolean = Value.Boolean(true);
      Assert.True(boolean.IsBoolean());
      Assert.True(boolean.AsBoolean());
      Assert.Equal(1, boolean.AsNumber());
      Assert.Equal(_expectedTrueString, boolean.AsString());
      Assert.Equal(_expectedTrueString, boolean.ToString());

      Assert.Throws<NotImplementedException>(() => boolean.Count());
      Assert.Throws<NotImplementedException>(() => boolean[0]);
    }

    [Fact]
    public void TestNumberValue() {
      var number = Value.Number(1);
      Assert.True(number.IsNumber());
      Assert.True(number.AsBoolean());
      Assert.Equal(1, number.AsNumber());
      Assert.Equal("1", number.AsString());
      Assert.Equal("1", number.ToString());

      number = Value.Number(2.5);
      Assert.True(number.IsNumber());
      Assert.True(number.AsBoolean());
      Assert.Equal(2.5, number.AsNumber());
      Assert.Equal("2.5", number.AsString());
      Assert.Equal("2.5", number.ToString());

      Assert.Throws<NotImplementedException>(() => number.Count());
      Assert.Throws<NotImplementedException>(() => number[0]);
    }

    [Fact]
    public void TestStringValue() {
      var str = Value.String("");
      Assert.True(str.IsString());
      Assert.False(str.AsBoolean());
      Assert.Equal(0, str.AsNumber());
      Assert.Equal("", str.AsString());
      Assert.Equal("", str.ToString());

      str = Value.String(_expectedFalseString);
      Assert.True(str.IsString());
      Assert.True(str.AsBoolean());
      Assert.Equal(0, str.AsNumber());
      Assert.Equal(_expectedFalseString, str.AsString());
      Assert.Equal(_expectedFalseString, str.ToString());

      str = Value.String(_expectedTrueString);
      Assert.True(str.IsString());
      Assert.True(str.AsBoolean());
      Assert.Equal(0, str.AsNumber());
      Assert.Equal(_expectedTrueString, str.AsString());
      Assert.Equal(_expectedTrueString, str.ToString());

      Assert.Equal(_expectedTrueString.Length, str.Count());
      for (int i = 0; i < _expectedTrueString.Length; i++) {
        Assert.Equal(_expectedTrueString[i].ToString(), str[i].AsString());
      }
    }

    [Fact]
    public void TestListValue() {
      var values = new List<Value>() {
        Value.Number(1),
        Value.Number(2),
        Value.Number(3),
      };
      var list = Value.List(values);
      Assert.True(list.IsList());
      Assert.Equal(3, list.Count());
      Assert.True(list[0].IsNumber());
      Assert.Equal(1, list[0].AsNumber());
      Assert.True(list[1].IsNumber());
      Assert.Equal(2, list[1].AsNumber());
      Assert.True(list[2].IsNumber());
      Assert.Equal(3, list[2].AsNumber());

      string testString = "test";
      list[1] = Value.String(testString);
      Assert.True(list[1].IsString());
      Assert.Equal(testString, list[1].AsString());
    }

    [Fact]
    public void TestValueEquality() {
      Assert.NotEqual(Value.None(), Value.Boolean(false));
      Assert.NotEqual(Value.None(), Value.Number(0));
      Assert.NotEqual(Value.None(), Value.String(""));
      Assert.Equal(Value.None(), Value.None());

      Assert.NotEqual(Value.Boolean(false), Value.None());
      Assert.NotEqual(Value.Boolean(false), Value.String(""));
      Assert.Equal(Value.Boolean(false), Value.Boolean(false));
      Assert.Equal(Value.Boolean(true), Value.Boolean(true));
      Assert.NotEqual(Value.Boolean(false), Value.Boolean(true));

      Assert.NotEqual(Value.Boolean(false), Value.Number(1));
      Assert.NotEqual(Value.Boolean(false), Value.Number(2));
      Assert.NotEqual(Value.Boolean(true), Value.Number(0));
      Assert.NotEqual(Value.Boolean(true), Value.Number(2));
      Assert.Equal(Value.Boolean(false), Value.Number(0));
      Assert.Equal(Value.Boolean(true), Value.Number(1));

      Assert.NotEqual(Value.Number(0), Value.None());
      Assert.NotEqual(Value.Number(0), Value.String(""));

      Assert.NotEqual(Value.Number(1), Value.Boolean(false));
      Assert.NotEqual(Value.Number(2), Value.Boolean(false));
      Assert.NotEqual(Value.Number(0), Value.Boolean(true));
      Assert.NotEqual(Value.Number(2), Value.Boolean(true));
      Assert.Equal(Value.Number(0), Value.Boolean(false));
      Assert.Equal(Value.Number(1), Value.Boolean(true));

      Assert.NotEqual(Value.String(""), Value.None());
      Assert.NotEqual(Value.String(""), Value.Boolean(false));
      Assert.NotEqual(Value.String(""), Value.Number(0));
      Assert.NotEqual(Value.String("0"), Value.String("1"));
      Assert.Equal(Value.String("1"), Value.String("1"));
    }
  }
}
