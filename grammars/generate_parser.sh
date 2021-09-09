#!/bin/bash
# Copyright 2021 The Aha001 Team.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

readonly ANTLR4_JAR="antlr-4.9.2-complete.jar"
readonly ANTLR4_JAR_DIR=".antlr"
readonly ANTLR4_JAR_URL="https://www.antlr.org/download/${ANTLR4_JAR}"

if [[ ! -d "${ANTLR4_JAR_DIR}" ]]; then
  mkdir "${ANTLR4_JAR_DIR}"
fi

if [[ ! -f "${ANTLR4_JAR_DIR}/${ANTLR4_JAR}" ]]; then
  curl "${ANTLR4_JAR_URL}" -o "${ANTLR4_JAR_DIR}/${ANTLR4_JAR}"
fi

readonly ANTLR4="java -jar ${ANTLR4_JAR_DIR}/${ANTLR4_JAR}"
readonly SEEDBLOCK_DIR="../csharp/src/SeedLang/Block/antlr"
readonly SEEDX_DIR="../csharp/src/SeedLang/X/antlr"
readonly FLAGS=(-Dlanguage=CSharp -no-listener -visitor)

${ANTLR4} "${FLAGS[@]}" -o "${SEEDBLOCK_DIR}" SeedBlock.g4
${ANTLR4} "${FLAGS[@]}" -o "${SEEDX_DIR}" SeedPython.g4
