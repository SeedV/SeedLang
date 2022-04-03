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
using SeedLang.Common;
using SeedLang.Runtime;

namespace SeedLang.Interpreter {
  using Func = System.Func<uint, Value>;

  // The base class of all notification classes, which are used to store information for VISNOTIFY
  // instruction. The VM will create the correspding event object based on the information and send
  // to visualizers.
  internal abstract class Notification {
    protected readonly Range _range;

    internal Notification(Range range) {
      _range = range;
    }

    internal abstract void Notify(VisualizerCenter visualizerCenter, Func getRKValue);
  }

  internal class AssignmentNotification : Notification {
    private readonly string _name;
    private readonly VariableType _type;
    // The constant or register id of the assigned value.
    private readonly uint _valueId;

    public override string ToString() {
      return $"AssignmentNotification: '{_name}': {_type} {_valueId} {_range}";
    }

    internal AssignmentNotification(string name, VariableType type, uint valueId, Range range) :
        base(range) {
      _name = name;
      _type = type;
      _valueId = valueId;
    }

    internal override void Notify(VisualizerCenter visualizerCenter, Func getRKValue) {
      var ae = new AssignmentEvent(_name, _type, new ValueWrapper(getRKValue(_valueId)), _range);
      visualizerCenter.AssignmentPublisher.Notify(ae);
    }
  }

  internal class BinaryNotification : Notification {
    private readonly uint _leftId;
    private readonly BinaryOperator _op;
    private readonly uint _rightId;
    private readonly uint _resultId;

    public override string ToString() {
      return $"BinaryNotification: {_leftId} {_op} {_rightId} {_resultId} {_range}";
    }

    internal BinaryNotification(uint leftId, BinaryOperator op, uint rightId, uint resultId,
                                Range range) : base(range) {
      _leftId = leftId;
      _op = op;
      _rightId = rightId;
      _resultId = resultId;
    }

    internal override void Notify(VisualizerCenter visualizerCenter, Func getRKValue) {
      var be = new BinaryEvent(new ValueWrapper(getRKValue(_leftId)), _op,
                               new ValueWrapper(getRKValue(_rightId)),
                               new ValueWrapper(getRKValue(_resultId)), _range);
      visualizerCenter.BinaryPublisher.Notify(be);
    }
  }


  internal class UnaryNotification : Notification {
    private readonly UnaryOperator _op;
    private readonly uint _valueId;
    private readonly uint _resultId;

    public override string ToString() {
      return $"UnaryNotification: {_op} {_valueId} {_resultId} {_range}";
    }

    internal UnaryNotification(UnaryOperator op, uint valueId, uint resultId, Range range) :
        base(range) {
      _op = op;
      _valueId = valueId;
      _resultId = resultId;
    }

    internal override void Notify(VisualizerCenter visualizerCenter, Func getRKValue) {
      var ue = new UnaryEvent(_op, new ValueWrapper(getRKValue(_valueId)),
                              new ValueWrapper(getRKValue(_resultId)), _range);
      visualizerCenter.UnaryPublisher.Notify(ue);
    }
  }

  internal class VTagEnteredNotification : Notification {
    internal class VTagInfo {
      public string Name { get; }

      internal VTagInfo(string name) {
        Name = name;
      }

      public override string ToString() {
        return $"{Name}";
      }
    }

    private readonly VTagInfo[] _vTags;

    public override string ToString() {
      return $"VTagEnteredNotification: {string.Join<VTagInfo>(",", _vTags)} {_range}";
    }

    internal VTagEnteredNotification(VTagInfo[] vTags, Range range) : base(range) {
      _vTags = vTags;
    }

    internal override void Notify(VisualizerCenter visualizerCenter, Func getRKValue) {
      var vTags = new VTagEnteredEvent.VTagInfo[_vTags.Length];
      for (int i = 0; i < _vTags.Length; i++) {
        vTags[i] = new VTagEnteredEvent.VTagInfo(_vTags[i].Name);
      }
      visualizerCenter.VTagEnteredPublisher.Notify(new VTagEnteredEvent(vTags, _range));
    }
  }

  internal class VTagExitedNotification : Notification {

    public override string ToString() {
      return $"VTagExitedNotification: {_range}";
    }

    internal VTagExitedNotification(Range range) : base(range) {
    }

    internal override void Notify(VisualizerCenter visualizerCenter, Func getRKValue) {
      visualizerCenter.VTagExitedPublisher.Notify(new VTagExitedEvent(_range));
    }
  }
}
