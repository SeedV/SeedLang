using System.Text;
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

using SeedLang.Runtime;

namespace SeedLang.Ast.Tests {
  internal class MockupVisualizer : IVisualizer<AssignmentEvent>, IVisualizer<BinaryEvent>,
                                    IVisualizer<BooleanEvent>, IVisualizer<ComparisonEvent>,
                                    IVisualizer<EvalEvent>, IVisualizer<UnaryEvent> {
    private AssignmentEvent _assignEvent;
    private BinaryEvent _binaryEvent;
    private BooleanEvent _booleanEvent;
    private ComparisonEvent _comparisonEvent;
    private EvalEvent _evalEvent;
    private UnaryEvent _unaryEvent;

    public override string ToString() {
      var sb = new StringBuilder();
      AssignmentEventToString(sb);
      BinaryEventToString(sb);
      BooleanEventToString(sb);
      ComparisonEventToString(sb);
      EvalEventToString(sb);
      UnaryEventToString(sb);
      return sb.ToString();
    }

    public void On(AssignmentEvent ae) {
      _assignEvent = ae;
    }

    public void On(BinaryEvent be) {
      _binaryEvent = be;
    }

    public void On(BooleanEvent be) {
      _booleanEvent = be;
    }

    public void On(ComparisonEvent ce) {
      _comparisonEvent = ce;
    }

    public void On(EvalEvent ee) {
      _evalEvent = ee;
    }

    public void On(UnaryEvent ue) {
      _unaryEvent = ue;
    }

    private void AssignmentEventToString(StringBuilder sb) {
      if (!(_assignEvent is null)) {
        sb.AppendLine($"{_assignEvent.Range} {_assignEvent.Identifier} = {_assignEvent.Value}");
      }
    }

    private void BinaryEventToString(StringBuilder sb) {
      if (!(_binaryEvent is null)) {
        sb.Append($"{_binaryEvent.Range} {_binaryEvent.Left} {_binaryEvent.Op} ");
        sb.AppendLine($"{_binaryEvent.Right} = {_binaryEvent.Result}");
      }
    }

    private void BooleanEventToString(StringBuilder sb) {
      if (!(_booleanEvent is null)) {
        sb.Append($"{_booleanEvent.Range} ");
        sb.Append($"{_booleanEvent.Values[0]} ");
        for (int i = 1; i < _booleanEvent.Values.Length; ++i) {
          IValue value = _booleanEvent.Values[i];
          string valueStr = value is null ? "?" : value.ToString();
          sb.Append($"{_booleanEvent.Op} {valueStr} ");
        }
        sb.AppendLine($"= {_booleanEvent.Result}");
      }
    }

    private void ComparisonEventToString(StringBuilder sb) {
      if (!(_comparisonEvent is null)) {
        sb.Append($"{_comparisonEvent.Range} {_comparisonEvent.First} ");
        for (int i = 0; i < _comparisonEvent.Ops.Length; ++i) {
          IValue value = _comparisonEvent.Values[i];
          string valueStr = value is null ? "?" : value.ToString();
          sb.Append($"{_comparisonEvent.Ops[i]} {valueStr} ");
        }
        sb.AppendLine($"= {_comparisonEvent.Result}");
      }
    }

    private void EvalEventToString(StringBuilder sb) {
      if (!(_evalEvent is null)) {
        sb.AppendLine($"{_evalEvent.Range} Eval {_evalEvent.Value}");
      }
    }

    private void UnaryEventToString(StringBuilder sb) {
      if (!(_unaryEvent is null)) {
        sb.Append($"{_unaryEvent.Range} {_unaryEvent.Op} {_unaryEvent.Value} ");
        sb.AppendLine($"= {_unaryEvent.Result}");
      }
    }
  }
}

