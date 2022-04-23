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

namespace SeedLang.Ast {
  // An abstract base class to traverse a statement tree.
  internal abstract class StatementWalker {
    // Dispatches to the correspoding visit method based on the type of the statement node.
    internal void Visit(Statement statement) {
      Enter(statement);
      switch (statement) {
        case AssignmentStatement assignment:
          VisitAssignment(assignment);
          break;
        case BlockStatement block:
          VisitBlock(block);
          break;
        case BreakStatement @break:
          VisitBreak(@break);
          break;
        case ContinueStatement @continue:
          VisitContinue(@continue);
          break;
        case ExpressionStatement expr:
          VisitExpression(expr);
          break;
        case ForInStatement forIn:
          VisitForIn(forIn);
          break;
        case FuncDefStatement funcDef:
          VisitFuncDef(funcDef);
          break;
        case IfStatement @if:
          VisitIf(@if);
          break;
        case PassStatement pass:
          VisitPass(pass);
          break;
        case ReturnStatement @return:
          VisitReturn(@return);
          break;
        case WhileStatement @while:
          VisitWhile(@while);
          break;
        case VTagStatement vTag:
          VisitVTag(vTag);
          break;
        default:
          throw new NotImplementedException(
              $"Not implemented statement type: {statement.GetType()}");
      }
      Exit(statement);
    }

    protected virtual void Enter(Statement statement) { }
    protected virtual void Exit(Statement statement) { }

    protected abstract void VisitAssignment(AssignmentStatement assignment);
    protected abstract void VisitBlock(BlockStatement block);
    protected abstract void VisitBreak(BreakStatement @break);
    protected abstract void VisitContinue(ContinueStatement @continue);
    protected abstract void VisitExpression(ExpressionStatement expr);
    protected abstract void VisitForIn(ForInStatement forIn);
    protected abstract void VisitFuncDef(FuncDefStatement funcDef);
    protected abstract void VisitIf(IfStatement @if);
    protected abstract void VisitPass(PassStatement pass);
    protected abstract void VisitReturn(ReturnStatement @return);
    protected abstract void VisitVTag(VTagStatement vTag);
    protected abstract void VisitWhile(WhileStatement @while);
  }
}
