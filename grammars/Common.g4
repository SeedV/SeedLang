/**
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
 */

grammar Common;

@header {
  #pragma warning disable 3021
}

/*
 * Parser rules
 */

expression:
  unaryOperator expression                   # unaryExpression
  | expression mulDivOperator expression     # mulDivExpression
  | expression addSubOperator expression     # addSubExpression
  | expression (compareOperator expression)+ # comapreExpression
  | IDENTIFIER                               # identifier
  | NUMBER                                   # number
  | OPEN_PAREN expression CLOSE_PAREN        # grouping;

compare: expression ( compareOperator expression)+;

unaryOperator: SUB;

mulDivOperator: MUL | DIV;

addSubOperator: ADD | SUB;

compareOperator:
  LESS
  | GREAT
  | EQUALEQUAL
  | LESSEQUAL
  | GREATEQUAL
  | NOTEQUAL;

/*
 * Lexer rules
 */

EQUAL: '=';

ADD: '+';
SUB: '-';
MUL: '*';
DIV: '/';

LESS: '<';
GREAT: '>';
EQUALEQUAL: '==';
GREATEQUAL: '>=';
LESSEQUAL: '<=';
NOTEQUAL: '!=';

IDENTIFIER: ID_START ID_CONTINUE*;

NUMBER: INTEGER | FLOAT_NUMBER;

STRING: '"' .*? '"';

INTEGER: DECIMAL_INTEGER;

DECIMAL_INTEGER: NON_ZERO_DIGIT DIGIT* | '0'+;

FLOAT_NUMBER: POINT_FLOAT | EXPONENT_FLOAT;

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
