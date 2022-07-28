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
using System.Text.RegularExpressions;
using SeedLang.Common;
using SeedLang.Runtime;
using SeedLang.Visualization;

namespace SeedLang.Shell {
  // A class to manage all the visualizers.
  internal static class VisualizerManager {
    // The visualizer for a specified event.
    private class Visualizer<Event> : IVisualizer<Event> where Event : AbstractEvent {
      public void On(Event e, IVM vm) {
        if (e.Range is TextRange range) {
          Source.WriteSourceWithHighlight(range);
        }
        WriteEvent(e);
      }
    }

    public static IEnumerable<string> EventNames =>
        Array.ConvertAll(typeof(Event).GetNestedTypes(), type => type.Name);
    public static SourceCode Source { get; set; }

    private static readonly Dictionary<BinaryOperator, string> _binaryOperatorStrings =
        new Dictionary<BinaryOperator, string>() {
          [BinaryOperator.Add] = "+",
          [BinaryOperator.Subtract] = "-",
          [BinaryOperator.Multiply] = "*",
          [BinaryOperator.Divide] = "/",
          [BinaryOperator.FloorDivide] = "//",
          [BinaryOperator.Power] = "**",
          [BinaryOperator.Modulo] = "%",
        };

    private static readonly Dictionary<BooleanOperator, string> _booleanOperatorStrings =
        new Dictionary<BooleanOperator, string>() {
          [BooleanOperator.And] = "and",
          [BooleanOperator.Or] = "or",
        };

    private static readonly Dictionary<ComparisonOperator, string> _comparisonOperatorStrings =
        new Dictionary<ComparisonOperator, string>() {
          [ComparisonOperator.Less] = "<",
          [ComparisonOperator.Greater] = ">",
          [ComparisonOperator.LessEqual] = "<=",
          [ComparisonOperator.GreaterEqual] = ">=",
          [ComparisonOperator.EqEqual] = "==",
          [ComparisonOperator.NotEqual] = "!=",
          [ComparisonOperator.In] = "in",
        };

    private static readonly Dictionary<UnaryOperator, string> _unaryOperatorStrings =
        new Dictionary<UnaryOperator, string>() {
          [UnaryOperator.Positive] = "+",
          [UnaryOperator.Negative] = "-",
          [UnaryOperator.Not] = "not",
        };

    private static readonly Dictionary<string, dynamic> _visualizers =
        new Dictionary<string, dynamic>();

    internal static ICollection<string> CreateVisualizers(IEnumerable<string> visualizers) {
      var qualifiedNames = new Dictionary<string, string>();
      var eventTypes = typeof(Event).GetNestedTypes();
      for (int i = 0; i < eventTypes.Length; i++) {
        qualifiedNames[eventTypes[i].Name] = eventTypes[i].AssemblyQualifiedName;
      }
      foreach (var name in EventClassNames(visualizers, qualifiedNames)) {
        var type = typeof(Visualizer<>).MakeGenericType(Type.GetType(name.Value));
        _visualizers[name.Key] = Activator.CreateInstance(type);
      }
      return _visualizers.Keys;
    }

    internal static void RegisterToEngine(Engine engine) {
      foreach (var visualizer in _visualizers.Values) {
        engine.Register(visualizer);
      }
    }

    internal static void UnregisterFromEngine(Engine engine) {
      foreach (var visualizer in _visualizers.Values) {
        engine.Unregister(visualizer);
      }
    }

    private static Dictionary<string, string> EventClassNames(
        IEnumerable<string> visualizers, Dictionary<string, string> qualifiedNames) {
      var eventClassNames = new Dictionary<string, string>();
      foreach (string visualizer in visualizers) {
        string visualizerName = visualizer.Trim();
        if (visualizerName.Contains('*')) {
          var regex = new Regex(visualizerName.Replace("*", ".*"));
          foreach (string name in qualifiedNames.Keys) {
            Match match = regex.Match(name);
            if (match.Success) {
              eventClassNames[name] = qualifiedNames[name];
            }
          }
        } else if (qualifiedNames.ContainsKey(visualizerName)) {
          eventClassNames[visualizerName] = qualifiedNames[visualizerName];
        }
      }
      return eventClassNames;
    }

    private static void WriteEvent(AbstractEvent e) {
      Console.BackgroundColor = ConsoleColor.White;
      Console.ForegroundColor = ConsoleColor.Black;
      switch (e) {
        case Event.Assignment ae:
          Console.Write($"Assign: {ae.Target} = {ae.RValue}");
          break;
        case Event.Binary be: {
            var op = _binaryOperatorStrings[be.Op];
            Console.Write($"Binary: {be.Left} {op} {be.Right} = {be.Result}");
            break;
          }
        case Event.Comparison ce: {
            var op = _comparisonOperatorStrings[ce.Op];
            Console.Write($"Comparison: {ce.Left} {op} {ce.Right} = {ce.Result}");
            break;
          }
        case Event.FuncCalled fce:
          Console.Write($"FuncCalled: {fce.Name} {string.Join(", ", fce.Args)}");
          break;
        case Event.FuncReturned fre:
          Console.Write($"FuncReturned: {fre.Name} {fre.Result}");
          break;
        case Event.SingleStep sse:
          Console.Write($"SingleStep: {sse.Range.Start.Line}");
          break;
        case Event.VariableDefined vde:
          Console.Write($"VariableDefined: {vde.Name}");
          break;
        case Event.VariableDeleted vde:
          Console.Write($"VariableDeleted: {vde.Name}");
          break;
        case Event.VTagEntered vee:
          Console.Write($"VTagEntered: {string.Join(", ", vee.VTags)}");
          break;
        case Event.VTagExited vee:
          Console.Write($"VTagExited: {string.Join(", ", vee.VTags)}");
          break;
        default:
          throw new NotImplementedException($"Unsupported event: {e}");
      }
      Console.ResetColor();
      Console.WriteLine();
    }
  }
}
