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

using System.Diagnostics;
using SeedLang.Common;

namespace SeedLang.Ast {
  // The base class of all statement nodes.
  internal abstract class Statement : AstNode {
    // The factory method to create an assignment statement.
    internal static AssignmentStatement Assignment(Expression target, Expression expr,
                                                   Range range) {
      return new AssignmentStatement(target, expr, range);
    }

    // The factory method to create a block statement.
    internal static BlockStatement Block(Statement[] statements, Range range) {
      return new BlockStatement(statements, range);
    }

    // The factory method to create an expression statement.
    internal static ExpressionStatement Expression(Expression expr, Range range) {
      return new ExpressionStatement(expr, range);
    }

    // The factory method to create a function declearation statement.
    internal static FuncDefStatement FuncDef(string name, string[] parameters, Statement body,
                                             Range range) {
      return new FuncDefStatement(name, parameters, body, range);
    }

    // The factory method to create an if statement.
    internal static IfStatement If(Expression test, Statement thenBody, Statement elseBody,
                                   Range range) {
      return new IfStatement(test, thenBody, elseBody, range);
    }

    // The factory method to create a return statement.
    internal static ReturnStatement Return(Expression result, Range range) {
      return new ReturnStatement(result, range);
    }

    // The factory method to create an while statement.
    internal static WhileStatement While(Expression test, Statement body, Range range) {
      return new WhileStatement(test, body, range);
    }

    internal Statement(Range range) : base(range) { }
  }

  internal class AssignmentStatement : Statement {
    // The target of the assignment statement. It could be IdentifierExpression or
    // SubscriptExpression.
    public Expression Target { get; }
    public Expression Expr { get; }

    internal AssignmentStatement(Expression target, Expression expr, Range range) :
        base(range) {
      Debug.Assert(target is IdentifierExpression || target is SubscriptExpression);
      Target = target;
      Expr = expr;
    }
  }

  internal class BlockStatement : Statement {
    public Statement[] Statements { get; }

    internal BlockStatement(Statement[] statements, Range range) : base(range) {
      Statements = statements;
    }
  }

  internal class ExpressionStatement : Statement {
    public Expression Expr { get; }

    internal ExpressionStatement(Expression expr, Range range) : base(range) {
      Expr = expr;
    }
  }

  internal class FuncDefStatement : Statement {
    public string Name { get; }
    public string[] Parameters { get; }
    public Statement Body { get; }

    internal FuncDefStatement(string name, string[] parameters, Statement body, Range range) :
        base(range) {
      Name = name;
      Parameters = parameters;
      Body = body;
    }
  }

  internal class IfStatement : Statement {
    public Expression Test { get; }
    public Statement ThenBody { get; }
    // The else body of if statements, could be null.
    public Statement ElseBody { get; }

    internal IfStatement(Expression test, Statement thenBody, Statement elseBody, Range range) :
        base(range) {
      Test = test;
      ThenBody = thenBody;
      ElseBody = elseBody;
    }
  }

  internal class ReturnStatement : Statement {
    public Expression Result { get; }

    internal ReturnStatement(Expression result, Range range) : base(range) {
      Result = result;
    }
  }

  internal class WhileStatement : Statement {
    public Expression Test { get; }
    public Statement Body { get; }

    internal WhileStatement(Expression test, Statement body, Range range) : base(range) {
      Test = test;
      Body = body;
    }
  }
}
