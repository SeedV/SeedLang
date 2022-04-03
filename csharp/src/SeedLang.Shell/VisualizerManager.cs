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
    VTagEntered,
    VTagExited,
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
    private readonly Visualizer<Event.Assignment> _assignmentVisualizer;
    private readonly Visualizer<Event.Binary> _binaryVisualizer;
    private readonly Visualizer<Event.Boolean> _booleanVisualizer;
    private readonly Visualizer<Event.Comparison> _comparisonVisualizer;
    private readonly Visualizer<Event.Unary> _unaryVisualizer;
    private readonly Visualizer<Event.VTagEntered> _vTagEnteredVisualizer;
    private readonly Visualizer<Event.VTagExited> _vTagExitedVisualizer;

    internal VisualizerManager(SourceCode source, IEnumerable<VisualizerType> visualizerTypes) {
      _source = source;
      // Generates an array with all visualizer types if All is included in the visualizerTypes.
      var visualizers = Enumerable.Contains(visualizerTypes, VisualizerType.All) ?
                        Enum.GetValues(typeof(VisualizerType)) :
                        visualizerTypes.ToArray();
      foreach (VisualizerType type in visualizers) {
        switch (type) {
          case VisualizerType.Assignment:
            _assignmentVisualizer = new Visualizer<Event.Assignment>(
                _source.WriteSourceWithHighlight, WriteEvent);
            break;
          case VisualizerType.Binary:
            _binaryVisualizer = new Visualizer<Event.Binary>(_source.WriteSourceWithHighlight,
                                                             WriteEvent);
            break;
          case VisualizerType.Boolean:
            _booleanVisualizer = new Visualizer<Event.Boolean>(_source.WriteSourceWithHighlight,
                                                               WriteEvent);
            break;
          case VisualizerType.Comparison:
            _comparisonVisualizer = new Visualizer<Event.Comparison>(
                _source.WriteSourceWithHighlight, WriteEvent);
            break;
          case VisualizerType.Unary:
            _unaryVisualizer = new Visualizer<Event.Unary>(_source.WriteSourceWithHighlight,
                                                           WriteEvent);
            break;
          case VisualizerType.VTagEntered:
            _vTagEnteredVisualizer = new Visualizer<Event.VTagEntered>(
                _source.WriteSourceWithHighlight, WriteEvent);
            break;
          case VisualizerType.VTagExited:
            _vTagExitedVisualizer = new Visualizer<Event.VTagExited>(
                _source.WriteSourceWithHighlight, WriteEvent);
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
      RegisterToExecutor(executor, _vTagEnteredVisualizer);
      RegisterToExecutor(executor, _vTagExitedVisualizer);
    }

    internal void UnregisterFromExecutor(Executor executor) {
      UnregisterFromExecutor(executor, _assignmentVisualizer);
      UnregisterFromExecutor(executor, _binaryVisualizer);
      UnregisterFromExecutor(executor, _booleanVisualizer);
      UnregisterFromExecutor(executor, _comparisonVisualizer);
      UnregisterFromExecutor(executor, _unaryVisualizer);
      UnregisterFromExecutor(executor, _vTagEnteredVisualizer);
      UnregisterFromExecutor(executor, _vTagExitedVisualizer);
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
        case Event.Assignment ae:
          Console.Write("Assign ");
          switch (ae.Type) {
            case VariableType.Global:
              Console.Write("global");
              break;
            case VariableType.Local:
              Console.Write("local");
              break;
          }
          Console.Write($" {ae.Name} = {ae.Value}");
          break;
        case Event.Binary be: {
            var op = _binaryOperatorStrings[be.Op];
            Console.Write($"Binary: {be.Left} {op} {be.Right} = {be.Result}");
            break;
          }
        case Event.Boolean be: {
            var op = _booleanOperatorStrings[be.Op];
            foreach (IValue value in be.Values) {
              string valueString = value.IsBoolean ? value.String : "?";
              Console.Write($"{op} {valueString} ");
            }
            Console.Write($"= {be.Result}");
            break;
          }
        case Event.Comparison ce:
          Console.Write($"Comparison: {ce.First} ");
          for (int i = 0; i < ce.Ops.Count; i++) {
            string valueString = ce.Values[i].IsNumber ? ce.Values[i].String : "?";
            Console.Write($"{_comparisonOperatorStrings[ce.Ops[i]]} {valueString} ");
          }
          Console.Write($"= {ce.Result}");
          break;
        case Event.Unary ue: {
            var op = _unaryOperatorStrings[ue.Op];
            Console.Write($"Unary: {op} {ue.Value} = {ue.Result}");
          }
          break;
        case Event.VTagEntered vee:
          Console.Write($"VTagEntered: {string.Join<Event.VTagEntered.VTagInfo>(",", vee.VTags)}");
          break;
        case Event.VTagExited _:
          Console.Write($"VTagExited");
          break;
        default:
          throw new NotImplementedException($"Unsupported event: {e}");
      }
      Console.ResetColor();
      Console.WriteLine();
    }
  }
}
