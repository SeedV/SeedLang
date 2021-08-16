using System.Reflection;
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

using System.Collections.Generic;

namespace SeedLang.Runtime {
  // The visualizer center to observer execution events and dispatch them to the subscribed
  // visualizers.
  public class VisualizerCenter {
    public EventHandler<BinaryEvent> BinaryEvent { get; } = new EventHandler<BinaryEvent>();
    public EventHandler<EvalEvent> EvalEvent { get; } = new EventHandler<EvalEvent>();

    private readonly List<object> _eventHandlers = new List<object>();

    public VisualizerCenter() {
      // Collects all the event handlers into a list.
      foreach (var property in GetType().GetProperties()) {
        if (property.PropertyType.IsGenericType &&
            property.PropertyType.GetGenericTypeDefinition() == typeof(EventHandler<>)) {
          _eventHandlers.Add(property.GetValue(this));
        }
      }
    }

    // Subscribes a visualizer to this visualizer center. Loops each event handler, and subscribe to
    // it if the visualizer implement the IVisualizer interface of this event.
    public void Subscribe<Visualizer>(Visualizer visualizer) {
      foreach (var eventHandler in _eventHandlers) {
        if (eventHandler is ISubscribable<Visualizer> subscribable) {
          subscribable.Subscribe(visualizer);
        }
      }
    }

    // Unsubscribes a visualizer from this visualizer center. Loops each event handler, and
    // unsubscribe from it if the visualizer implement the IVisualizer interface of this event.
    public void Unsubscribe<Visualizer>(Visualizer visualizer) {
      foreach (var eventHandler in _eventHandlers) {
        if (eventHandler is ISubscribable<Visualizer> subscribable) {
          subscribable.Unsubscribe(visualizer);
        }
      }
    }
  }
}
