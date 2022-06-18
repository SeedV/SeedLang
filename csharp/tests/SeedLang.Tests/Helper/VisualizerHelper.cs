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
using SeedLang.Visualization;

namespace SeedLang.Tests.Helper {
  // A helper class to manage visualizers.
  internal class VisualizerHelper {
    private class Visualizer<Event> : IVisualizer<Event> where Event : AbstractEvent {
      public IReadOnlyList<string> EventStrings => _eventStrings;

      private readonly List<string> _eventStrings = new List<string>();

      public void On(Event e, IVM vm) {
        _eventStrings.Add(e.ToString());
      }
    }

    private readonly Dictionary<string, dynamic> _visualizers = new Dictionary<string, dynamic>();

    public IReadOnlyList<string> EventStrings {
      get {
        var eventStrings = new List<string>();
        foreach (var visualizer in _visualizers.Values) {
          eventStrings.AddRange(visualizer.EventStrings);
        }
        return eventStrings;
      }
    }

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
  }
}
