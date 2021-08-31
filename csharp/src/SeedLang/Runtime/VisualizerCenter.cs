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
  // The visualizer center to observe execution events and dispatch them to the subscribed
  // visualizers.
  internal class VisualizerCenter {
    public Publisher<BinaryEvent> BinaryPublisher { get; } = new Publisher<BinaryEvent>();
    public Publisher<AssignmentEvent> AssignmentPublisher { get; } =
        new Publisher<AssignmentEvent>();
    public Publisher<EvalEvent> EvalPublisher { get; } = new Publisher<EvalEvent>();

    private readonly List<object> _publishers = new List<object>();

    internal VisualizerCenter() {
      // Collects all the event handlers into a list.
      foreach (var property in GetType().GetProperties()) {
        if (property.PropertyType.IsGenericType &&
            property.PropertyType.GetGenericTypeDefinition() == typeof(Publisher<>)) {
          _publishers.Add(property.GetValue(this));
        }
      }
    }

    // Registers a visualizer into this visualizer center.
    //
    // Loops each event publisher, and registers into it if the visualizer implements the
    // IVisualizer interface of this event.
    // The parameter of IPublisher<in Visualizer> must be contravariance with a "in" modifier, so
    // that the Visualizer can be cast to IVisualizer<Event> interface to make the check work.
    internal void Register<Visualizer>(Visualizer visualizer) {
      foreach (var p in _publishers) {
        if (p is IPublisher<Visualizer> publisher) {
          publisher.Register(visualizer);
        }
      }
    }

    // Unregisters a visualizer from this visualizer center.
    //
    // Loops each event publisher, and unregisters from it if the visualizer implements the
    // IVisualizer interface of this event.
    internal void Unregister<Visualizer>(Visualizer visualizer) {
      foreach (var p in _publishers) {
        if (p is IPublisher<Visualizer> publisher) {
          publisher.Unregister(visualizer);
        }
      }
    }
  }
}
