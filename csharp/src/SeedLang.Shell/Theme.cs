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

using System;
using System.Collections.Generic;
using SeedLang.Common;

namespace SeedLang.Shell {
  internal static class Theme {
    // Theme information of source code.
    internal class ThemeInfo {
      public ConsoleColor ForegroundColor;
    }

    // The dictionary to map syntax token types to theme information. The default console forground
    // color is used if any syntax type does not exist in this dictionary.
    public static IReadOnlyDictionary<TokenType, ThemeInfo> SyntaxToThemeInfoMap =
        new Dictionary<TokenType, ThemeInfo> {
          {
            TokenType.Boolean,
            new ThemeInfo {
              ForegroundColor = ConsoleColor.Yellow,
            }
          },
          {
            TokenType.OpenBracket,
            new ThemeInfo {
              ForegroundColor = ConsoleColor.Gray,
            }
          },
          {
            TokenType.CloseBracket,
            new ThemeInfo {
              ForegroundColor = ConsoleColor.Gray,
            }
          },
          {
            TokenType.Function,
            new ThemeInfo {
              ForegroundColor = ConsoleColor.DarkBlue,
            }
          },
          {
            TokenType.Keyword,
            new ThemeInfo {
              ForegroundColor = ConsoleColor.Magenta,
            }
          },
          {
            TokenType.None,
            new ThemeInfo {
              ForegroundColor = ConsoleColor.DarkYellow,
            }
          },
          {
            TokenType.Number,
            new ThemeInfo {
              ForegroundColor = ConsoleColor.Yellow,
            }
          },
          {
            TokenType.Operator,
            new ThemeInfo {
              ForegroundColor = ConsoleColor.Blue,
            }
          },
          {
            TokenType.Parameter,
            new ThemeInfo {
              ForegroundColor = ConsoleColor.Blue,
            }
          },
          {
            TokenType.OpenParenthesis,
            new ThemeInfo {
              ForegroundColor = ConsoleColor.Gray,
            }
          },
          {
            TokenType.CloseParenthesis,
            new ThemeInfo {
              ForegroundColor = ConsoleColor.Gray,
            }
          },
          {
            TokenType.String,
            new ThemeInfo {
              ForegroundColor = ConsoleColor.Cyan,
            }
          },
          {
            TokenType.Variable,
            new ThemeInfo {
              ForegroundColor = ConsoleColor.Green,
            }
          },
        };
  }
}
