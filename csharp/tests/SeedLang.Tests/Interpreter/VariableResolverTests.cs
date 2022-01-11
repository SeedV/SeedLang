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

namespace SeedLang.Interpreter.Tests {
  using VariableInfo = VariableResolver.VariableInfo;
  using VariableType = VariableResolver.VariableType;

  public class VariableResolverTests {
    [Fact]
    public void TestFunctionScope() {
      var env = new GlobalEnvironment();
      string a = "a";
      string b = "b";
      Assert.Equal(0u, env.DefineVariable(a));
      var resolver = new VariableResolver(env);
      Assert.Equal(new VariableInfo(VariableType.Global, 0), resolver.FindVariable(a));
      Assert.Equal(new VariableInfo(VariableType.Global, 1), resolver.DefineVariable(b));
      Assert.Equal(new VariableInfo(VariableType.Global, 1), resolver.FindVariable(b));

      string local = "local";
      resolver.BeginFunctionScope();
      Assert.Equal(new VariableInfo(VariableType.Local, 0), resolver.DefineVariable(local));
      Assert.Equal(new VariableInfo(VariableType.Local, 0), resolver.FindVariable(local));
      Assert.Equal(new VariableInfo(VariableType.Global, 0), resolver.FindVariable(a));
      Assert.Equal(new VariableInfo(VariableType.Global, 1), resolver.FindVariable(b));

      Assert.Equal(new VariableInfo(VariableType.Local, 1), resolver.DefineVariable(a));
      Assert.Equal(new VariableInfo(VariableType.Local, 1), resolver.FindVariable(a));

      resolver.BeginFunctionScope();
      Assert.Equal(new VariableInfo(VariableType.Local, 0), resolver.DefineVariable(a));
      Assert.Equal(new VariableInfo(VariableType.Local, 0), resolver.FindVariable(a));
      Assert.Equal(new VariableInfo(VariableType.Local, 1), resolver.DefineVariable(local));
      Assert.Equal(new VariableInfo(VariableType.Local, 1), resolver.FindVariable(local));
      resolver.EndFunctionScope();

      resolver.EndFunctionScope();
      Assert.Null(resolver.FindVariable(local));
      Assert.Equal(new VariableInfo(VariableType.Global, 0), resolver.FindVariable(a));
      Assert.Equal(new VariableInfo(VariableType.Global, 1), resolver.FindVariable(b));
    }
  }
}
