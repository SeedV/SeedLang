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
using System.Diagnostics;
using SeedLang.Runtime;
using SeedLang.Common;

namespace SeedLang.Interpreter {
  using Func = Func<uint, Value>;

  internal static class Notification {
    // The base class of all notification classes, which are used to store information for VISNOTIFY
    // instructions. The VM will create the correspding event object based on the information and
    // then send to visualizers.
    internal abstract class AbstractNotification {
      internal abstract void Notify(VisualizerCenter visualizerCenter, Func getRKValue, uint data,
                                    TextRange range);
    }

    internal class Assignment : AbstractNotification {
      private readonly string _name;
      private readonly VariableType _type;
      // The constant or register id of the assigned value.
      private readonly uint _valueId;

      internal Assignment(string name, VariableType type, uint valueId) {
        _name = name;
        _type = type;
        _valueId = valueId;
      }

      public override string ToString() {
        return $"Notification.Assignment: '{_name}': {_type} {_valueId}";
      }

      internal override void Notify(VisualizerCenter visualizerCenter, Func getRKValue, uint data,
                                    TextRange range) {
        var ae = new Event.Assignment(_name, _type, new ValueWrapper(getRKValue(_valueId)), range);
        visualizerCenter.Notify(ae);
      }
    }

    internal class Binary : AbstractNotification {
      private readonly uint _leftId;
      private readonly BinaryOperator _op;
      private readonly uint _rightId;
      private readonly uint _resultId;

      internal Binary(uint leftId, BinaryOperator op, uint rightId, uint resultId) {
        _leftId = leftId;
        _op = op;
        _rightId = rightId;
        _resultId = resultId;
      }

      public override string ToString() {
        return $"Notification.Binary: {_leftId} {_op} {_rightId} {_resultId}";
      }

      internal override void Notify(VisualizerCenter visualizerCenter, Func getRKValue, uint data,
                                    TextRange range) {
        var be = new Event.Binary(new ValueWrapper(getRKValue(_leftId)), _op,
                                  new ValueWrapper(getRKValue(_rightId)),
                                  new ValueWrapper(getRKValue(_resultId)), range);
        visualizerCenter.Notify(be);
      }
    }

    internal class Function : AbstractNotification {
      internal enum Status : uint {
        Called,
        Returned,
      }

      private readonly string _name;
      private readonly uint _funcId;
      private readonly uint _argLength;

      internal Function(string name, uint funcId, uint argLength) {
        _name = name;
        _funcId = funcId;
        _argLength = argLength;
      }

      public override string ToString() {
        return $"Notification.Function: {_name} {_funcId} {_argLength}";
      }

      internal override void Notify(VisualizerCenter visualizerCenter, Func getRKValue, uint data,
                                    TextRange range) {
        Debug.Assert(Enum.IsDefined(typeof(Status), data));
        switch ((Status)data) {
          case Status.Called:
            var args = new ValueWrapper[_argLength];
            uint argStartId = _funcId + 1;
            for (uint i = 0; i < _argLength; i++) {
              args[i] = new ValueWrapper(getRKValue(argStartId + i));
            }
            var fce = new Event.FuncCalled(_name, args, range);
            visualizerCenter.Notify(fce);
            break;
          case Status.Returned:
            var fre = new Event.FuncReturned(_name, new ValueWrapper(getRKValue(_funcId)), range);
            visualizerCenter.Notify(fre);
            break;
        }
      }
    }

    internal class SingleStep : AbstractNotification {
      public override string ToString() {
        return $"Notification.SingleStep";
      }

      internal override void Notify(VisualizerCenter visualizerCenter, Func getRKValue, uint data,
                                    TextRange range) {
        visualizerCenter.Notify(new Event.SingleStep(range));
      }
    }

    internal class Unary : AbstractNotification {
      private readonly UnaryOperator _op;
      private readonly uint _valueId;
      private readonly uint _resultId;

      internal Unary(UnaryOperator op, uint valueId, uint resultId) {
        _op = op;
        _valueId = valueId;
        _resultId = resultId;
      }

      public override string ToString() {
        return $"Notification.Unary: {_op} {_valueId} {_resultId}";
      }

      internal override void Notify(VisualizerCenter visualizerCenter, Func getRKValue, uint data,
                                    TextRange range) {
        var ue = new Event.Unary(_op, new ValueWrapper(getRKValue(_valueId)),
                                 new ValueWrapper(getRKValue(_resultId)), range);
        visualizerCenter.Notify(ue);
      }
    }

    internal class VTagEntered : AbstractNotification {
      private readonly Event.VTagEntered.VTagInfo[] _vTagInfos;

      internal VTagEntered(Event.VTagEntered.VTagInfo[] vTagInfos) {
        _vTagInfos = vTagInfos;
      }

      public override string ToString() {
        var vTagInfoStr = string.Join<Event.VTagEntered.VTagInfo>(",", _vTagInfos);
        return $"Notification.VTagEntered: {vTagInfoStr}";
      }

      internal override void Notify(VisualizerCenter visualizerCenter, Func getRKValue, uint data,
                                    TextRange range) {
        visualizerCenter.Notify(new Event.VTagEntered(_vTagInfos, range));
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

      internal VTagExited(VTagInfo[] vTagInfos) {
        _vTagInfos = vTagInfos;
      }

      public override string ToString() {
        return $"Notification.VTagExited: {string.Join<VTagInfo>(",", _vTagInfos)}";
      }

      internal override void Notify(VisualizerCenter visualizerCenter, Func getRKValue, uint data,
                                    TextRange range) {
        var vTags = Array.ConvertAll(_vTagInfos, vTag => {
          var values = Array.ConvertAll(vTag.ValueIds,
                                        valueId => new ValueWrapper(getRKValue(valueId)));
          return new Event.VTagExited.VTagInfo(vTag.Name, values);
        });
        visualizerCenter.Notify(new Event.VTagExited(vTags, range));
      }
    }
  }
}
