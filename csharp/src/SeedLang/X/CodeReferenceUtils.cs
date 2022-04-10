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

using Antlr4.Runtime;
using SeedLang.Common;

namespace SeedLang.X {
  // Utilities to set up and maintain code references during parsing and compiling.
  internal static class CodeReferenceUtils {
    internal static TextRange RangeOfToken(IToken token) {
      return RangeOfTokens(token, token);
    }

    internal static TextRange RangeOfTokens(IToken start, IToken end) {
      // TODO: need scan the source string to calculate the end column if the end token is in
      // multiple lines.
      return new TextRange(start.Line, start.Column,
                           end.Line, end.Column + end.StopIndex - end.StartIndex);
    }

    internal static TextRange CombineRanges(TextRange start, TextRange end) {
      TextPosition startPos = start.Start.Line < end.Start.Line || (
                              start.Start.Line == end.Start.Line &&
                              start.Start.Column < end.Start.Column) ? start.Start : end.Start;
      TextPosition endPos = start.End.Line < end.End.Line || (
                            start.End.Line == end.End.Line &&
                            start.End.Column < end.End.Column) ? end.End : start.End;
      return new TextRange(startPos.Line, startPos.Column, endPos.Line, endPos.Column);
    }
  }
}
