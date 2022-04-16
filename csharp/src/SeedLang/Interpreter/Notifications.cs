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
  using Array = System.Array;
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

    internal class FuncCalled : AbstractNotification {
      private readonly string _name;
      private readonly uint _argStartId;
      private readonly uint _argLength;

      public override string ToString() {
        return $"Notification.FuncCalled: {_name} {_argStartId} {_argLength} {_range}";
      }

      internal FuncCalled(string name, uint argStartId, uint argLength, Range range) : base(range) {
        _name = name;
        _argStartId = argStartId;
        _argLength = argLength;
      }

      internal override void Notify(VisualizerCenter visualizerCenter, Func getRKValue) {
        var args = new ValueWrapper[_argLength];
        for (uint i = 0; i < _argLength; i++) {
          args[i] = new ValueWrapper(getRKValue(_argStartId + i));
        }
        var fce = new Event.FuncCalled(_name, args, _range);
        visualizerCenter.Notify(fce);
      }
    }

    internal class FuncReturned : AbstractNotification {
      private readonly string _name;
      private readonly uint? _resultId;

      public override string ToString() {
        return $"Notification.FuncReturned: {_name} {_resultId} {_range}";
      }

      internal FuncReturned(string name, uint? resultId, Range range) : base(range) {
        _name = name;
        _resultId = resultId;
      }

      internal override void Notify(VisualizerCenter visualizerCenter, Func getRKValue) {
        var result = new ValueWrapper(_resultId.HasValue ? getRKValue((uint)_resultId) :
                                                           new Value());
        var fre = new Event.FuncReturned(_name, result, _range);
        visualizerCenter.Notify(fre);
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

    internal class VTagEntered : AbstractNotification {
      private readonly Event.VTagEntered.VTagInfo[] _vTagInfos;

      public override string ToString() {
        var vTagInfoStr = string.Join<Event.VTagEntered.VTagInfo>(",", _vTagInfos);
        return $"Notification.VTagEntered: {vTagInfoStr} {_range}";
      }

      internal VTagEntered(Event.VTagEntered.VTagInfo[] vTagInfos, Range range) :
          base(range) {
        _vTagInfos = vTagInfos;
      }

      internal override void Notify(VisualizerCenter visualizerCenter, Func getRKValue) {
        visualizerCenter.Notify(new Event.VTagEntered(_vTagInfos, _range));
      }
    }

    internal class VTagExited : AbstractNotification {
      public class VTagInfo {
        public string Name { get; }
        public uint[] ValueIds { get; }

        public VTagInfo(string name, uint[] valueIds) {
          Name = name;
          ValueIds = valueIds;
        }

        public override string ToString() {
          return Name + (ValueIds.Length > 0 ? $"({string.Join(",", ValueIds)})" : "");
        }
      }
      private readonly VTagInfo[] _vTagInfos;

      public override string ToString() {
        return $"Notification.VTagExited: {string.Join<VTagInfo>(",", _vTagInfos)} {_range}";
      }

      internal VTagExited(VTagInfo[] vTagInfos, Range range) : base(range) {
        _vTagInfos = vTagInfos;
      }

      internal override void Notify(VisualizerCenter visualizerCenter, Func getRKValue) {
        var vTags = Array.ConvertAll(_vTagInfos, vTag => {
          var values = Array.ConvertAll(vTag.ValueIds,
                                        valueId => new ValueWrapper(getRKValue(valueId)));
          return new Event.VTagExited.VTagInfo(vTag.Name, values);
        });
        visualizerCenter.Notify(new Event.VTagExited(vTags, _range));
      }
    }
  }
}
