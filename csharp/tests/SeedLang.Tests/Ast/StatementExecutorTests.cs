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
using SeedLang.Common;
using SeedLang.Runtime;
using SeedLang.Tests.Helper;
using Xunit;

namespace SeedLang.Ast {
  public class StatementExecutorTests {
    private class MockupEnvironment : IEnvironment {
      public bool TryGetValueOfVariable(string name, out VMValue value) {
        value = new VMValue();
        return true;
      }
    }

    [Fact]
    public void TestExpression() {
      var executor = new StatementExecutor(new MockupEnvironment());
      var expression = AstHelper.ExpressionStmt(AstHelper.NumberConstant(1));
      var result = new ExpressionExecutor.Result();
      executor.Visit(expression, result);
      result.Value.Should().Be(new VMValue(1));
    }

    [Fact]
    public void TestUnsupportedSyntax() {
      var executor = new StatementExecutor(new MockupEnvironment());
      var result = new ExpressionExecutor.Result();

      Statement statement = AstHelper.Assign(
        AstHelper.ChainedTargets(AstHelper.Targets(AstHelper.Id("a"))),
        AstHelper.NumberConstant(1)
      );
      Action action = () => executor.Visit(statement, result);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.UnsupportedEvalSyntax);

      statement = AstHelper.Block();
      action = () => executor.Visit(statement, result);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.UnsupportedEvalSyntax);

      statement = AstHelper.Break();
      action = () => executor.Visit(statement, result);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.UnsupportedEvalSyntax);

      statement = AstHelper.Continue();
      action = () => executor.Visit(statement, result);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.UnsupportedEvalSyntax);

      statement = AstHelper.ForIn(AstHelper.Id("i"),
                                  AstHelper.BooleanConstant(false),
                                  AstHelper.Block());
      action = () => executor.Visit(statement, result);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.UnsupportedEvalSyntax);

      statement = AstHelper.FuncDef("func", AstHelper.Params(AstHelper.Id("a")), AstHelper.Block());
      action = () => executor.Visit(statement, result);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.UnsupportedEvalSyntax);

      statement = AstHelper.If(AstHelper.BooleanConstant(false),
                               AstHelper.Block(),
                               AstHelper.Block());
      action = () => executor.Visit(statement, result);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.UnsupportedEvalSyntax);

      statement = AstHelper.Pass();
      action = () => executor.Visit(statement, result);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.UnsupportedEvalSyntax);

      statement = AstHelper.Return();
      action = () => executor.Visit(statement, result);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.UnsupportedEvalSyntax);

      statement = AstHelper.VTag(null);
      action = () => executor.Visit(statement, result);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.UnsupportedEvalSyntax);

      statement = AstHelper.While(AstHelper.BooleanConstant(false), AstHelper.Block());
      action = () => executor.Visit(statement, result);
      action.Should().Throw<DiagnosticException>().Where(
          ex => ex.Diagnostic.MessageId == Message.UnsupportedEvalSyntax);
    }
  }
}
