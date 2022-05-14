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
using System.Collections.Immutable;
using SeedLang.Common;
using Xunit;

namespace SeedLang.Runtime.Tests {
  using NativeFunction = HeapObject.NativeFunction;

  public class VMValueTests {
    private readonly string _expectedNilString = "None";
    private readonly string _expectedFalseString = "False";
    private readonly string _expectedTrueString = "True";

    [Fact]
    public void TestNil() {
      var nil = new VMValue();
      Assert.True(nil.IsNil);
      Assert.False(nil.AsBoolean());
      Assert.Equal(0, nil.AsNumber());
      Assert.Equal(_expectedNilString, nil.AsString());
      Assert.Equal(_expectedNilString, nil.ToString());

      var exception1 = Assert.Throws<DiagnosticException>(() => nil.Length);
      Assert.Equal(Message.RuntimeErrorNotCountable, exception1.Diagnostic.MessageId);
      var exception2 = Assert.Throws<DiagnosticException>(() => nil[new VMValue(0)]);
      Assert.Equal(Message.RuntimeErrorNotSubscriptable, exception2.Diagnostic.MessageId);
    }

    [Fact]
    public void TestBoolean() {
      var boolean = new VMValue(false);
      Assert.True(boolean.IsBoolean);
      Assert.False(boolean.AsBoolean());
      Assert.Equal(0, boolean.AsNumber());
      Assert.Equal(_expectedFalseString, boolean.AsString());
      Assert.Equal(_expectedFalseString, boolean.ToString());

      boolean = new VMValue(true);
      Assert.True(boolean.IsBoolean);
      Assert.True(boolean.AsBoolean());
      Assert.Equal(1, boolean.AsNumber());
      Assert.Equal(_expectedTrueString, boolean.AsString());
      Assert.Equal(_expectedTrueString, boolean.ToString());

      var exception1 = Assert.Throws<DiagnosticException>(() => boolean.Length);
      Assert.Equal(Message.RuntimeErrorNotCountable, exception1.Diagnostic.MessageId);
      var exception2 = Assert.Throws<DiagnosticException>(() => boolean[new VMValue(0)]);
      Assert.Equal(Message.RuntimeErrorNotSubscriptable, exception2.Diagnostic.MessageId);
    }

    [Fact]
    public void TestNumber() {
      var number = new VMValue(1);
      Assert.True(number.IsNumber);
      Assert.True(number.AsBoolean());
      Assert.Equal(1, number.AsNumber());
      Assert.Equal("1", number.AsString());
      Assert.Equal("1", number.ToString());

      number = new VMValue(2.5);
      Assert.True(number.IsNumber);
      Assert.True(number.AsBoolean());
      Assert.Equal(2.5, number.AsNumber());
      Assert.Equal("2.5", number.AsString());
      Assert.Equal("2.5", number.ToString());

      var exception1 = Assert.Throws<DiagnosticException>(() => number.Length);
      Assert.Equal(Message.RuntimeErrorNotCountable, exception1.Diagnostic.MessageId);
      var exception2 = Assert.Throws<DiagnosticException>(() => number[new VMValue(0)]);
      Assert.Equal(Message.RuntimeErrorNotSubscriptable, exception2.Diagnostic.MessageId);
    }

    [Fact]
    public void TestString() {
      var str = new VMValue("");
      Assert.True(str.IsString);
      Assert.False(str.AsBoolean());
      Assert.Equal(0, str.AsNumber());
      Assert.Equal("", str.AsString());
      Assert.Equal("''", str.ToString());

      str = new VMValue(_expectedFalseString);
      Assert.True(str.IsString);
      Assert.True(str.AsBoolean());
      Assert.Equal(0, str.AsNumber());
      Assert.Equal(_expectedFalseString, str.AsString());
      Assert.Equal($"'{_expectedFalseString}'", str.ToString());

      str = new VMValue(_expectedTrueString);
      Assert.True(str.IsString);
      Assert.True(str.AsBoolean());
      Assert.Equal(0, str.AsNumber());
      Assert.Equal(_expectedTrueString, str.AsString());
      Assert.Equal($"'{_expectedTrueString}'", str.ToString());

      Assert.Equal(_expectedTrueString.Length, str.Length);
      for (int i = 0; i < _expectedTrueString.Length; i++) {
        Assert.Equal(_expectedTrueString[i].ToString(), str[new VMValue(i)].AsString());
      }
    }

    [Fact]
    public void TestDict() {
      string str = "2";
      var tuple = new VMValue(ImmutableArray.Create(new VMValue(1), new VMValue(2)));
      var value = new Dictionary<VMValue, VMValue>() {
        [new VMValue(1)] = new VMValue(1),
        [new VMValue(str)] = new VMValue(2),
        [tuple] = new VMValue(3),
      };
      var dict = new VMValue(value);
      Assert.True(dict.IsDict);
      Assert.Equal(3, dict.Length);
      Assert.True(dict[new VMValue(1)].IsNumber);
      Assert.Equal(1, dict[new VMValue(1)].AsNumber());
      Assert.True(dict[new VMValue(str)].IsNumber);
      Assert.Equal(2, dict[new VMValue(str)].AsNumber());
      Assert.True(dict[tuple].IsNumber);
      Assert.Equal(3, dict[tuple].AsNumber());
      Assert.Equal("{1: 1, '2': 2, (1, 2): 3}", dict.AsString());

      string testString = "test";
      dict[new VMValue(1)] = new VMValue(testString);
      Assert.True(dict[new VMValue(1)].IsString);
      Assert.Equal(testString, dict[new VMValue(1)].AsString());
      Assert.Equal("{1: 'test', '2': 2, (1, 2): 3}", dict.AsString());

      dict[new VMValue()] = new VMValue(100);
      Assert.Equal("{1: 'test', '2': 2, (1, 2): 3, None: 100}", dict.AsString());

      var exception = Assert.Throws<DiagnosticException>(() => {
        dict[new VMValue(new List<VMValue>() { new VMValue(1) })] = new VMValue(2);
      });
      Assert.Equal(Message.RuntimeErrorUnhashableType, exception.Diagnostic.MessageId);

      exception = Assert.Throws<DiagnosticException>(() => {
        var list = new VMValue(new List<VMValue>() { });
        new VMValue(new Dictionary<VMValue, VMValue> {
          [list] = new VMValue(),
        });
      });
      Assert.Equal(Message.RuntimeErrorUnhashableType, exception.Diagnostic.MessageId);
    }

    [Fact]
    public void TestList() {
      var values = new List<VMValue>() {
        new VMValue(1),
        new VMValue(2),
        new VMValue(3),
      };
      var list = new VMValue(values);
      Assert.True(list.IsList);
      Assert.Equal(3, list.Length);
      Assert.True(list[new VMValue(0)].IsNumber);
      Assert.Equal(1, list[new VMValue(0)].AsNumber());
      Assert.True(list[new VMValue(1)].IsNumber);
      Assert.Equal(2, list[new VMValue(1)].AsNumber());
      Assert.True(list[new VMValue(2)].IsNumber);
      Assert.Equal(3, list[new VMValue(2)].AsNumber());

      string testString = "test";
      list[new VMValue(1)] = new VMValue(testString);
      Assert.True(list[new VMValue(1)].IsString);
      Assert.Equal(testString, list[new VMValue(1)].AsString());
    }

    [Fact]
    public void TestNativeFunction() {
      string add = "add";
      var nativeFunc = new NativeFunction(add, (VMValue[] args, int offset, int length, Sys _) => {
        if (length == 2) {
          return new VMValue(args[offset].AsNumber() + args[offset + 1].AsNumber());
        }
        throw new NotImplementedException();
      });
      var func = new VMValue(nativeFunc);
      Assert.Equal($"NativeFunction <{add}>", func.AsString());
      Assert.Equal($"NativeFunction <{add}>", func.ToString());
      Assert.True(func.IsFunction);
      var result = nativeFunc.Call(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2, null);
      Assert.Equal(3, result.AsNumber());
    }

    [Fact]
    public void TestValueEquality() {
      Assert.NotEqual(new VMValue(), new VMValue(false));
      Assert.NotEqual(new VMValue(), new VMValue(0));
      Assert.NotEqual(new VMValue(), new VMValue(""));
      Assert.Equal(new VMValue(), new VMValue());

      Assert.NotEqual(new VMValue(false), new VMValue());
      Assert.NotEqual(new VMValue(false), new VMValue(""));
      Assert.Equal(new VMValue(false), new VMValue(false));
      Assert.Equal(new VMValue(true), new VMValue(true));
      Assert.NotEqual(new VMValue(false), new VMValue(true));

      Assert.NotEqual(new VMValue(false), new VMValue(1));
      Assert.NotEqual(new VMValue(false), new VMValue(2));
      Assert.NotEqual(new VMValue(true), new VMValue(0));
      Assert.NotEqual(new VMValue(true), new VMValue(2));
      Assert.Equal(new VMValue(false), new VMValue(0));
      Assert.Equal(new VMValue(true), new VMValue(1));

      Assert.NotEqual(new VMValue(0), new VMValue());
      Assert.NotEqual(new VMValue(0), new VMValue(""));

      Assert.NotEqual(new VMValue(1), new VMValue(false));
      Assert.NotEqual(new VMValue(2), new VMValue(false));
      Assert.NotEqual(new VMValue(0), new VMValue(true));
      Assert.NotEqual(new VMValue(2), new VMValue(true));
      Assert.Equal(new VMValue(0), new VMValue(false));
      Assert.Equal(new VMValue(1), new VMValue(true));

      Assert.NotEqual(new VMValue(""), new VMValue());
      Assert.NotEqual(new VMValue(""), new VMValue(false));
      Assert.NotEqual(new VMValue(""), new VMValue(0));
      Assert.NotEqual(new VMValue("0"), new VMValue("1"));
      Assert.Equal(new VMValue("1"), new VMValue("1"));

      Assert.NotEqual(new VMValue(new List<VMValue>() { new VMValue(1) }), new VMValue());
      Assert.NotEqual(new VMValue(new List<VMValue>() { new VMValue(1) }), new VMValue(false));
      Assert.NotEqual(new VMValue(new List<VMValue>() { new VMValue(1) }), new VMValue(0));
      Assert.NotEqual(new VMValue(new List<VMValue>() { new VMValue(1) }), new VMValue("1"));
      Assert.NotEqual(new VMValue(new List<VMValue>() { new VMValue(1) }),
                      new VMValue(new List<VMValue>() { new VMValue(2) }));
      Assert.NotEqual(new VMValue(new List<VMValue>() { new VMValue(1) }),
                      new VMValue(new List<VMValue>() { new VMValue(1), new VMValue(2) }));
      Assert.Equal(new VMValue(new List<VMValue>() { new VMValue(1) }),
                   new VMValue(new List<VMValue>() { new VMValue(1) }));
      var list = new List<VMValue>() { new VMValue(1) };
      Assert.Equal(new VMValue(list), new VMValue(list));
    }

    [Fact]
    public void TestValueWithReferenceCycle() {
      var a = new VMValue(new List<VMValue>() {
        new VMValue(1),
        new VMValue(2),
      });
      var b = new VMValue(new List<VMValue>() { a });
      a[new VMValue(1)] = b;
      Assert.Equal("[1, [[...]]]", a.ToString());
      Assert.Equal("[[1, [...]]]", b.ToString());

      a = new VMValue(new Dictionary<VMValue, VMValue>());
      b = new VMValue(new Dictionary<VMValue, VMValue>() {
        [new VMValue("a")] = a,
      });
      a[new VMValue("b")] = b;
      Assert.Equal("{'b': {'a': {...}}}", a.ToString());
      Assert.Equal("{'a': {'b': {...}}}", b.ToString());

      a = new VMValue(new List<VMValue>() {
        new VMValue(1),
        new VMValue(2),
      });
      b = new VMValue(new Dictionary<VMValue, VMValue>() {
        [new VMValue("a")] = a,
      });
      a[new VMValue(1)] = b;
      Assert.Equal("[1, {'a': [...]}]", a.ToString());
      Assert.Equal("{'a': [1, {...}]}", b.ToString());
    }
  }
}
