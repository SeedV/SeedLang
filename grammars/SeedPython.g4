/*
 * Copyright 2021 The Aha001 Team.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except
 * in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software distributed under the License
 * is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
 * or implied. See the License for the specific language governing permissions and limitations under
 * the License.
 */
grammar SeedPython;

/*
 * parser rules
 */

prog: (NEWLINE | stmt)*;

stmt: simple_stmt;

simple_stmt: small_stmt NEWLINE;
small_stmt: assign_stmt;
assign_stmt: IDENTIFIER '=' expr;

expr:
	expr op = (MUL | DIV) expr		# mul_div
	| expr op = (ADD | SUB) expr	# add_sub
	| IDENTIFIER					# identifier
	| NUMBER						# number
	| '(' expr ')'					# grouping;

/*
 * lexer rules
 */

ADD: '+';
SUB: '-';
MUL: '*';
DIV: '/';

NUMBER: INTEGER;
INTEGER: DECIMAL_INTEGER;
DECIMAL_INTEGER: NON_ZERO_DIGIT DIGIT* | '0'+;

STRING: STRING_LITERAL;
STRING_LITERAL: '"' .*? '"';

IDENTIFIER: ID_START ID_CONTINUE*;

NEWLINE: ( '\r'? '\n' | '\r' | '\f') SPACES?;
SKIP_RULES: (SPACES | COMMENT | LINE_JOINING) -> skip;

/*
 * fragments
 */

fragment NON_ZERO_DIGIT: [1-9];
fragment DIGIT: [0-9];
fragment SPACES: [ \t]+;
fragment COMMENT: '#' ~[\r\n\f]*;
fragment LINE_JOINING: '\\' SPACES? ('\r'? '\n' | '\r' | '\f');
fragment ID_START: '_' | [A-Z] | [a-z];
fragment ID_CONTINUE: ID_START | [0-9];
