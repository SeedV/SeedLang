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
using System.Linq;
using SeedLang.Common;
using SeedLang.Runtime;

namespace SeedLang.Shell {
  // The visualizer type. Uses All to enable all visualizers.
  internal enum VisualizerType {
    Assignment,
    Binary,
    Boolean,
    Comparison,
    Unary,
    All,
  }

  // A class to manage all the visualizers.
  internal class VisualizerManager {
    // The visualizer for a specified event.
    private class Visualizer<Event> : IVisualizer<Event> where Event : AbstractEvent {
      private readonly Action<TextRange> _writeSourceWithHighlight;
      private readonly Action<AbstractEvent> _writeEvent;

      public Visualizer(Action<TextRange> writeSourceWithHighlight,
                        Action<AbstractEvent> writeEvent) {
        _writeSourceWithHighlight = writeSourceWithHighlight;
        _writeEvent = writeEvent;
      }

      public void On(Event e) {
        if (e.Range is TextRange range) {
          _writeSourceWithHighlight(range);
        }
        _writeEvent(e);
      }
    }

    private readonly Dictionary<BinaryOperator, string> _binaryOperatorStrings =
        new Dictionary<BinaryOperator, string>() {
          [BinaryOperator.Add] = "+",
          [BinaryOperator.Subtract] = "-",
          [BinaryOperator.Multiply] = "*",
          [BinaryOperator.Divide] = "/",
          [BinaryOperator.FloorDivide] = "//",
          [BinaryOperator.Power] = "**",
          [BinaryOperator.Modulo] = "%",
        };

    private readonly Dictionary<BooleanOperator, string> _booleanOperatorStrings =
        new Dictionary<BooleanOperator, string>() {
          [BooleanOperator.And] = "and",
          [BooleanOperator.Or] = "or",
        };

    private readonly Dictionary<ComparisonOperator, string> _comparisonOperatorStrings =
        new Dictionary<ComparisonOperator, string>() {
          [ComparisonOperator.Less] = "<",
          [ComparisonOperator.Greater] = ">",
          [ComparisonOperator.LessEqual] = "<=",
          [ComparisonOperator.GreaterEqual] = ">=",
          [ComparisonOperator.EqEqual] = "==",
          [ComparisonOperator.NotEqual] = "!=",
        };

    private readonly Dictionary<UnaryOperator, string> _unaryOperatorStrings =
        new Dictionary<UnaryOperator, string>() {
          [UnaryOperator.Positive] = "+",
          [UnaryOperator.Negative] = "-",
          [UnaryOperator.Not] = "not",
        };


    private readonly SourceCode _source;
    private readonly Visualizer<AssignmentEvent> _assignmentVisualizer;
    private readonly Visualizer<BinaryEvent> _binaryVisualizer;
    private readonly Visualizer<BooleanEvent> _booleanVisualizer;
    private readonly Visualizer<ComparisonEvent> _comparisonVisualizer;
    private readonly Visualizer<UnaryEvent> _unaryVisualizer;

    internal VisualizerManager(SourceCode source, IEnumerable<VisualizerType> visualizerTypes) {
      _source = source;
      // Generates an array with all visualizer types if All is included in the visualizerTypes.
      var visualizers = Enumerable.Contains(visualizerTypes, VisualizerType.All) ?
                        Enum.GetValues(typeof(VisualizerType)) :
                        visualizerTypes.ToArray();
      foreach (VisualizerType type in visualizers) {
        switch (type) {
          case VisualizerType.Assignment:
            _assignmentVisualizer = new Visualizer<AssignmentEvent>(
                _source.WriteSourceWithHighlight, WriteEvent);
            break;
          case VisualizerType.Binary:
            _binaryVisualizer = new Visualizer<BinaryEvent>(_source.WriteSourceWithHighlight,
                                                            WriteEvent);
            break;
          case VisualizerType.Boolean:
            _booleanVisualizer = new Visualizer<BooleanEvent>(_source.WriteSourceWithHighlight,
                                                              WriteEvent);
            break;
          case VisualizerType.Comparison:
            _comparisonVisualizer = new Visualizer<ComparisonEvent>(
                _source.WriteSourceWithHighlight, WriteEvent);
            break;
          case VisualizerType.Unary:
            _unaryVisualizer = new Visualizer<UnaryEvent>(_source.WriteSourceWithHighlight,
                                                          WriteEvent);
            break;
          case VisualizerType.All:
            // Ignores.
            break;
          default:
            throw new NotImplementedException($"Unsupported visualizer type: {type}.");
        }
      }
    }

    internal void RegisterToExecutor(Executor executor) {
      RegisterToExecutor(executor, _assignmentVisualizer);
      RegisterToExecutor(executor, _binaryVisualizer);
      RegisterToExecutor(executor, _booleanVisualizer);
      RegisterToExecutor(executor, _comparisonVisualizer);
      RegisterToExecutor(executor, _unaryVisualizer);
    }

    internal void UnregisterFromExecutor(Executor executor) {
      UnregisterFromExecutor(executor, _assignmentVisualizer);
      UnregisterFromExecutor(executor, _binaryVisualizer);
      UnregisterFromExecutor(executor, _booleanVisualizer);
      UnregisterFromExecutor(executor, _comparisonVisualizer);
      UnregisterFromExecutor(executor, _unaryVisualizer);
    }

    private static void RegisterToExecutor<Event>(Executor executor,
                                                  IVisualizer<Event> visualizer) {
      if (!(visualizer is null)) {
        executor.Register(visualizer);
      }
    }

    private static void UnregisterFromExecutor<Event>(Executor executor,
                                                      IVisualizer<Event> visualizer) {
      if (!(visualizer is null)) {
        executor.Unregister(visualizer);
      }
    }

    private void WriteEvent(AbstractEvent e) {
      Console.BackgroundColor = ConsoleColor.White;
      Console.ForegroundColor = ConsoleColor.Black;
      switch (e) {
        case AssignmentEvent ae:
          Console.Write($"Assignment: {ae.Identifier} = {ae.Value}");
          break;
        case BinaryEvent be: {
            var op = _binaryOperatorStrings[be.Op];
            Console.Write($"Binary: {be.Left} {op} {be.Right} = {be.Result}");
            break;
          }
        case BooleanEvent be: {
            var op = _booleanOperatorStrings[be.Op];
            foreach (IValue value in be.Values) {
              string valueString = value.IsBoolean ? value.String : "?";
              Console.Write($"{op} {valueString} ");
            }
            Console.Write($"= {be.Result}");
            break;
          }
        case ComparisonEvent ce:
          Console.Write($"Comparison: {ce.First} ");
          for (int i = 0; i < ce.Ops.Count; i++) {
            string valueString = ce.Values[i].IsNumber ? ce.Values[i].String : "?";
            Console.Write($"{_comparisonOperatorStrings[ce.Ops[i]]} {valueString} ");
          }
          Console.Write($"= {ce.Result}");
          break;
        case UnaryEvent ue: {
            var op = _unaryOperatorStrings[ue.Op];
            Console.Write($"Unary: {op} {ue.Value} = {ue.Result}");
          }
          break;
        default:
          throw new NotImplementedException($"Unsupported event: {e}");
      }
      Console.ResetColor();
      Console.WriteLine();
    }
  }
}
