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

namespace SeedLang.Ast {
  using Result = ExpressionExecutor.Result;

  internal class StatementExecutor : StatementWalker<Result> {
    private readonly IEnvironment _variables;

    internal StatementExecutor(IEnvironment variables) {
      _variables = variables;
    }

    protected override void VisitAssignment(AssignmentStatement assignment, Result result) {
      throw new System.NotImplementedException();
    }

    protected override void VisitBlock(BlockStatement block, Result result) {
      throw new System.NotImplementedException();
    }

    protected override void VisitBreak(BreakStatement @break, Result result) {
      throw new System.NotImplementedException();
    }

    protected override void VisitContinue(ContinueStatement @continue, Result result) {
      throw new System.NotImplementedException();
    }

    protected override void VisitExpression(ExpressionStatement expr, Result result) {
      new ExpressionExecutor(_variables).Visit(expr.Expr, result);
    }

    protected override void VisitForIn(ForInStatement forIn, Result result) {
      throw new System.NotImplementedException();
    }

    protected override void VisitFuncDef(FuncDefStatement funcDef, Result result) {
      throw new System.NotImplementedException();
    }

    protected override void VisitIf(IfStatement @if, Result result) {
      throw new System.NotImplementedException();
    }

    protected override void VisitPass(PassStatement pass, Result result) {
      throw new System.NotImplementedException();
    }

    protected override void VisitReturn(ReturnStatement @return, Result result) {
      throw new System.NotImplementedException();
    }

    protected override void VisitVTag(VTagStatement vTag, Result result) {
      throw new System.NotImplementedException();
    }

    protected override void VisitWhile(WhileStatement @while, Result result) {
      throw new System.NotImplementedException();
    }
  }
}
