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

using System.Diagnostics;
using Antlr4.Runtime;
using SeedLang.Common;

namespace SeedLang.X {
  // A syntax error strategy for SeedLang languages. It overrides the report methods of
  // DefaultErrorStrategy to collect diagnostics.
  internal class SyntaxErrorStrategy : DefaultErrorStrategy {
    private readonly string _module;
    private readonly DiagnosticCollection _collection;

    public SyntaxErrorStrategy(string module, DiagnosticCollection collection) {
      _module = module ?? "";
      _collection = collection ?? new DiagnosticCollection();
    }

    // Invoked when an exception is thrown during parsing.
    public override void ReportError(Parser parser, RecognitionException e) {
      if (TryBeginErrorCondition(parser)) {
        switch (e) {
          case NoViableAltException exception:
            if (parser.InputStream is ITokenStream tokenStream) {
              string tokens;
              if (exception.StartToken.Type == TokenConstants.EOF) {
                tokens = "<EOF>";
              } else {
                tokens = tokenStream.GetText(exception.StartToken, exception.OffendingToken);
              }
              tokens = EscapeWSAndQuote(tokens);
              ReportDiagnostic(
                  CodeReferenceUtils.RangeOfTokens(exception.StartToken, exception.OffendingToken),
                  Message.SyntaxErrorNoViableAlternative1, tokens);
            }
            break;
          case InputMismatchException ime:
            ReportDiagnosticForToken(parser, ime.OffendingToken, Message.SyntaxErrorInputMismatch2);
            break;
          case FailedPredicateException fpe:
            string ruleName = parser.RuleNames[parser.RuleContext.RuleIndex];
            ReportDiagnostic(
                CodeReferenceUtils.RangeOfTokens(fpe.OffendingToken, fpe.OffendingToken),
                Message.SyntaxErrorFailedPredicate1, ruleName);
            break;
          default:
            Debug.Fail("Unsupported recognition exception.");
            break;
        }
      }
    }

    protected override void ReportMissingToken(Parser parser) {
      if (TryBeginErrorCondition(parser)) {
        ReportDiagnosticForToken(parser, parser.CurrentToken, Message.SyntaxErrorMissingToken2);
      }
    }

    protected override void ReportUnwantedToken(Parser parser) {
      if (TryBeginErrorCondition(parser)) {
        ReportDiagnosticForToken(parser, parser.CurrentToken, Message.SyntaxErrorUnwantedToken2);
      }
    }

    private bool TryBeginErrorCondition(Parser parser) {
      // Doesn't report any error, if an error has already been reported and a token haven't been
      // matched successfully yet.
      if (!InErrorRecoveryMode(parser)) {
        BeginErrorCondition(parser);
        return true;
      }
      return false;
    }

    private void ReportDiagnostic(Range range, Message messageId, params string[] arguments) {
      _collection.Report(SystemReporters.SeedX, Severity.Fatal, _module, range, messageId,
                         arguments);
    }

    private void ReportDiagnosticForToken(Parser parser, IToken token, Message messageId) {
      string expectedTokens = GetExpectedTokens(parser).ToString(parser.Vocabulary);
      ReportDiagnostic(CodeReferenceUtils.RangeOfTokens(token, token),
                       messageId, GetTokenErrorDisplay(token), expectedTokens);
    }
  }
}
