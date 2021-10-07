using System.Collections.Generic;
// Copyright 2021 The Aha001 Team.
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
using SeedLang.Common;

namespace SeedLang.Shell {
  internal static class Theme {
    internal class ThemeInfo {
      public ConsoleColor BackgroundColor;
      public ConsoleColor ForegroundColor;
    }

    public const ConsoleColor BackgroundColor = ConsoleColor.Black;
    public const ConsoleColor ForegroundColor = ConsoleColor.White;

    public static IReadOnlyDictionary<SyntaxType, ThemeInfo> SyntaxToThemeInfoMap =
        new Dictionary<SyntaxType, ThemeInfo> {
          {
            SyntaxType.Keyword,
            new ThemeInfo {
              BackgroundColor = ConsoleColor.Black,
              ForegroundColor = ConsoleColor.Magenta,
            }
          },
          {
            SyntaxType.Number,
            new ThemeInfo {
              BackgroundColor = ConsoleColor.Black,
              ForegroundColor = ConsoleColor.Yellow,
            }
          },
          {
            SyntaxType.Operator,
            new ThemeInfo {
              BackgroundColor = ConsoleColor.Black,
              ForegroundColor = ConsoleColor.Blue,
            }
          },
          {
            SyntaxType.Parenthesis,
            new ThemeInfo {
              BackgroundColor = ConsoleColor.Black,
              ForegroundColor = ConsoleColor.White,
            }
          },
          {
            SyntaxType.String,
            new ThemeInfo {
              BackgroundColor = ConsoleColor.Black,
              ForegroundColor = ConsoleColor.Cyan,
            }
          },
          {
            SyntaxType.Symbol,
            new ThemeInfo {
              BackgroundColor = ConsoleColor.Black,
              ForegroundColor = ConsoleColor.White,
            }
          },
          {
            SyntaxType.Variable,
            new ThemeInfo {
              BackgroundColor = ConsoleColor.Black,
              ForegroundColor = ConsoleColor.Green,
            }
          },
        };
  }
}
