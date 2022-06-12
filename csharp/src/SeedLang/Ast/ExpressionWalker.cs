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
  // An abstract base class to traverse an expression tree.
  //
  // All node types in the AST tree are either Expression or Statement. So the separated
  // ExpressionWalker and StatementWalker classes are enough to visit all nodes in AST trees.
  internal abstract class ExpressionWalker<Param> {
    // Dispatches to the corresponding visit method based on the type of the expression node.
    internal void Visit(Expression expression, Param param) {
      Enter(expression, param);
      switch (expression) {
        case BinaryExpression binary:
          VisitBinary(binary, param);
          break;
        case BooleanExpression boolean:
          VisitBoolean(boolean, param);
          break;
        case BooleanConstantExpression booleanConstant:
          VisitBooleanConstant(booleanConstant, param);
          break;
        case CallExpression call:
          VisitCall(call, param);
          break;
        case ComparisonExpression comparison:
          VisitComparison(comparison, param);
          break;
        case DictExpression dict:
          VisitDict(dict, param);
          break;
        case IdentifierExpression identifier:
          VisitIdentifier(identifier, param);
          break;
        case ListExpression list:
          VisitList(list, param);
          break;
        case NilConstantExpression nilConstant:
          VisitNilConstant(nilConstant, param);
          break;
        case NumberConstantExpression numberConstant:
          VisitNumberConstant(numberConstant, param);
          break;
        case SliceExpression slice:
          VisitSlice(slice, param);
          break;
        case StringConstantExpression stringConstant:
          VisitStringConstant(stringConstant, param);
          break;
        case SubscriptExpression subscript:
          VisitSubscript(subscript, param);
          break;
        case TupleExpression tuple:
          VisitTuple(tuple, param);
          break;
        case UnaryExpression unary:
          VisitUnary(unary, param);
          break;
        default:
          throw new NotImplementedException(
              $"Not implemented expression type: {expression.GetType()}");
      }
      Exit(expression, param);
    }

    protected virtual void Enter(Expression expr, Param param) { }
    protected virtual void Exit(Expression expr, Param param) { }

    protected abstract void VisitBinary(BinaryExpression binary, Param param);
    protected abstract void VisitBoolean(BooleanExpression boolean, Param param);
    protected abstract void VisitBooleanConstant(BooleanConstantExpression booleanConstant,
                                                 Param param);
    protected abstract void VisitCall(CallExpression call, Param param);
    protected abstract void VisitComparison(ComparisonExpression comparison, Param param);
    protected abstract void VisitDict(DictExpression dict, Param param);
    protected abstract void VisitIdentifier(IdentifierExpression identifier, Param param);
    protected abstract void VisitList(ListExpression list, Param param);
    protected abstract void VisitNilConstant(NilConstantExpression nilConstant, Param param);
    protected abstract void VisitNumberConstant(NumberConstantExpression numberConstant,
                                                Param param);
    protected abstract void VisitSlice(SliceExpression slice, Param param);
    protected abstract void VisitStringConstant(StringConstantExpression stringConstant,
                                                Param param);
    protected abstract void VisitSubscript(SubscriptExpression subscript, Param param);
    protected abstract void VisitTuple(TupleExpression tuple, Param param);
    protected abstract void VisitUnary(UnaryExpression unary, Param param);
  }
}
