/**
 * Copyright 2021-2022 The SeedV Lab.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * SeedPyhton grammar is referred and modified from:
 * https://docs.python.org/3.10/reference/grammar.html
*/

grammar SeedPython;

import Common;

tokens {
  INDENT,
  DEDENT,
  VTAG_START,
  VTAG_END
}

/*
 * Parser rules
 */

program: statements EOF;

statements: (NEWLINE | statement)+;
statement:
  simple_stmts
  | compound_stmt
  | vtag_stmt;

simple_stmts:
  simple_stmt (SEMICOLON simple_stmt)* SEMICOLON? NEWLINE;
simple_stmt:
  assignment    # assignment_placeholder
  | expressions # expression_stmt
  | return_stmt # return_stmt_placeholder
  | PASS        # pass
  | BREAK       # break
  | CONTINUE    # continue
  | import_stmt # import_stmt_placeholder;

compound_stmt:
  function_def
  | for_in_stmt
  | if_stmt
  | while_stmt;

vtag_stmt:
  vtag_start VTAG_END NEWLINE statement? # single_line_vtag_stmt
  | vtag_start statements VTAG_END       # multiple_line_vtag_stmt;
vtag_start: VTAG_START vtag (COMMA vtag)*;
vtag: NAME (OPEN_PAREN arguments CLOSE_PAREN)?;

assignment:
  (targets EQUAL)+ expressions        # assign
  | target ADD_ASSIGN expression      # add_assign
  | target SUBTRACT_ASSIGN expression # subtract_assign
  | target MULTIPLY_ASSIGN expression # multiply_assign
  | target DIVIDE_ASSIGN expression   # divide_assign
  | target MODULO_ASSIGN expression   # modulo_assign;

targets: target (COMMA target)*;
target:
  identifier                                   # identifier_target
  | primary OPEN_BRACK slice_index CLOSE_BRACK # subscript_target;

if_stmt:
  IF expression COLON block elif_stmt       # if_elif
  | IF expression COLON block (else_block)? # if_else;
elif_stmt:
  ELIF expression COLON block elif_stmt       # elif_elif
  | ELIF expression COLON block (else_block)? # elif_else;
else_block: ELSE COLON block;

for_in_stmt:
  FOR identifier IN expression COLON block;

while_stmt: WHILE expression COLON block;

function_def:
  DEF NAME OPEN_PAREN parameters? CLOSE_PAREN COLON block;
parameters: NAME (COMMA NAME)*;

return_stmt: RETURN expressions?;

import_stmt: IMPORT NAME;

block:
  NEWLINE INDENT statements DEDENT # statements_as_block
  | simple_stmts                   # simple_stmts_as_block;

/*
 * Lexer rules
 */

IF: 'if';
ELIF: 'elif';
ELSE: 'else';
FOR: 'for';
IN: 'in';
WHILE: 'while';
DEF: 'def';
RETURN: 'return';
PASS: 'pass';
BREAK: 'break';
CONTINUE: 'continue';
IMPORT: 'import';

SEMICOLON: ';';

COMMENT: '#' ~[\r\n\f]*;
