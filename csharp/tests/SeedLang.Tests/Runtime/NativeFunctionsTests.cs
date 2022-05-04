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
using System.IO;
using Xunit;

namespace SeedLang.Runtime.Tests {
  using Range = HeapObject.Range;
  using NativeFunction = HeapObject.NativeFunction;

  public class NativeFunctionsTests {
    [Fact]
    public void TestEmptyList() {
      var listFunc = Function(NativeFunctions.List);
      VMValue list = listFunc.Call(Array.Empty<VMValue>(), 0, 0, null);
      Assert.True(list.IsList);
      Assert.Equal(0, list.Length);
    }

    [Fact]
    public void TestListWithListArgument() {
      var listFunc = Function(NativeFunctions.List);
      var args = new VMValue[] {
        new VMValue(new List<VMValue>() {
          new VMValue(1),
          new VMValue(2)
        })
      };
      VMValue list = listFunc.Call(args, 0, args.Length, null);
      Assert.True(list.IsList);
      Assert.Equal(2, list.Length);
      Assert.Equal(1, list[new VMValue(0)].AsNumber());
      Assert.Equal(2, list[new VMValue(1)].AsNumber());
    }

    [Fact]
    public void TestListWithRangeArgument() {
      var listFunc = Function(NativeFunctions.List);
      var length = 10;
      var args = new VMValue[] { new VMValue(new Range(length)) };
      VMValue list = listFunc.Call(args, 0, args.Length, null);
      Assert.True(list.IsList);
      Assert.Equal(length, list.Length);
      for (int i = 0; i < length; i++) {
        Assert.Equal(i, list[new VMValue(i)].AsNumber());
      }
    }

    [Fact]
    public void TestPrint() {
      var sys = new Sys() { Stdout = new StringWriter() };
      var print = Function(NativeFunctions.Print);
      var args = new VMValue[] { new VMValue(1), new VMValue(2), new VMValue(3) };
      print.Call(args, 0, args.Length, sys);
      var expected = "1 2 3" + Environment.NewLine;
      Assert.Equal(expected, sys.Stdout.ToString());
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
