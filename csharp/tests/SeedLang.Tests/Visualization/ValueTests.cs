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
using SeedLang.Runtime;
using Xunit;

namespace SeedLang.Visualization.Tests {
  public class ValueTests {
    [Fact]
    public void TestValues() {
      var nil = new Value(new VMValue());
      Assert.True(nil.IsNil);
      Assert.Equal("None", nil.ToString());

      var boolean = new Value(new VMValue(true));
      Assert.True(boolean.IsBoolean);
      Assert.True(boolean.AsBoolean());
      Assert.Equal("True", boolean.ToString());

      var number = new Value(new VMValue(1));
      Assert.True(number.IsNumber);
      Assert.Equal(1, number.AsNumber());
      Assert.Equal("1", number.ToString());

      string rawStr = "text";
      var str = new Value(new VMValue(rawStr));
      Assert.True(str.IsString);
      Assert.Equal(rawStr, str.AsString());
      Assert.Equal($"'{rawStr}'", str.ToString());
    }

    [Fact]
    public void TestContainerValues() {
      var rawDict = new Dictionary<VMValue, VMValue> {
        [new VMValue(1)] = new VMValue(100),
        [new VMValue(2)] = new VMValue(200),
      };
      var dict = new Value(new VMValue(rawDict));
      Assert.True(dict.IsDict);
      Assert.Equal(new Value(100), dict[new Value(1)]);
      Assert.Equal(new Value(200), dict[new Value(2)]);

      var rawList = new List<VMValue> {
        new VMValue(1),
        new VMValue(2),
        new VMValue(3)
      };
      var list = new Value(new VMValue(rawList));
      Assert.True(list.IsList);
      Assert.Equal(new Value(1), list[new Value(0)]);
      Assert.Equal(new Value(2), list[new Value(1)]);
      Assert.Equal(new Value(3), list[new Value(2)]);

      var rawTuple = ImmutableArray.Create(new VMValue(1),
                                           new VMValue(2),
                                           new VMValue(3));
      var tuple = new Value(new VMValue(rawTuple));
      Assert.True(tuple.IsTuple);
      Assert.Equal(new Value(1), tuple[new Value(0)]);
      Assert.Equal(new Value(2), tuple[new Value(1)]);
      Assert.Equal(new Value(3), tuple[new Value(2)]);
    }
  }
}
