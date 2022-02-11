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

using SeedLang.Common;

namespace SeedLang.Ast {
  // The base class of all statement nodes.
  internal abstract class Statement : AstNode {
    // The factory method to create an assignment statement.
    internal static AssignmentStatement Assignment(Expression[] targets, Expression[] exprs,
                                                   Range range) {
      return new AssignmentStatement(targets, exprs, range);
    }

    // The factory method to create a block statement.
    internal static BlockStatement Block(Statement[] statements, Range range) {
      return new BlockStatement(statements, range);
    }

    // The factory method to create an expression statement.
    internal static ExpressionStatement Expression(Expression expr, Range range) {
      return new ExpressionStatement(expr, range);
    }

    // The factory method to create a for in statement.
    internal static ForInStatement ForIn(IdentifierExpression id, Expression expr, Statement body,
                                         Range range) {
      return new ForInStatement(id, expr, body, range);
    }

    // The factory method to create a function define statement.
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
    // The targets of assignment statements. The class of each target could be IdentifierExpression
    // or SubscriptExpression.
    public Expression[] Targets { get; }
    // The right values of assignment statements. The lengths of targets and exprs are not necessary
    // to be the same.
    public Expression[] Exprs { get; }

    internal AssignmentStatement(Expression[] targets, Expression[] exprs, Range range) :
        base(range) {
      Targets = targets;
      Exprs = exprs;
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

  internal class ForInStatement : Statement {
    public IdentifierExpression Id { get; }
    public Expression Expr { get; }
    public Statement Body { get; }

    internal ForInStatement(IdentifierExpression id, Expression expr, Statement body, Range range) :
        base(range) {
      Id = id;
      Expr = expr;
      Body = body;
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
