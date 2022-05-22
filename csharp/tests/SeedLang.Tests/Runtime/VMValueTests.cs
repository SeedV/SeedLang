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
using FluentAssertions;
using SeedLang.Common;
using SeedLang.Runtime.HeapObjects;
using Xunit;

namespace SeedLang.Runtime.Tests {
  public class VMValueTests {
    private readonly string _expectedNilString = "None";
    private readonly string _expectedFalseString = "False";
    private readonly string _expectedTrueString = "True";

    [Fact]
    public void TestNil() {
      var nil = new VMValue();
      nil.IsNil.Should().Be(true);
      nil.AsBoolean().Should().Be(false);
      nil.AsNumber().Should().Be(0);
      nil.AsString().Should().Be(_expectedNilString);
      nil.ToString().Should().Be(_expectedNilString);

      nil.Should().NotBe(new VMValue(false));
      nil.Should().NotBe(new VMValue(0));
      nil.Should().NotBe(new VMValue(""));
      nil.Should().Be(new VMValue());

      Action action = () => _ = nil.Length;
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorNotCountable);
      action = () => _ = nil[new VMValue(0)];
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorNotSubscriptable);
    }

    [Fact]
    public void TestBoolean() {
      var falseBool = new VMValue(false);
      falseBool.IsBoolean.Should().Be(true);
      falseBool.AsBoolean().Should().Be(false);
      falseBool.AsNumber().Should().Be(0);
      falseBool.AsString().Should().Be(_expectedFalseString);
      falseBool.ToString().Should().Be(_expectedFalseString);

      falseBool.Should().NotBe(new VMValue());
      falseBool.Should().NotBe(new VMValue(""));
      falseBool.Should().Be(new VMValue(false));
      falseBool.Should().NotBe(new VMValue(true));
      falseBool.Should().Be(new VMValue(0));
      falseBool.Should().NotBe(new VMValue(1));
      falseBool.Should().NotBe(new VMValue(2));

      var trueBool = new VMValue(true);
      trueBool.IsBoolean.Should().Be(true);
      trueBool.AsBoolean().Should().Be(true);
      trueBool.AsNumber().Should().Be(1);
      trueBool.AsString().Should().Be(_expectedTrueString);
      trueBool.ToString().Should().Be(_expectedTrueString);

      trueBool.Should().Be(new VMValue(true));
      trueBool.Should().Be(new VMValue(1));
      trueBool.Should().NotBe(new VMValue(0));
      trueBool.Should().NotBe(new VMValue(2));

      Action action = () => _ = falseBool.Length;
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorNotCountable);
      action = () => _ = falseBool[new VMValue(0)];
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorNotSubscriptable);
    }

    [Fact]
    public void TestNumber() {
      var zero = new VMValue(0);
      zero.IsNumber.Should().Be(true);
      zero.AsBoolean().Should().Be(false);
      zero.AsNumber().Should().Be(0);
      zero.AsString().Should().Be("0");
      zero.ToString().Should().Be("0");

      zero.Should().NotBe(new VMValue());
      zero.Should().NotBe(new VMValue(""));
      zero.Should().NotBe(new VMValue(true));
      zero.Should().Be(new VMValue(false));

      var one = new VMValue(1);
      one.IsNumber.Should().Be(true);
      one.AsBoolean().Should().Be(true);
      one.AsNumber().Should().Be(1);
      one.AsString().Should().Be("1");
      one.ToString().Should().Be("1");

      var number = new VMValue(2.5);
      number.IsNumber.Should().Be(true);
      number.AsBoolean().Should().Be(true);
      number.AsNumber().Should().Be(2.5);
      number.AsString().Should().Be("2.5");
      number.ToString().Should().Be("2.5");

      Action action = () => _ = one.Length;
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorNotCountable);
      action = () => _ = one[new VMValue(0)];
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorNotSubscriptable);
    }

    [Fact]
    public void TestString() {
      var emptyStr = new VMValue("");
      emptyStr.IsString.Should().Be(true);
      emptyStr.AsBoolean().Should().Be(false);
      emptyStr.AsNumber().Should().Be(0);
      emptyStr.AsString().Should().Be("");
      emptyStr.ToString().Should().Be("''");

      emptyStr.Should().Be(new VMValue(""));
      emptyStr.Should().NotBe(new VMValue(false));
      emptyStr.Should().NotBe(new VMValue(0));

      string expectedStr = "string";
      var str = new VMValue(expectedStr);
      str.IsString.Should().Be(true);
      str.AsBoolean().Should().Be(true);
      str.AsNumber().Should().Be(0);
      str.AsString().Should().Be($"{expectedStr}");
      str.ToString().Should().Be($"'{expectedStr}'");

      str.Length.Should().Be(expectedStr.Length);
      for (int i = 0; i < expectedStr.Length; i++) {
        str[new VMValue(i)].Should().Be(new VMValue(expectedStr[i].ToString()));
      }

      str.Should().Be(new VMValue(expectedStr));
      str.Should().NotBe(new VMValue("another string"));
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
      dict.IsDict.Should().Be(true);
      dict.Length.Should().Be(3);
      dict[new VMValue(1)].IsNumber.Should().Be(true);
      dict[new VMValue(1)].AsNumber().Should().Be(1);
      dict[new VMValue(str)].IsNumber.Should().Be(true);
      dict[new VMValue(str)].AsNumber().Should().Be(2);
      dict[tuple].IsNumber.Should().Be(true);
      dict[tuple].AsNumber().Should().Be(3);
      dict.AsString().Should().Be("{1: 1, '2': 2, (1, 2): 3}");

      string testString = "test";
      dict[new VMValue(1)] = new VMValue(testString);
      dict[new VMValue(1)].IsString.Should().Be(true);
      dict[new VMValue(1)].AsString().Should().Be(testString);
      dict.AsString().Should().Be("{1: 'test', '2': 2, (1, 2): 3}");

      dict[new VMValue()] = new VMValue(100);
      dict.AsString().Should().Be("{1: 'test', '2': 2, (1, 2): 3, None: 100}");

      Action action = () => {
        dict[new VMValue(new List<VMValue>() { new VMValue(1) })] = new VMValue(2);
      };
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorUnhashableType);

      action = () => {
        var list = new VMValue(new List<VMValue>() { });
        _ = new VMValue(new Dictionary<VMValue, VMValue> {
          [list] = new VMValue(),
        });
      };
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorUnhashableType);
    }

    [Fact]
    public void TestList() {
      var values = new List<VMValue>() {
        new VMValue(1),
        new VMValue(2),
        new VMValue(3),
      };
      var list = new VMValue(values);
      list.IsList.Should().Be(true);
      list.Length.Should().Be(values.Count);
      list[new VMValue(0)].IsNumber.Should().Be(true);
      list[new VMValue(0)].AsNumber().Should().Be(1);
      list[new VMValue(1)].IsNumber.Should().Be(true);
      list[new VMValue(1)].AsNumber().Should().Be(2);
      list[new VMValue(2)].IsNumber.Should().Be(true);
      list[new VMValue(2)].AsNumber().Should().Be(3);

      string testString = "test";
      list[new VMValue(1)] = new VMValue(testString);
      list[new VMValue(1)].IsString.Should().Be(true);
      list[new VMValue(1)].AsString().Should().Be(testString);

      list.Should().NotBe(new VMValue());
      list.Should().NotBe(new VMValue(false));
      list.Should().NotBe(new VMValue(0));
      list.Should().NotBe(new VMValue("1"));

      list = new VMValue(new List<VMValue>() { new VMValue(1) });
      list.Should().NotBe(new VMValue(new List<VMValue>() { new VMValue(2) }));
      list.Should().NotBe(new VMValue(new List<VMValue>() { new VMValue(1), new VMValue(2) }));
      list.Should().Be(new VMValue(new List<VMValue>() { new VMValue(1) }));
      values = new List<VMValue>() { new VMValue(1) };
      new VMValue(values).Should().Be(new VMValue(values));
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
      func.IsFunction.Should().Be(true);
      func.AsString().Should().Be($"NativeFunction <{add}>");
      func.ToString().Should().Be($"NativeFunction <{add}>");
      var result = nativeFunc.Call(new VMValue[] { new VMValue(1), new VMValue(2) }, 0, 2, null);
      result.AsNumber().Should().Be(3);
    }

    [Fact]
    public void TestStringSubscript() {
      var strValue = "string";
      var str = new VMValue(strValue);
      str[new VMValue(3)].AsString().Should().Be("i");
      str[new VMValue(-3)].AsString().Should().Be("i");

      str[new VMValue(new Slice(3))].AsString().Should().Be("str");
      str[new VMValue(new Slice(2, 5))].AsString().Should().Be("rin");
      str[new VMValue(new Slice(2, null))].AsString().Should().Be("ring");
      str[new VMValue(new Slice(-4, -1))].AsString().Should().Be("rin");
      str[new VMValue(new Slice(2, 5, 2))].AsString().Should().Be("rn");
      str[new VMValue(new Slice(null, null, 3))].AsString().Should().Be("si");
      str[new VMValue(new Slice(null, null, -1))].AsString().Should().Be("gnirts");
      str[new VMValue(new Slice(2, 8))].AsString().Should().Be("ring");
      str[new VMValue(new Slice(1, 5, -1))].AsString().Should().Be("");
      str[new VMValue(new Slice(10, 1, 1))].AsString().Should().Be("");
    }

    [Fact]
    public void TestListSubscript() {
      var list = new VMValue(new List<VMValue>{
        new VMValue(1),
        new VMValue(2),
        new VMValue(3),
        new VMValue(4),
        new VMValue(5),
      });
      list[new VMValue(0)].AsNumber().Should().Be(1);
      list[new VMValue(-5)].AsNumber().Should().Be(1);
      list[new VMValue(new Slice())].Should().Be(list);
      list[new VMValue(new Slice(3))].Should().Be(new VMValue(new List<VMValue> {
        new VMValue(1),
        new VMValue(2),
        new VMValue(3),
      }));
      list[new VMValue(new Slice(1, 3))].Should().Be(new VMValue(new List<VMValue> {
        new VMValue(2),
        new VMValue(3),
      }));
      list[new VMValue(new Slice(0, 4, 2))].Should().Be(new VMValue(new List<VMValue> {
        new VMValue(1),
        new VMValue(3),
      }));
      list[new VMValue(new Slice(-3))].Should().Be(new VMValue(new List<VMValue> {
        new VMValue(1),
        new VMValue(2),
      }));
      list[new VMValue(new Slice(-3, 4))].Should().Be(new VMValue(new List<VMValue> {
        new VMValue(3),
        new VMValue(4),
      }));
      list[new VMValue(new Slice(-3, 8, 2))].Should().Be(new VMValue(new List<VMValue> {
        new VMValue(3),
        new VMValue(5),
      }));
      list[new VMValue(new Slice(8, -3, -2))].Should().Be(new VMValue(new List<VMValue> {
        new VMValue(5),
      }));
      list[new VMValue(new Slice(8, -1, 2))].Should().Be(new VMValue(new List<VMValue>()));
    }

    [Fact]
    public void TestListSubscriptAssign() {
      var list = new VMValue(new List<VMValue>{
        new VMValue(1),
        new VMValue(2),
        new VMValue(3),
        new VMValue(4),
        new VMValue(5),
      });

      list[new VMValue(new Slice(1, 4))] = new VMValue(new List<VMValue> {
        new VMValue(1),
        new VMValue(2),
      });
      list.Should().Be(new VMValue(new List<VMValue> {
        new VMValue(1),
        new VMValue(1),
        new VMValue(2),
        new VMValue(5),
      }));

      list[new VMValue(new Slice(-3, -1))] = new VMValue(new List<VMValue> {
        new VMValue(2),
        new VMValue(3),
        new VMValue(4),
      });
      list.Should().Be(new VMValue(new List<VMValue> {
        new VMValue(1),
        new VMValue(2),
        new VMValue(3),
        new VMValue(4),
        new VMValue(5),
      }));

      list[new VMValue(new Slice(4, 2))] = new VMValue(new List<VMValue> {
        new VMValue(1),
        new VMValue(2),
        new VMValue(3),
      });
      list.Should().Be(new VMValue(new List<VMValue> {
        new VMValue(1),
        new VMValue(2),
        new VMValue(3),
        new VMValue(4),
        new VMValue(1),
        new VMValue(2),
        new VMValue(3),
        new VMValue(5),
      }));

      list[new VMValue(new Slice(-2, 3, -1))] = new VMValue(new List<VMValue> {
        new VMValue(1),
        new VMValue(2),
        new VMValue(3),
      });
      list.Should().Be(new VMValue(new List<VMValue> {
        new VMValue(1),
        new VMValue(2),
        new VMValue(3),
        new VMValue(4),
        new VMValue(3),
        new VMValue(2),
        new VMValue(1),
        new VMValue(5),
      }));

      list[new VMValue(new Slice(2, 10, 3))] = new VMValue(new List<VMValue> {
        new VMValue(5),
        new VMValue(4),
      });
      list.Should().Be(new VMValue(new List<VMValue> {
        new VMValue(1),
        new VMValue(2),
        new VMValue(5),
        new VMValue(4),
        new VMValue(3),
        new VMValue(4),
        new VMValue(1),
        new VMValue(5),
      }));
    }

    [Fact]
    public void TestTupleSubscript() {
      var tuple = new VMValue(ImmutableArray.Create(
        new VMValue(1),
        new VMValue(2),
        new VMValue(3),
        new VMValue(4),
        new VMValue(5)
      ));
      tuple[new VMValue(0)].AsNumber().Should().Be(1);
      tuple[new VMValue(-5)].AsNumber().Should().Be(1);
      tuple[new VMValue(new Slice())].Should().Be(tuple);
      tuple[new VMValue(new Slice(3))].Should().Be(new VMValue(ImmutableArray.Create(
        new VMValue(1),
        new VMValue(2),
        new VMValue(3)
      )));
      tuple[new VMValue(new Slice(1, 3))].Should().Be(new VMValue(ImmutableArray.Create(
        new VMValue(2),
        new VMValue(3)
      )));
      tuple[new VMValue(new Slice(0, 4, 2))].Should().Be(new VMValue(ImmutableArray.Create(
        new VMValue(1),
        new VMValue(3)
      )));
      tuple[new VMValue(new Slice(5, null, null))].Should().Be(
          new VMValue(ImmutableArray.Create<VMValue>()));
    }

    [Fact]
    public void TestFloatSubscript() {
      var list = new VMValue(new List<VMValue>{
        new VMValue(1),
        new VMValue(2),
        new VMValue(3),
        new VMValue(4),
        new VMValue(5),
      });
      Action action = () => _ = list[new VMValue(0.5)];
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorInvalidIntIndex);

      action = () => _ = list[new VMValue(new Slice(0.5))];
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorInvalidIntIndex);
      action = () => _ = list[new VMValue(new Slice(null, 0.5))];
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorInvalidIntIndex);
      action = () => _ = list[new VMValue(new Slice(null, null, 0.5))];
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.RuntimeErrorInvalidIntIndex);
    }

    [Fact]
    public void TestValueWithReferenceCycle() {
      var a = new VMValue(new List<VMValue>() {
        new VMValue(1),
        new VMValue(2),
      });
      var b = new VMValue(new List<VMValue>() { a });
      a[new VMValue(1)] = b;
      a.ToString().Should().Be("[1, [[...]]]");
      b.ToString().Should().Be("[[1, [...]]]");

      a = new VMValue(new Dictionary<VMValue, VMValue>());
      b = new VMValue(new Dictionary<VMValue, VMValue>() {
        [new VMValue("a")] = a,
      });
      a[new VMValue("b")] = b;
      a.ToString().Should().Be("{'b': {'a': {...}}}");
      b.ToString().Should().Be("{'a': {'b': {...}}}");

      a = new VMValue(new List<VMValue>() {
        new VMValue(1),
        new VMValue(2),
      });
      b = new VMValue(new Dictionary<VMValue, VMValue>() {
        [new VMValue("a")] = a,
      });
      a[new VMValue(1)] = b;
      a.ToString().Should().Be("[1, {'a': [...]}]");
      b.ToString().Should().Be("{'a': [1, {...}]}");
    }
  }
}
