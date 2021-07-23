/*
 * Copyright 2021 The Aha001 Team.
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
 * This is a derived work of https://github.com/bkiers/Python3-parser.
 * Here is the original license notice:
 *
 * The MIT License (MIT)
 *
 * Copyright (c) 2014 by Bart Kiers
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 *
 * Project      : python3-parser; an ANTLR4 grammar for Python 3
 *                https://github.com/bkiers/python3-parser
 * Developed by : Bart Kiers, bart@big-o.nl
 */

grammar SeedPython;

@header {
  #pragma warning disable 3021
}

tokens {
  INDENT,
  DEDENT
}

/*
 * Parser rules
 */

single_input:
  NEWLINE
  | simple_stmt
  | compound_stmt NEWLINE;

file_input: (NEWLINE | stmt)* EOF;

stmt: simple_stmt | compound_stmt;

simple_stmt:
  small_stmt (';' small_stmt)* (';')? NEWLINE;

small_stmt:
  assignment_stmt
  | eval_stmt
  | flow_stmt;

assignment_stmt: IDENTIFIER '=' expr;
eval_stmt: 'eval' expr;
flow_stmt: break_stmt | continue_stmt;
break_stmt: 'break';
continue_stmt: 'continue';

compound_stmt: if_stmt | while_stmt;
if_stmt:
  'if' test ':' suite ('elif' test ':' suite)* (
    'else' ':' suite
  )?;
while_stmt: 'while' test ':' suite;

suite: simple_stmt | NEWLINE INDENT stmt+ DEDENT;

test: expr (comp_op expr)*;

comp_op: '<' | '>' | '==' | '>=' | '<=' | '!=';

expr:
  expr op = (MUL | DIV) expr   # mul_div
  | expr op = (ADD | SUB) expr # add_sub
  | IDENTIFIER                 # identifier
  | NUMBER                     # number
  | '(' expr ')'               # grouping;

/*
 * Lexer rules
 */

ADD: '+';
SUB: '-';
MUL: '*';
DIV: '/';

NUMBER: INTEGER | FLOAT_NUMBER;

INTEGER: DECIMAL_INTEGER;

DECIMAL_INTEGER: NON_ZERO_DIGIT DIGIT* | '0'+;

FLOAT_NUMBER: POINT_FLOAT | EXPONENT_FLOAT;

STRING: STRING_LITERAL;

STRING_LITERAL: '"' .*? '"';

IDENTIFIER: ID_START ID_CONTINUE*;

OPEN_PAREN: '(';
CLOSE_PAREN: ')';
OPEN_BRACK: '[';
CLOSE_BRACK: ']';
OPEN_BRACE: '{';
CLOSE_BRACE: '}';

NEWLINE: ( '\r'? '\n' | '\r' | '\f') SPACES?;

SKIP_: ( SPACES | COMMENT | LINE_JOINING) -> skip;

UNKNOWN_CHAR: .;

/*
 * Fragments
 */

fragment POINT_FLOAT:
  INT_PART? FRACTION
  | INT_PART '.';

fragment EXPONENT_FLOAT: (INT_PART | POINT_FLOAT) EXPONENT;

fragment INT_PART: DIGIT+;

fragment FRACTION: '.' DIGIT+;

fragment EXPONENT: [eE] [+-]? DIGIT+;

fragment NON_ZERO_DIGIT: [1-9];

fragment DIGIT: [0-9];

fragment SPACES: [ \t]+;

fragment COMMENT: '#' ~[\r\n\f]*;

fragment LINE_JOINING:
  '\\' SPACES? ('\r'? '\n' | '\r' | '\f');

fragment ID_START: '_' | [A-Z] | [a-z];

fragment ID_CONTINUE: ID_START | [0-9];
