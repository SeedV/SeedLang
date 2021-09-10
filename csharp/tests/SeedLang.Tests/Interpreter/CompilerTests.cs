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

using System;
using SeedLang.Ast;
using Xunit;

namespace SeedLang.Interpreter.Tests {
  public class CompilerTests {
    [Fact]
    public void TestCompile() {
      var eval = Statement.Eval(Expression.Number(1));
      var compiler = new Compiler();
      var chunk = compiler.Compile(eval);
      string expected = "LOADK 0 0           ; 1" + Environment.NewLine +
                        "EVAL 0              " + Environment.NewLine +
                        "RETURN 0            " + Environment.NewLine;
      Assert.Equal(expected, chunk.ToString());
    }
  }
}
