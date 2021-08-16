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
  // The interface of subscribable objects.
  internal interface ISubscribable<in Visualizer> {
    void Subscribe(Visualizer visualizer);
    void Unsubscribe(Visualizer visualizer);
  }

  // The event handler to notify all the subscribed visualizers when the event is triggered.
  public class EventHandler<Event> : ISubscribable<IVisualizer<Event>> {
    private readonly List<IVisualizer<Event>> _visualizers = new List<IVisualizer<Event>>();

    public void Subscribe(IVisualizer<Event> visualizer) {
      _visualizers.Add(visualizer);
    }

    public void Unsubscribe(IVisualizer<Event> visualizer) {
      _visualizers.Remove(visualizer);
    }

    public void Notify(Event e) {
      foreach (var visualizer in _visualizers) {
        visualizer.On(e);
      }
    }
  }
}
