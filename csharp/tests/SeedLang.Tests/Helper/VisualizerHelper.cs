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
using System.Text;
using SeedLang.Runtime;

namespace SeedLang.Tests.Helper {
  internal class VisualizerHelper {
    private class Visualizer<Event> : IVisualizer<Event> where Event : AbstractEvent {
      private readonly List<Event> _events = new List<Event>();

      public void On(Event e) {
        _events.Add(e);
      }

      internal string ToString(StringBuilder sb) {
        foreach (var e in _events) {
          EventToString(e, sb);
        }
        return sb.ToString();
      }
    }

    private readonly Dictionary<string, dynamic> _visualizers = new Dictionary<string, dynamic>();

    internal VisualizerHelper(IReadOnlyList<Type> eventTypes = null) {
      var visualizerType = typeof(Visualizer<>);
      foreach (var eventType in eventTypes is null ? typeof(Event).GetNestedTypes() : eventTypes) {
        var type = visualizerType.MakeGenericType(Type.GetType(eventType.AssemblyQualifiedName));
        _visualizers[eventType.Name] = Activator.CreateInstance(type);
      }
    }

    internal void RegisterToVisualizerCenter(VisualizerCenter vc) {
      foreach (var visualizer in _visualizers.Values) {
        vc.Register(visualizer);
      }
    }

    internal void UnregisterFromVisualizerCenter(VisualizerCenter vc) {
      foreach (var visualizer in _visualizers.Values) {
        vc.Unregister(visualizer);
      }
    }

    internal string EventsToString() {
      var sb = new StringBuilder();
      foreach (var visualizer in _visualizers.Values) {
        visualizer.ToString(sb);
      }
      return sb.ToString();
    }

    private static void EventToString<E>(E e, StringBuilder sb) {
      switch (e) {
        case Event.Assignment ae:
          sb.AppendLine($"{ae.Range} {ae.Name} = {ae.Value}");
          break;
        case Event.Binary be:
          sb.AppendLine($"{be.Range} {be.Left} {be.Op} {be.Right} = {be.Result}");
          break;
        case Event.Boolean be:
          sb.Append($"{be.Range} {be.Values[0]} ");
          for (int i = 1; i < be.Values.Count; ++i) {
            sb.Append($"{be.Op} {be.Values[i]} ");
          }
          sb.AppendLine($"= {be.Result}");
          break;
        case Event.Comparison ce:
          sb.Append($"{ce.Range} {ce.First} ");
          for (int i = 0; i < ce.Ops.Count; ++i) {
            sb.Append($"{ce.Ops[i]} {ce.Values[i]} ");
          }
          sb.AppendLine($"= {ce.Result}");
          break;
        case Event.FuncCalled fce:
          sb.AppendLine($"{fce.Range} FuncCalled: {fce.Name}({string.Join(", ", fce.Args)})");
          break;
        case Event.FuncReturned fre:
          sb.AppendLine($"{fre.Range} FuncReturned: {fre.Name} {fre.Result}");
          break;
        case Event.SingleStep sse:
          sb.AppendLine($"{sse.Range} SingleStep");
          break;
        case Event.Unary ue:
          sb.AppendLine($"{ue.Range} {ue.Op} {ue.Value} = {ue.Result}");
          break;
        case Event.VTagEntered vee:
          sb.AppendLine($"{vee.Range} {string.Join(",", vee.VTags)}");
          break;
        case Event.VTagExited vee:
          sb.AppendLine($"{vee.Range} {string.Join(",", vee.VTags)}");
          break;
        default:
          throw new NotImplementedException($"Unsupported event: {e.GetType()}");
      }
    }
  }
}
