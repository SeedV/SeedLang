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
  //
  // All node types in the AST tree are either Expression or Statement. So the separated
  // ExpressionWalker and StatementWalker classes are enough to visit all nodes in AST trees.
  internal abstract class StatementWalker<Param> {
    // Dispatches to the corresponding visit method based on the type of the statement node.
    internal void Visit(Statement statement, Param param) {
      Enter(statement, param);
      switch (statement) {
        case AssignmentStatement assignment:
          VisitAssignment(assignment, param);
          break;
        case BlockStatement block:
          VisitBlock(block, param);
          break;
        case BreakStatement @break:
          VisitBreak(@break, param);
          break;
        case ContinueStatement @continue:
          VisitContinue(@continue, param);
          break;
        case ExpressionStatement expr:
          VisitExpression(expr, param);
          break;
        case ForInStatement forIn:
          VisitForIn(forIn, param);
          break;
        case FuncDefStatement funcDef:
          VisitFuncDef(funcDef, param);
          break;
        case IfStatement @if:
          VisitIf(@if, param);
          break;
        case ImportStatement import:
          VisitImport(import, param);
          break;
        case PassStatement pass:
          VisitPass(pass, param);
          break;
        case ReturnStatement @return:
          VisitReturn(@return, param);
          break;
        case WhileStatement @while:
          VisitWhile(@while, param);
          break;
        case VTagStatement vTag:
          VisitVTag(vTag, param);
          break;
        default:
          throw new NotImplementedException(
              $"Not implemented statement type: {statement.GetType()}");
      }
      Exit(statement, param);
    }

    protected virtual void Enter(Statement statement, Param param) { }
    protected virtual void Exit(Statement statement, Param param) { }

    protected abstract void VisitAssignment(AssignmentStatement assignment, Param param);
    protected abstract void VisitBlock(BlockStatement block, Param param);
    protected abstract void VisitBreak(BreakStatement @break, Param param);
    protected abstract void VisitContinue(ContinueStatement @continue, Param param);
    protected abstract void VisitExpression(ExpressionStatement expr, Param param);
    protected abstract void VisitForIn(ForInStatement forIn, Param param);
    protected abstract void VisitFuncDef(FuncDefStatement funcDef, Param param);
    protected abstract void VisitIf(IfStatement @if, Param param);
    protected abstract void VisitImport(ImportStatement import, Param param);
    protected abstract void VisitPass(PassStatement pass, Param param);
    protected abstract void VisitReturn(ReturnStatement @return, Param param);
    protected abstract void VisitVTag(VTagStatement vTag, Param param);
    protected abstract void VisitWhile(WhileStatement @while, Param param);
  }
}
