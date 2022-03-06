using System.IO;
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

using SeedLang.Common;
using Xunit;

namespace SeedLang.Runtime.Tests {
  public class ValueHelperTests {
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
