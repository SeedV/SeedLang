@echo off
REM Copyright 2021 The Aha001 Team.
REM
REM Licensed under the Apache License, Version 2.0 (the "License");
REM you may not use this file except in compliance with the License.
REM You may obtain a copy of the License at
REM
REM     http://www.apache.org/licenses/LICENSE-2.0
REM
REM Unless required by applicable law or agreed to in writing, software
REM distributed under the License is distributed on an "AS IS" BASIS,
REM WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
REM See the License for the specific language governing permissions and
REM limitations under the License.

set ANTLR4_JAR=antlr-4.9.2-complete.jar
set ANTLR4_JAR_DIR=.antlr
set ANTLR4_JAR_URL=https://www.antlr.org/download/%ANTLR4_JAR%

if not exist %ANTLR4_JAR_DIR% (
  mkdir "%ANTLR4_JAR_DIR%"
)

if not exist %ANTLR4_JAR_DIR%\%ANTLR4_JAR% (
  curl "%ANTLR4_JAR_URL%" -o "%ANTLR4_JAR_DIR%\%ANTLR4_JAR%"
)

set ANTLR4=java -jar "%ANTLR4_JAR_DIR%\%ANTLR4_JAR%"
set SEEDX_DIR=..\csharp\src\SeedLang\X\antlr
set FLAGS=-Dlanguage=CSharp -no-listener -visitor

%ANTLR4% %FLAGS% -o "%SEEDX_DIR%" SeedBlock.g4
%ANTLR4% %FLAGS% -o "%SEEDX_DIR%" SeedPython.g4
