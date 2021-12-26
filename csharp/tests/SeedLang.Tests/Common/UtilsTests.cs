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

using Xunit;

namespace SeedLang.Common.Tests {
  public class UtilsTests {
    [Fact]
    public void TestTimestamp() {
      var time = new System.DateTime(1234, 1, 2, 3, 4, 5, 6);
      Assert.Equal("12340102030405006", Utils.Timestamp(time));
    }
  }
}
