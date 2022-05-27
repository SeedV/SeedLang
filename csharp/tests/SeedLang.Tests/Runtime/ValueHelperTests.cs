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
      var dict = new VMValue(new Dictionary<VMValue, VMValue> {
        [new VMValue(1)] = new VMValue(1),
        [new VMValue(a)] = new VMValue(2),
      });
      Assert.True(ValueHelper.Contains(dict, new VMValue(1)));
      Assert.True(ValueHelper.Contains(dict, new VMValue(a)));
      Assert.False(ValueHelper.Contains(dict, new VMValue()));

      var list = new VMValue(new List<VMValue>() { new VMValue(1), new VMValue(a) });
      Assert.True(ValueHelper.Contains(list, new VMValue(1)));
      Assert.True(ValueHelper.Contains(list, new VMValue(a)));
      Assert.False(ValueHelper.Contains(list, new VMValue()));

      var tuple = new VMValue(ImmutableArray.Create(new VMValue(1), new VMValue(a)));
      Assert.True(ValueHelper.Contains(tuple, new VMValue(1)));
      Assert.True(ValueHelper.Contains(tuple, new VMValue(a)));
      Assert.False(ValueHelper.Contains(tuple, new VMValue()));

      var str = new VMValue("Hello World");
      Assert.True(ValueHelper.Contains(str, new VMValue("o")));
      Assert.True(ValueHelper.Contains(str, new VMValue(" ")));
      Assert.True(ValueHelper.Contains(str, new VMValue("Hello")));
      Assert.False(ValueHelper.Contains(str, new VMValue("HW")));
      var exception = Assert.Throws<DiagnosticException>(() => {
        ValueHelper.Contains(str, new VMValue());
      });
      Assert.Equal(Message.RuntimeErrorUnsupportedOperands, exception.Diagnostic.MessageId);
    }

    [Fact]
    public void TestDivide() {
      Assert.Equal(2.5, ValueHelper.Divide(new VMValue(5), new VMValue(2)).AsNumber());
      Assert.Throws<DiagnosticException>(() => ValueHelper.Divide(new VMValue(5), new VMValue(0)));
    }

    [Fact]
    public void TestFloorDivide() {
      Assert.Equal(2, ValueHelper.FloorDivide(new VMValue(5), new VMValue(2)).AsNumber());
      Assert.Throws<DiagnosticException>(() => ValueHelper.FloorDivide(new VMValue(5),
                                                                       new VMValue(0)));
    }

    [Fact]
    public void TestModulo() {
      Assert.Equal(1, ValueHelper.Modulo(new VMValue(5), new VMValue(2)).AsNumber());
      Assert.Throws<DiagnosticException>(() => ValueHelper.Modulo(new VMValue(5), new VMValue(0)));
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
