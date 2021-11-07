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
  public class ValuesTests {
    [Fact]
    public void TestBooleanValue() {
      var boolean = new BooleanValue();
      Assert.False(boolean.Boolean);
      Assert.Equal(0, boolean.Number);
      Assert.Equal("False", boolean.String);
    }
  }
}
