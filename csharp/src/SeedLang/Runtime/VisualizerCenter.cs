using System.IO;
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
using System.Diagnostics;

namespace SeedLang.Runtime {
  // The visualizer center to observe execution events and dispatch them to the subscribed
  // visualizers.
  internal class VisualizerCenter {
    // The interface of the event publisher.
    private interface IPublisher<in Visualizer> {
      // Registers a visualizer into this event publisher.
      void Register(Visualizer visualizer);

      // Unregisters a visualizer into this event publisher.
      void Unregister(Visualizer visualizer);
    }

    // The publisher to notify all the registered visualizers when the event is triggered.
    private class Publisher<Event> : IPublisher<IVisualizer<Event>> {
      private readonly List<IVisualizer<Event>> _visualizers = new List<IVisualizer<Event>>();

      public void Register(IVisualizer<Event> visualizer) {
        if (!_visualizers.Contains(visualizer)) {
          _visualizers.Add(visualizer);
        }
      }

      public void Unregister(IVisualizer<Event> visualizer) {
        _visualizers.Remove(visualizer);
      }

      internal bool IsEmpty() {
        return _visualizers.Count == 0;
      }

      internal void Notify(Event e) {
        foreach (var visualizer in _visualizers) {
          visualizer.On(e);
        }
      }
    }

    private readonly Dictionary<Type, object> _publishers = new Dictionary<Type, object>();

    internal VisualizerCenter() {
      Type[] eventTypes = typeof(Event).GetNestedTypes();
      foreach (Type type in eventTypes) {
        Type publisherType = typeof(Publisher<>).MakeGenericType(new Type[] { type });
        _publishers[type] = Activator.CreateInstance(publisherType);
      }
    }

    internal bool HasVisualizer<Event>() {
      return !(_publishers[typeof(Event)] as Publisher<Event>).IsEmpty();
    }

    internal void Notify<Event>(Event e) {
      (_publishers[typeof(Event)] as Publisher<Event>).Notify(e);
    }

    // Registers a visualizer into this visualizer center.
    //
    // Loops each event publisher, and registers into it if the visualizer implements the
    // IVisualizer interface of this event.
    // The parameter of IPublisher<in Visualizer> must be contravariance with a "in" modifier, so
    // that the Visualizer can be cast to IVisualizer<Event> interface to make the check work.
    internal void Register<Visualizer>(Visualizer visualizer) {
      foreach (var keyValue in _publishers) {
        if (keyValue.Value is IPublisher<Visualizer> publisher) {
          publisher.Register(visualizer);
        }
      }
    }

    // Unregisters a visualizer from this visualizer center.
    //
    // Loops each event publisher, and unregisters from it if the visualizer implements the
    // IVisualizer interface of this event.
    internal void Unregister<Visualizer>(Visualizer visualizer) {
      foreach (var keyValue in _publishers) {
        if (keyValue.Value is IPublisher<Visualizer> publisher) {
          publisher.Unregister(visualizer);
        }
      }
    }
  }
}
