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
    public void TestNone() {
      var none = new Value();
      Assert.True(none.IsNone);
      Assert.False(none.AsBoolean());
      Assert.Equal(0, none.AsNumber());
      Assert.Equal("None", none.AsString());
      Assert.Equal("None", none.ToString());

      var exception1 = Assert.Throws<DiagnosticException>(() => none.Length);
      Assert.Equal(Message.RuntimeErrorNotCountable, exception1.Diagnostic.MessageId);
      var exception2 = Assert.Throws<DiagnosticException>(() => none[0]);
      Assert.Equal(Message.RuntimeErrorNotSubscriptable, exception2.Diagnostic.MessageId);
      var exception3 = Assert.Throws<DiagnosticException>(() => none.Call(Array.Empty<Value>(),
                                                                          0,
                                                                          0));
      Assert.Equal(Message.RuntimeErrorNotCallable, exception3.Diagnostic.MessageId);
    }

    [Fact]
    public void TestBoolean() {
      var boolean = new Value(false);
      Assert.True(boolean.IsBoolean);
      Assert.False(boolean.AsBoolean());
      Assert.Equal(0, boolean.AsNumber());
      Assert.Equal(_expectedFalseString, boolean.AsString());
      Assert.Equal(_expectedFalseString, boolean.ToString());

      boolean = new Value(true);
      Assert.True(boolean.IsBoolean);
      Assert.True(boolean.AsBoolean());
      Assert.Equal(1, boolean.AsNumber());
      Assert.Equal(_expectedTrueString, boolean.AsString());
      Assert.Equal(_expectedTrueString, boolean.ToString());

      var exception1 = Assert.Throws<DiagnosticException>(() => boolean.Length);
      Assert.Equal(Message.RuntimeErrorNotCountable, exception1.Diagnostic.MessageId);
      var exception2 = Assert.Throws<DiagnosticException>(() => boolean[0]);
      Assert.Equal(Message.RuntimeErrorNotSubscriptable, exception2.Diagnostic.MessageId);
      var exception3 = Assert.Throws<DiagnosticException>(() => boolean.Call(Array.Empty<Value>(),
                                                                             0,
                                                                             0));
      Assert.Equal(Message.RuntimeErrorNotCallable, exception3.Diagnostic.MessageId);
    }

    [Fact]
    public void TestNumber() {
      var number = new Value(1);
      Assert.True(number.IsNumber);
      Assert.True(number.AsBoolean());
      Assert.Equal(1, number.AsNumber());
      Assert.Equal("1", number.AsString());
      Assert.Equal("1", number.ToString());

      number = new Value(2.5);
      Assert.True(number.IsNumber);
      Assert.True(number.AsBoolean());
      Assert.Equal(2.5, number.AsNumber());
      Assert.Equal("2.5", number.AsString());
      Assert.Equal("2.5", number.ToString());

      var exception1 = Assert.Throws<DiagnosticException>(() => number.Length);
      Assert.Equal(Message.RuntimeErrorNotCountable, exception1.Diagnostic.MessageId);
      var exception2 = Assert.Throws<DiagnosticException>(() => number[0]);
      Assert.Equal(Message.RuntimeErrorNotSubscriptable, exception2.Diagnostic.MessageId);
      var exception3 = Assert.Throws<DiagnosticException>(() => number.Call(Array.Empty<Value>(),
                                                                            0,
                                                                            0));
      Assert.Equal(Message.RuntimeErrorNotCallable, exception3.Diagnostic.MessageId);
    }

    [Fact]
    public void TestString() {
      var str = new Value("");
      Assert.True(str.IsString);
      Assert.False(str.AsBoolean());
      Assert.Equal(0, str.AsNumber());
      Assert.Equal("", str.AsString());
      Assert.Equal("", str.ToString());

      str = new Value(_expectedFalseString);
      Assert.True(str.IsString);
      Assert.True(str.AsBoolean());
      Assert.Equal(0, str.AsNumber());
      Assert.Equal(_expectedFalseString, str.AsString());
      Assert.Equal(_expectedFalseString, str.ToString());

      str = new Value(_expectedTrueString);
      Assert.True(str.IsString);
      Assert.True(str.AsBoolean());
      Assert.Equal(0, str.AsNumber());
      Assert.Equal(_expectedTrueString, str.AsString());
      Assert.Equal(_expectedTrueString, str.ToString());

      Assert.Equal(_expectedTrueString.Length, str.Length);
      for (int i = 0; i < _expectedTrueString.Length; i++) {
        Assert.Equal(_expectedTrueString[i].ToString(), str[i].AsString());
      }
    }

    [Fact]
    public void TestList() {
      var values = new List<Value>() {
        new Value(1),
        new Value(2),
        new Value(3),
      };
      var list = new Value(values);
      Assert.True(list.IsList);
      Assert.Equal(3, list.Length);
      Assert.True(list[0].IsNumber);
      Assert.Equal(1, list[0].AsNumber());
      Assert.True(list[1].IsNumber);
      Assert.Equal(2, list[1].AsNumber());
      Assert.True(list[2].IsNumber);
      Assert.Equal(3, list[2].AsNumber());

      string testString = "test";
      list[1] = new Value(testString);
      Assert.True(list[1].IsString);
      Assert.Equal(testString, list[1].AsString());
    }

    [Fact]
    public void TestNativeFunction() {
      var nativeFunc = new NativeFunction("add", (Value[] args, int offset, int length) => {
        if (length == 2) {
          return new Value(args[offset].AsNumber() + args[offset + 1].AsNumber());
        }
        throw new NotImplementedException();
      });
      var func = new Value(nativeFunc);
      Assert.Equal("NativeFunction <add>", func.AsString());
      Assert.Equal("NativeFunction <add>", func.ToString());
      Assert.True(func.IsFunction);
      var result = func.Call(new Value[] { new Value(1), new Value(2) }, 0, 2);
      Assert.Equal(3, result.AsNumber());
    }

    [Fact]
    public void TestValueEquality() {
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

      Assert.NotEqual(new Value(new List<Value>() { new Value(1) }), new Value());
      Assert.NotEqual(new Value(new List<Value>() { new Value(1) }), new Value(false));
      Assert.NotEqual(new Value(new List<Value>() { new Value(1) }), new Value(0));
      Assert.NotEqual(new Value(new List<Value>() { new Value(1) }), new Value("1"));
      Assert.NotEqual(new Value(new List<Value>() { new Value(1) }),
                      new Value(new List<Value>() { new Value(2) }));
      Assert.NotEqual(new Value(new List<Value>() { new Value(1) }),
                      new Value(new List<Value>() { new Value(1), new Value(2) }));
      Assert.NotEqual(new Value(new List<Value>() { new Value(1) }),
                      new Value(new List<Value>() { new Value(1) }));
      var list = new List<Value>() { new Value(1) };
      Assert.Equal(new Value(list), new Value(list));
    }

    [Fact]
    public void TestListWithReferenceCycle() {
      var a = new Value(new List<Value>() {
        new Value(1),
        new Value(2),
      });
      var b = new Value(new List<Value>() { a });
      a[1] = b;
      Assert.Equal("[1, [[...]]]", a.ToString());
      Assert.Equal("[[1, [...]]]", b.ToString());
    }
  }
}
