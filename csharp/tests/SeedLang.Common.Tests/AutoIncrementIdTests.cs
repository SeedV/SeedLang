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

using System.Collections.Generic;
using Xunit;

namespace SeedLang.Common.Tests {
  public class AutoIncrementIdTests {
    [Fact]
    public void TestIdSequence() {
      var generator = new AutoIncrementId();
      Assert.Equal(0, generator.NextInt());
      Assert.Equal(1, generator.NextInt());
      Assert.Equal(2, generator.NextInt());
      Assert.Equal(3, generator.NextInt());
      Assert.Equal(4, generator.NextInt());
      generator.Reset();
      Assert.Equal("000", generator.NextString(3));
      Assert.Equal("001", generator.NextString(3));
      Assert.Equal("002", generator.NextString(3));
      Assert.Equal("003", generator.NextString(3));
      Assert.Equal("004", generator.NextString(3));
      generator.Reset(999);
      Assert.Equal("1000", generator.NextString(3));
      Assert.Equal("1001", generator.NextString(4));
      Assert.Equal("01002", generator.NextString(5));
      Assert.Equal("001003", generator.NextString(6));
      Assert.Equal("0001004", generator.NextString(7));
    }
  }

  // The following classes leverage XUnit's parallelism feature to test the auto-increment ID
  // generator in a multi-threaded environment.
  //
  // See https://xunit.net/docs/running-tests-in-parallel
  static class ParallelContext {
    private static readonly object _lock = new object();
    private static readonly HashSet<string> _idSet = new HashSet<string>();
    private static readonly AutoIncrementId _generator = new AutoIncrementId();

    public static void GenerateIds() {
      for (int i = 0; i < 1000; i++) {
        string id = _generator.NextString(8);
        lock (_lock) {
          Assert.True(_idSet.Add(id));
        }
      }
    }
  }

  [Collection("ParallelTestCollection#1")]
  public class ParallelAutoIncrementIdTests1 {
    [Fact]
    public void TestParallelism() {
      ParallelContext.GenerateIds();
    }
  }

  [Collection("ParallelTestCollection#2")]
  public class ParallelAutoIncrementIdTests2 {
    [Fact]
    public void TestParallelism() {
      ParallelContext.GenerateIds();
    }
  }

  [Collection("ParallelTestCollection#3")]
  public class ParallelAutoIncrementIdTests3 {
    [Fact]
    public void TestParallelism() {
      ParallelContext.GenerateIds();
    }
  }

  [Collection("ParallelTestCollection#4")]
  public class ParallelAutoIncrementIdTests4 {
    [Fact]
    public void TestParallelism() {
      ParallelContext.GenerateIds();
    }
  }

  [Collection("ParallelTestCollection#5")]
  public class ParallelAutoIncrementIdTests5 {
    [Fact]
    public void TestParallelism() {
      ParallelContext.GenerateIds();
    }
  }
}
