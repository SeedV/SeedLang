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

using SeedLang.Common;
using SeedLang.Runtime;

namespace SeedLang.Interpreter {
  using Func = System.Func<uint, Value>;

  internal static class Notification {
    // The base class of all notification classes, which are used to store information for VISNOTIFY
    // instruction. The VM will create the correspding event object based on the information and
    // send to visualizers.
    internal abstract class AbstractNotification {
      protected readonly Range _range;

      internal AbstractNotification(Range range) {
        _range = range;
      }

      internal abstract void Notify(VisualizerCenter visualizerCenter, Func getRKValue);
    }

    internal class Assignment : AbstractNotification {
      private readonly string _name;
      private readonly VariableType _type;
      // The constant or register id of the assigned value.
      private readonly uint _valueId;

      public override string ToString() {
        return $"Notification.Assignment: '{_name}': {_type} {_valueId} {_range}";
      }

      internal Assignment(string name, VariableType type, uint valueId, Range range) :
          base(range) {
        _name = name;
        _type = type;
        _valueId = valueId;
      }

      internal override void Notify(VisualizerCenter visualizerCenter, Func getRKValue) {
        var ae = new Event.Assignment(_name, _type, new ValueWrapper(getRKValue(_valueId)), _range);
        visualizerCenter.Notify(ae);
      }
    }

    internal class Binary : AbstractNotification {
      private readonly uint _leftId;
      private readonly BinaryOperator _op;
      private readonly uint _rightId;
      private readonly uint _resultId;

      public override string ToString() {
        return $"Notification.Binary: {_leftId} {_op} {_rightId} {_resultId} {_range}";
      }

      internal Binary(uint leftId, BinaryOperator op, uint rightId, uint resultId,
                                  Range range) : base(range) {
        _leftId = leftId;
        _op = op;
        _rightId = rightId;
        _resultId = resultId;
      }

      internal override void Notify(VisualizerCenter visualizerCenter, Func getRKValue) {
        var be = new Event.Binary(new ValueWrapper(getRKValue(_leftId)), _op,
                                  new ValueWrapper(getRKValue(_rightId)),
                                  new ValueWrapper(getRKValue(_resultId)), _range);
        visualizerCenter.Notify(be);
      }
    }


    internal class Unary : AbstractNotification {
      private readonly UnaryOperator _op;
      private readonly uint _valueId;
      private readonly uint _resultId;

      public override string ToString() {
        return $"Notification.Unary: {_op} {_valueId} {_resultId} {_range}";
      }

      internal Unary(UnaryOperator op, uint valueId, uint resultId, Range range) :
          base(range) {
        _op = op;
        _valueId = valueId;
        _resultId = resultId;
      }

      internal override void Notify(VisualizerCenter visualizerCenter, Func getRKValue) {
        var ue = new Event.Unary(_op, new ValueWrapper(getRKValue(_valueId)),
                                 new ValueWrapper(getRKValue(_resultId)), _range);
        visualizerCenter.Notify(ue);
      }
    }

    // TODO: add properties for parameter information. This class is different from Event.VTagInfo,
    // because only register or constant ids can be get for parameters during compilation. The final
    // result value of each parameter can be get during execution, and set into the Event.VTagInfo.
    internal class VTagInfo {
      public string Name { get; }

      internal VTagInfo(string name) {
        Name = name;
      }

      public override string ToString() {
        return $"{Name}";
      }
    }

    internal class VTagEntered : AbstractNotification {
      private readonly VTagInfo[] _vTags;

      public override string ToString() {
        return $"Notification.VTagEntered: {string.Join<VTagInfo>(",", _vTags)} {_range}";
      }

      internal VTagEntered(VTagInfo[] vTags, Range range) : base(range) {
        _vTags = vTags;
      }

      internal override void Notify(VisualizerCenter visualizerCenter, Func getRKValue) {
        var vTags = System.Array.ConvertAll(_vTags, vTag => new Event.VTagInfo(vTag.Name));
        visualizerCenter.Notify(new Event.VTagEntered(vTags, _range));
      }
    }

    internal class VTagExited : AbstractNotification {
      private readonly VTagInfo[] _vTags;

      public override string ToString() {
        return $"Notification.VTagExited: {string.Join<VTagInfo>(",", _vTags)} {_range}";
      }

      internal VTagExited(VTagInfo[] vTags, Range range) : base(range) {
        _vTags = vTags;
      }

      internal override void Notify(VisualizerCenter visualizerCenter, Func getRKValue) {
        var vTags = System.Array.ConvertAll(_vTags, vTag => new Event.VTagInfo(vTag.Name));
        visualizerCenter.Notify(new Event.VTagExited(vTags, _range));
      }
    }
  }
}
