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
using System.Collections.Generic;
using SeedLang.Common;
using Xunit;

namespace SeedLang.Runtime.Tests {
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

      var exception1 = Assert.Throws<DiagnosticException>(() => none.Count());
      Assert.Equal(Message.RuntimeErrorNotCountable, exception1.Diagnostic.MessageId);
      var exception2 = Assert.Throws<DiagnosticException>(() => none[0]);
      Assert.Equal(Message.RuntimeErrorNotSubscriptable, exception2.Diagnostic.MessageId);
      var exception3 = Assert.Throws<DiagnosticException>(() => none.Call(Array.Empty<Value>()));
      Assert.Equal(Message.RuntimeErrorNotCallable, exception3.Diagnostic.MessageId);
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

      var exception1 = Assert.Throws<DiagnosticException>(() => boolean.Count());
      Assert.Equal(Message.RuntimeErrorNotCountable, exception1.Diagnostic.MessageId);
      var exception2 = Assert.Throws<DiagnosticException>(() => boolean[0]);
      Assert.Equal(Message.RuntimeErrorNotSubscriptable, exception2.Diagnostic.MessageId);
      var exception3 = Assert.Throws<DiagnosticException>(() => boolean.Call(Array.Empty<Value>()));
      Assert.Equal(Message.RuntimeErrorNotCallable, exception3.Diagnostic.MessageId);
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

      var exception1 = Assert.Throws<DiagnosticException>(() => number.Count());
      Assert.Equal(Message.RuntimeErrorNotCountable, exception1.Diagnostic.MessageId);
      var exception2 = Assert.Throws<DiagnosticException>(() => number[0]);
      Assert.Equal(Message.RuntimeErrorNotSubscriptable, exception2.Diagnostic.MessageId);
      var exception3 = Assert.Throws<DiagnosticException>(() => number.Call(Array.Empty<Value>()));
      Assert.Equal(Message.RuntimeErrorNotCallable, exception3.Diagnostic.MessageId);
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
    public void TestNativeFunction() {
      var nativeFunc = new NativeFunction("add", (ArraySegment<Value> parameters) => {
        if (parameters.Count == 2) {
          return Value.Number(parameters[0].AsNumber() + parameters[1].AsNumber());
        }
        throw new NotImplementedException();
      });
      var func = Value.Function(nativeFunc);
      Assert.Equal("NativeFunction <add>", func.AsString());
      Assert.Equal("NativeFunction <add>", func.ToString());
      Assert.True(func.IsFunction());
      var result = func.Call(new Value[] { Value.Number(1), Value.Number(2) });
      Assert.Equal(3, result.AsNumber());
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

      Assert.NotEqual(Value.List(new List<Value>() { Value.Number(1) }), Value.None());
      Assert.NotEqual(Value.List(new List<Value>() { Value.Number(1) }), Value.Boolean(false));
      Assert.NotEqual(Value.List(new List<Value>() { Value.Number(1) }), Value.Number(0));
      Assert.NotEqual(Value.List(new List<Value>() { Value.Number(1) }), Value.String("1"));
      Assert.NotEqual(Value.List(new List<Value>() { Value.Number(1) }),
                      Value.List(new List<Value>() { Value.Number(2) }));
      Assert.NotEqual(Value.List(new List<Value>() { Value.Number(1) }),
                      Value.List(new List<Value>() { Value.Number(1), Value.Number(2) }));
      Assert.NotEqual(Value.List(new List<Value>() { Value.Number(1) }),
                      Value.List(new List<Value>() { Value.Number(1) }));
      var list = new List<Value>() { Value.Number(1) };
      Assert.Equal(Value.List(list), Value.List(list));
    }

    [Fact]
    public void TestListWithReferenceCycle() {
      var a = Value.List(new List<Value>() {
        Value.Number(1),
        Value.Number(2),
      });
      var b = Value.List(new List<Value>() { a });
      a[1] = b;
      Assert.Equal("[1, [[...]]]", a.ToString());
      Assert.Equal("[[1, [...]]]", b.ToString());
    }
  }
}
