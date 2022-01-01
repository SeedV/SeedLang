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

using BenchmarkDotNet.Attributes;
using SeedLang.Ast;
using SeedLang.Common;
using SeedLang.Interpreter;
using SeedLang.Runtime;

namespace SeedLang.Benchmark {
  public class BinaryExpressionBenchmark {
    private readonly VisualizerCenter _visualizerCenter = new VisualizerCenter();
    private readonly ExpressionStatement _expressionStatement;
    private readonly Ast.Executor _executor;
    private readonly Chunk _chunk;
    private readonly VM _vm;

    public BinaryExpressionBenchmark() {
      var left = Expression.NumberConstant(1, NewTextRange());
      var right = Expression.NumberConstant(2, NewTextRange());
      var binary = Expression.Binary(left, BinaryOperator.Add, right, NewTextRange());
      _expressionStatement = Statement.Expression(binary, NewTextRange());

      _executor = new Ast.Executor(_visualizerCenter);

      var compiler = new Compiler();
      _chunk = compiler.Compile(_expressionStatement);
      _vm = new VM(_visualizerCenter);
    }

    // Benchmarks binary expression running time of the AST executor.
    [Benchmark]
    public void BenchmarkAst() {
      _executor.Run(_expressionStatement);
    }

    // Benchmarks binary expression running time of the VM. Compiling time is not included.
    [Benchmark]
    public void BenchmarkBytecode() {
      _vm.Run(_chunk);
    }

    private static TextRange NewTextRange() {
      return new TextRange(0, 1, 2, 3);
    }
  }
}
