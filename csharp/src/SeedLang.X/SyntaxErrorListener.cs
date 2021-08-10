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

using System.IO;
using Antlr4.Runtime;
using SeedLang.Common;

namespace SeedLang.X {
  // A syntax error listener class to collect ANTLR4 parse errors and store them into a diagnostic
  // collection.
  internal class SyntaxErrorListener : BaseErrorListener {
    private readonly string _module;
    private readonly DiagnosticCollection _collection;

    public SyntaxErrorListener(string module, DiagnosticCollection collection) {
      _module = module ?? "";
      _collection = collection ?? new DiagnosticCollection();
    }

    public override void SyntaxError(TextWriter output, IRecognizer recognizer,
                                     IToken offendingSymbol, int line, int charPositionInLine,
                                     string msg, RecognitionException e) {
      var length = offendingSymbol.StopIndex - offendingSymbol.StartIndex + 1;
      var range = new TextRange(offendingSymbol.Line, offendingSymbol.Column, offendingSymbol.Line,
                                offendingSymbol.Column + length);
      // TODO: map the msg to the localized message defined in the common component.
      _collection.Report(new Diagnostic(SystemReporters.SeedX, Severity.Fatal, _module, range,
                                        msg));
    }
  }
}
