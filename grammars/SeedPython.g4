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

import Common;

tokens {
  INDENT,
  DEDENT
}

singleStatement: smallStatement EOF;

fileInput: (NEWLINE | statement)* EOF;

statement: simpleStatement | compoundStatement;

simpleStatement:
  smallStatement (';' smallStatement)* (';')?;

smallStatement:
  assignStatement
  | expressionStatement
  | flowStatement;

assignStatement: IDENTIFIER EQUAL expression;
expressionStatement: expression;
flowStatement: breakStatement | continueStatement;
breakStatement: 'break';
continueStatement: 'continue';

compoundStatement: ifStatement | whileStatement;
ifStatement:
  'if' comparison ':' suite (
    'elif' comparison ':' suite
  )* ('else' ':' suite)?;
whileStatement: 'while' comparison ':' suite;

suite:
  simpleStatement
  | NEWLINE INDENT statement+ DEDENT;
