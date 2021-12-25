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

using SeedLang.Runtime;
using Xunit;

namespace SeedLang.Ast.Tests {
  public class EnvironmentTests {
    [Fact]
    public void TestEnvironment() {
      var env = new Environment();
      env.SetVariable("a", Value.Number(1));
      Assert.True(env.ContainsVariable("a"));
      Assert.False(env.ContainsVariable("b"));
      Assert.Equal(Value.Number(1), env.GetVariable("a"));

      env.EnterScope();
      env.SetVariable("a", Value.Number(2));
      Assert.True(env.ContainsVariable("a"));
      Assert.False(env.ContainsVariable("b"));
      Assert.Equal(Value.Number(2), env.GetVariable("a"));

      env.ExitScope();
      Assert.True(env.ContainsVariable("a"));
      Assert.False(env.ContainsVariable("b"));
      Assert.Equal(Value.Number(1), env.GetVariable("a"));
    }
  }
}
