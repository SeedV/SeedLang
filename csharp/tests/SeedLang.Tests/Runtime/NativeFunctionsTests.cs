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
using System.Collections.Generic;
using Xunit;

namespace SeedLang.Runtime.Tests {
  using Range = HeapObject.Range;
  using NativeFunction = HeapObject.NativeFunction;

  public class NativeFunctionsTests {
    [Fact]
    public void TestEmptyList() {
      var listFunc = Function("list");
      Value list = listFunc.Call(Array.Empty<Value>(), 0, 0);
      Assert.True(list.IsList);
      Assert.Equal(0, list.Length);
    }

    [Fact]
    public void TestListOfList() {
      var listFunc = Function("list");
      var args = new Value[] {
        new Value(new List<Value>() {
          new Value(1),
          new Value(2)
        })
      };
      Value list = listFunc.Call(args, 0, args.Length);
      Assert.True(list.IsList);
      Assert.Equal(2, list.Length);
      Assert.Equal(1, list[0].AsNumber());
      Assert.Equal(2, list[1].AsNumber());
    }

    [Fact]
    public void TestListOfRange() {
      var listFunc = Function("list");
      var length = 10;
      var args = new Value[] { new Value(new Range(length)) };
      Value list = listFunc.Call(args, 0, args.Length);
      Assert.True(list.IsList);
      Assert.Equal(length, list.Length);
      for (int i = 0; i < length; i++) {
        Assert.Equal(i, list[i].AsNumber());
      }
    }

    private static NativeFunction Function(string name) {
      foreach (NativeFunction func in NativeFunctions.Funcs) {
        if (func.Name == name) {
          return func;
        }
      }
      throw new ArgumentException($"Function {name} does not exist.");
    }
  }
}
