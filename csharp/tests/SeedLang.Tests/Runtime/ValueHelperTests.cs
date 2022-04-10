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

using System.Collections.Generic;
using System.Collections.Immutable;
using SeedLang.Common;
using Xunit;

namespace SeedLang.Runtime.Tests {
  public class ValueHelperTests {
    [Fact]
    public void TestContains() {
      string a = "a";
      var dict = new Value(new Dictionary<Value, Value> {
        [new Value(1)] = new Value(1),
        [new Value(a)] = new Value(2),
      });
      Assert.True(ValueHelper.Contains(dict, new Value(1)));
      Assert.True(ValueHelper.Contains(dict, new Value(a)));
      Assert.False(ValueHelper.Contains(dict, new Value()));

      var list = new Value(new List<Value>() { new Value(1), new Value(a) });
      Assert.True(ValueHelper.Contains(list, new Value(1)));
      Assert.True(ValueHelper.Contains(list, new Value(a)));
      Assert.False(ValueHelper.Contains(list, new Value()));

      var tuple = new Value(ImmutableArray.Create(new Value(1), new Value(a)));
      Assert.True(ValueHelper.Contains(tuple, new Value(1)));
      Assert.True(ValueHelper.Contains(tuple, new Value(a)));
      Assert.False(ValueHelper.Contains(tuple, new Value()));

      var str = new Value("Hello World");
      Assert.True(ValueHelper.Contains(str, new Value("o")));
      Assert.True(ValueHelper.Contains(str, new Value(" ")));
      Assert.True(ValueHelper.Contains(str, new Value("Hello")));
      Assert.False(ValueHelper.Contains(str, new Value("HW")));
      var exception = Assert.Throws<DiagnosticException>(() => {
        ValueHelper.Contains(str, new Value());
      });
      Assert.Equal(Message.RuntimeErrorUnsupportedOperads, exception.Diagnostic.MessageId);
    }

    [Fact]
    public void TestDivide() {
      Assert.Equal(2.5, ValueHelper.Divide(new Value(5), new Value(2)).AsNumber());
      Assert.Throws<DiagnosticException>(() => ValueHelper.Divide(new Value(5), new Value(0)));
    }

    [Fact]
    public void TestFloorDivide() {
      Assert.Equal(2, ValueHelper.FloorDivide(new Value(5), new Value(2)).AsNumber());
      Assert.Throws<DiagnosticException>(() => ValueHelper.FloorDivide(new Value(5), new Value(0)));
    }

    [Fact]
    public void TestModulo() {
      Assert.Equal(1, ValueHelper.Modulo(new Value(5), new Value(2)).AsNumber());
      Assert.Throws<DiagnosticException>(() => ValueHelper.Modulo(new Value(5), new Value(0)));
    }

    [Fact]
    public void TestUnescape() {
      Assert.Equal("\"string\"", ValueHelper.Unescape("\\\"string\\\""));
      Assert.Equal("str'ing", ValueHelper.Unescape("str\\'ing"));
      Assert.Equal("s\ntr\ring", ValueHelper.Unescape("s\\ntr\\ring"));
      Assert.Equal("s\ttr\bing\f", ValueHelper.Unescape("s\\ttr\\bing\\f"));
    }
  }
}
