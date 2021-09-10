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

namespace SeedLang.Interpreter.Tests {
  public class ChunkTests {
    [Fact]
    public void TestEmit() {
      var chunk = new Chunk();
      chunk.AddConstant(1);
      chunk.Emit(Opcode.RETURN, 0);
      chunk.Emit(Opcode.ADD, 1, 2, 3);
      chunk.Emit(Opcode.LOADK, 1, 0);
      string expected = "RETURN 0            \n" +
                        "ADD 1 2 3           \n" +
                        "LOADK 1 0           ; 1\n";
      Assert.Equal(expected, chunk.ToString());
    }
  }
}
