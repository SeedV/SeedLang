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

using System;
using FluentAssertions;
using SeedLang.Runtime.HeapObjects;
using Xunit;

namespace SeedLang.Interpreter.Tests {
  public class VariableResolverTests {
    [Fact]
    public void TestScopes() {
      var env = new GlobalEnvironment(Array.Empty<NativeFunction>());
      string a = "a";
      string b = "b";
      env.DefineVariable(a).Should().Be(0);

      var resolver = new VariableResolver(env);
      var expectedA = new VariableInfo(VariableInfo.VarType.Global, 0, "global.a");
      var expectedB = new VariableInfo(VariableInfo.VarType.Global, 1, "global.b");
      resolver.FindVariable(a).Should().BeEquivalentTo(expectedA);
      resolver.FindVariable(b).Should().BeNull();
      resolver.DefineVariable(b).Should().BeEquivalentTo(expectedB);
      resolver.FindVariable(b).Should().BeEquivalentTo(expectedB);

      resolver.DefineTempVariable().Should().Be(0);

      resolver.BeginFuncScope("foo");
      string local = "local";
      resolver.FindVariable(local).Should().BeNull();
      var expectedFooLocal = new VariableInfo(VariableInfo.VarType.Local, 0, "global.foo.local");
      resolver.DefineVariable(local).Should().BeEquivalentTo(expectedFooLocal);
      resolver.FindVariable(local).Should().BeEquivalentTo(expectedFooLocal);

      resolver.FindVariable(a).Should().BeEquivalentTo(expectedA);
      resolver.FindVariable(b).Should().BeEquivalentTo(expectedB);
      var expectedFooA = new VariableInfo(VariableInfo.VarType.Local, 1, "global.foo.a");
      resolver.DefineVariable(a).Should().BeEquivalentTo(expectedFooA);

      resolver.DefineTempVariable().Should().Be(2);

      resolver.BeginFuncScope("bar");
      var expectedBarA = new VariableInfo(VariableInfo.VarType.Local, 0, "global.foo.bar.a");
      resolver.DefineVariable(a).Should().BeEquivalentTo(expectedBarA);
      resolver.FindVariable(a).Should().BeEquivalentTo(expectedBarA);
      var expectedBarLocal = new VariableInfo(VariableInfo.VarType.Local, 1,
                                              "global.foo.bar.local");
      resolver.DefineVariable(local).Should().BeEquivalentTo(expectedBarLocal);
      resolver.FindVariable(local).Should().BeEquivalentTo(expectedBarLocal);
      resolver.EndFuncScope();

      resolver.BeginExprScope();
      resolver.DefineTempVariable().Should().Be(3);
      resolver.EndExprScope();

      resolver.DefineTempVariable().Should().Be(3);
      resolver.EndFuncScope();

      resolver.FindVariable(local).Should().BeNull();
      resolver.FindVariable(a).Should().BeEquivalentTo(expectedA);
      resolver.FindVariable(b).Should().BeEquivalentTo(expectedB);
    }
  }
}
