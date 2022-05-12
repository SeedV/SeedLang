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
using System.Text;
using SeedLang.Common;
using SeedLang.Runtime;
using SeedLang.Visualization;

namespace SeedLang.Interpreter {
  internal static class Notification {
    // The base class of all notification classes, which are used to store information for VISNOTIFY
    // instructions. The VM will create the correspding event object based on the information and
    // then send to visualizers.
    internal abstract class AbstractNotification {
      internal abstract void Notify(VisualizerCenter visualizerCenter, VMProxy vm,
                                    Func<uint, VMValue> getRKValue, uint data, TextRange range);

      protected static void Notify<Event>(Event e, VisualizerCenter visualizerCenter, VMProxy vm) {
        visualizerCenter.Notify(e, vm);
        vm.Invalid();
      }
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

      internal override void Notify(VisualizerCenter visualizerCenter, VMProxy vm,
                                    Func<uint, VMValue> getRKValue, uint data, TextRange range) {
        var e = new Event.Assignment(_name, _type, new Value(getRKValue(_valueId)), range);
        Notify(e, visualizerCenter, vm);
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

      internal override void Notify(VisualizerCenter visualizerCenter, VMProxy vm,
                                    Func<uint, VMValue> getRKValue, uint data, TextRange range) {
        var e = new Event.Binary(new Value(getRKValue(_leftId)), _op,
                                 new Value(getRKValue(_rightId)), new Value(getRKValue(_resultId)),
                                 range);
        Notify(e, visualizerCenter, vm);
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

      internal override void Notify(VisualizerCenter visualizerCenter, VMProxy vm,
                                    Func<uint, VMValue> getRKValue, uint data, TextRange range) {
        Debug.Assert(Enum.IsDefined(typeof(Status), data));
        switch ((Status)data) {
          case Status.Called:
            var args = new Value[_argLength];
            uint argStartId = _funcId + 1;
            for (uint i = 0; i < _argLength; i++) {
              args[i] = new Value(getRKValue(argStartId + i));
            }
            Notify(new Event.FuncCalled(_name, args, range), visualizerCenter, vm);
            break;
          case Status.Returned:
            var e = new Event.FuncReturned(_name, new Value(getRKValue(_funcId)), range);
            Notify(e, visualizerCenter, vm);
            break;
        }
      }
    }

    internal class SingleStep : AbstractNotification {
      public override string ToString() {
        return $"Notification.SingleStep";
      }

      internal override void Notify(VisualizerCenter visualizerCenter, VMProxy vm,
                                    Func<uint, VMValue> getRKValue, uint data, TextRange range) {
        Notify(new Event.SingleStep(range), visualizerCenter, vm);
      }
    }

    internal class SubscriptAssignment : AbstractNotification {
      private readonly string _name;
      private readonly VariableType _type;
      private readonly uint _keyId;
      private readonly uint _valueId;

      internal SubscriptAssignment(string name, VariableType type, uint keyId, uint valueId) {
        _name = name;
        _type = type;
        _keyId = keyId;
        _valueId = valueId;
      }

      public override string ToString() {
        return $"Notification.SubscriptAssignment: '{_name}': {_type} {_keyId} {_valueId}";
      }

      internal override void Notify(VisualizerCenter visualizerCenter, VMProxy vm,
                                    Func<uint, VMValue> getRKValue, uint data, TextRange range) {
        var e = new Event.SubscriptAssignment(_name, _type, new Value(getRKValue(_keyId)),
                                              new Value(getRKValue(_valueId)), range);
        Notify(e, visualizerCenter, vm);
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

      internal override void Notify(VisualizerCenter visualizerCenter, VMProxy vm,
                                    Func<uint, VMValue> getRKValue, uint data, TextRange range) {
        var e = new Event.Unary(_op, new Value(getRKValue(_valueId)),
                                new Value(getRKValue(_resultId)), range);
        Notify(e, visualizerCenter, vm);
      }
    }

    public class VTagInfo {
      public string Name { get; }
      public string[] Args { get; }
      public uint?[] ValueIds { get; }

      public VTagInfo(string name, string[] args, uint?[] valueIds) {
        Debug.Assert(args.Length == valueIds.Length);
        Name = name;
        Args = args;
        ValueIds = valueIds;
      }

      public override string ToString() {
        var sb = new StringBuilder();
        sb.Append(Name);
        if (Args.Length > 0) {
          sb.Append('(');
          for (int i = 0; i < Args.Length; i++) {
            sb.Append(i > 0 ? ", " : "");
            sb.Append($"{Args[i]}: {ValueIds[i]?.ToString() ?? "null"}");
          }
          sb.Append(')');
        }
        return sb.ToString();
      }
    }

    internal abstract class VTag : AbstractNotification {
      private readonly VTagInfo[] _vTagInfos;

      internal VTag(VTagInfo[] vTagInfos) {
        _vTagInfos = vTagInfos;
      }

      public override string ToString() {
        return $"Notification.{GetType().Name}: {string.Join<VTagInfo>(", ", _vTagInfos)}";
      }

      internal override void Notify(VisualizerCenter visualizerCenter, VMProxy vm,
                                    Func<uint, VMValue> getRKValue, uint data, TextRange range) {
        var vTags = Array.ConvertAll(_vTagInfos, vTag => {
          var values = Array.ConvertAll(vTag.ValueIds, valueId =>
              valueId.HasValue ? new Value(getRKValue(valueId.Value)) : new Value());
          return new Visualization.VTagInfo(vTag.Name, vTag.Args, values);
        });
        Notify(visualizerCenter, vm, vTags, range);
      }

      protected abstract void Notify(VisualizerCenter visualizerCenter, VMProxy vm,
                                     IReadOnlyList<Visualization.VTagInfo> vTags, TextRange range);
    }

    internal class VTagEntered : VTag {
      internal VTagEntered(VTagInfo[] vTagInfos) : base(vTagInfos) { }

      protected override void Notify(VisualizerCenter visualizerCenter, VMProxy vm,
                                     IReadOnlyList<Visualization.VTagInfo> vTags, TextRange range) {
        visualizerCenter.Notify(new Event.VTagEntered(vTags, range), vm);
      }
    }

    internal class VTagExited : VTag {
      internal VTagExited(VTagInfo[] vTagInfos) : base(vTagInfos) { }

      protected override void Notify(VisualizerCenter visualizerCenter, VMProxy vm,
                                     IReadOnlyList<Visualization.VTagInfo> vTags, TextRange range) {
        visualizerCenter.Notify(new Event.VTagExited(vTags, range), vm);
      }
    }
  }
}
