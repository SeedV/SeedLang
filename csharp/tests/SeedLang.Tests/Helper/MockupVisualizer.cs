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

using System.Collections.Generic;
using System.Text;
using SeedLang.Runtime;

namespace SeedLang.Tests.Helper {
  internal class MockupVisualizer : IVisualizer<AssignmentEvent>, IVisualizer<BinaryEvent>,
                                    IVisualizer<BooleanEvent>, IVisualizer<ComparisonEvent>,
                                    IVisualizer<EvalEvent>, IVisualizer<PrintEvent>,
                                    IVisualizer<UnaryEvent> {
    private readonly List<AssignmentEvent> _assignEvents = new List<AssignmentEvent>();
    private readonly List<BinaryEvent> _binaryEvents = new List<BinaryEvent>();
    private readonly List<BooleanEvent> _booleanEvents = new List<BooleanEvent>();
    private readonly List<ComparisonEvent> _comparisonEvents = new List<ComparisonEvent>();
    private readonly List<EvalEvent> _evalEvents = new List<EvalEvent>();
    private readonly List<PrintEvent> _printEvents = new List<PrintEvent>();
    private readonly List<UnaryEvent> _unaryEvents = new List<UnaryEvent>();

    public override string ToString() {
      var sb = new StringBuilder();
      AssignmentEventsToString(sb);
      BinaryEventsToString(sb);
      BooleanEventsToString(sb);
      ComparisonEventsToString(sb);
      EvalEventsToString(sb);
      PrintEventsToString(sb);
      UnaryEventsToString(sb);
      return sb.ToString();
    }

    public void On(AssignmentEvent ae) {
      _assignEvents.Add(ae);
    }

    public void On(BinaryEvent be) {
      _binaryEvents.Add(be);
    }

    public void On(BooleanEvent be) {
      _booleanEvents.Add(be);
    }

    public void On(ComparisonEvent ce) {
      _comparisonEvents.Add(ce);
    }

    public void On(EvalEvent ee) {
      _evalEvents.Add(ee);
    }

    public void On(PrintEvent pe) {
      _printEvents.Add(pe);
    }

    public void On(UnaryEvent ue) {
      _unaryEvents.Add(ue);
    }

    internal string AssignmentEventsToString() {
      var sb = new StringBuilder();
      AssignmentEventsToString(sb);
      return sb.ToString();
    }

    internal string BinaryEventsToString() {
      var sb = new StringBuilder();
      BinaryEventsToString(sb);
      return sb.ToString();
    }

    internal string BooleanEventsToString() {
      var sb = new StringBuilder();
      BooleanEventsToString(sb);
      return sb.ToString();
    }

    internal string ComparisonEventsToString() {
      var sb = new StringBuilder();
      ComparisonEventsToString(sb);
      return sb.ToString();
    }

    internal string EvalEventsToString() {
      var sb = new StringBuilder();
      EvalEventsToString(sb);
      return sb.ToString();
    }

    internal string UnaryEventsToString() {
      var sb = new StringBuilder();
      UnaryEventsToString(sb);
      return sb.ToString();
    }

    private void AssignmentEventsToString(StringBuilder sb) {
      foreach (var ae in _assignEvents) {
        sb.AppendLine($"{ae.Range} {ae.Identifier} = {ae.Value}");
      }
    }

    private void BinaryEventsToString(StringBuilder sb) {
      foreach (var be in _binaryEvents) {
        sb.AppendLine($"{be.Range} {be.Left} {be.Op} {be.Right} = {be.Result}");
      }
    }

    private void BooleanEventsToString(StringBuilder sb) {
      foreach (var be in _booleanEvents) {
        sb.Append($"{be.Range} {be.Values[0]} ");
        for (int i = 1; i < be.Values.Count; ++i) {
          IValue value = be.Values[i];
          string valueStr = value.IsBoolean ? value.ToString() : "?";
          sb.Append($"{be.Op} {valueStr} ");
        }
        sb.AppendLine($"= {be.Result}");
      }
    }

    private void ComparisonEventsToString(StringBuilder sb) {
      foreach (var ce in _comparisonEvents) {
        sb.Append($"{ce.Range} {ce.First} ");
        for (int i = 0; i < ce.Ops.Count; ++i) {
          IValue value = ce.Values[i];
          string valueStr = value.IsNumber ? value.ToString() : "?";
          sb.Append($"{ce.Ops[i]} {valueStr} ");
        }
        sb.AppendLine($"= {ce.Result}");
      }
    }

    private void EvalEventsToString(StringBuilder sb) {
      foreach (var ee in _evalEvents) {
        sb.AppendLine($"{ee.Range} Eval {ee.Value}");
      }
    }

    private void PrintEventsToString(StringBuilder sb) {
      foreach (var pe in _printEvents) {
        sb.Append($"{pe.Range} Print ");
        for (int i = 0; i < pe.Values.Count; i++) {
          sb.Append($"{pe.Values[i]}");
          if (i < pe.Values.Count - 1) {
            sb.Append($", ");
          }
        }
        sb.AppendLine();
      }
    }

    private void UnaryEventsToString(StringBuilder sb) {
      foreach (var ue in _unaryEvents) {
        sb.AppendLine($"{ue.Range} {ue.Op} {ue.Value} = {ue.Result}");
      }
    }
  }
}
