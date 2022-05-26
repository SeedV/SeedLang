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
using System.Linq;
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

      // Notifies visualizers and then invalidate the VM proxy.
      protected static void Notify<Event>(Event e, VisualizerCenter visualizerCenter, VMProxy vm) {
        visualizerCenter.Notify(e, vm);
        vm.Invalid();
      }
    }

    internal sealed class Assignment : AbstractNotification, IEquatable<Assignment> {
      private readonly string _name;
      private readonly VariableType _type;
      // The constant or register id of the assigned value.
      private readonly uint _valueId;

      internal Assignment(string name, VariableType type, uint valueId) {
        _name = name;
        _type = type;
        _valueId = valueId;
      }

      public bool Equals(Assignment other) {
        if (other is null) {
          return false;
        }
        if (ReferenceEquals(this, other)) {
          return true;
        }
        return _name == other._name && _type == other._type && _valueId == other._valueId;
      }

      public override bool Equals(object obj) {
        return Equals(obj as Assignment);
      }

      public override int GetHashCode() {
        return new { _name, _type, _valueId, }.GetHashCode();
      }

      public override string ToString() {
        return $"Notification.{GetType().Name}: '{_name}': {_type} {_valueId}";
      }

      internal override void Notify(VisualizerCenter visualizerCenter, VMProxy vm,
                                    Func<uint, VMValue> getRKValue, uint data, TextRange range) {
        var e = new Event.Assignment(_name, _type, new Value(getRKValue(_valueId)), range);
        Notify(e, visualizerCenter, vm);
      }
    }

    internal sealed class Binary : AbstractNotification, IEquatable<Binary> {
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

      public bool Equals(Binary other) {
        if (other is null) {
          return false;
        }
        if (ReferenceEquals(this, other)) {
          return true;
        }
        return _leftId == other._leftId && _op == other._op &&
               _rightId == other._rightId && _resultId == other._resultId;
      }

      public override bool Equals(object obj) {
        return Equals(obj as Binary);
      }

      public override int GetHashCode() {
        return new { _leftId, _op, _rightId, _resultId, }.GetHashCode();
      }

      public override string ToString() {
        return $"Notification.{GetType().Name}: {_leftId} {_op} {_rightId} {_resultId}";
      }

      internal override void Notify(VisualizerCenter visualizerCenter, VMProxy vm,
                                    Func<uint, VMValue> getRKValue, uint data, TextRange range) {
        var e = new Event.Binary(new Value(getRKValue(_leftId)), _op,
                                 new Value(getRKValue(_rightId)), new Value(getRKValue(_resultId)),
                                 range);
        Notify(e, visualizerCenter, vm);
      }
    }

    internal class Function : AbstractNotification, IEquatable<Function> {
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

      public bool Equals(Function other) {
        if (other is null) {
          return false;
        }
        if (ReferenceEquals(this, other)) {
          return true;
        }
        return _name == other._name && _funcId == other._funcId && _argLength == other._argLength;
      }

      public override bool Equals(object obj) {
        return Equals(obj as Function);
      }

      public override int GetHashCode() {
        return new { _name, _funcId, _argLength, }.GetHashCode();
      }

      public override string ToString() {
        return $"Notification.{GetType().Name}: {_name} {_funcId} {_argLength}";
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

    internal sealed class SingleStep : AbstractNotification, IEquatable<SingleStep> {
      public bool Equals(SingleStep other) {
        if (other is null) {
          return false;
        }
        return true;
      }

      public override bool Equals(object obj) {
        return Equals(obj as SingleStep);
      }

      public override int GetHashCode() {
        return GetType().GetHashCode();
      }

      public override string ToString() {
        return $"Notification.{GetType().Name}";
      }

      internal override void Notify(VisualizerCenter visualizerCenter, VMProxy vm,
                                    Func<uint, VMValue> getRKValue, uint data, TextRange range) {
        Notify(new Event.SingleStep(range), visualizerCenter, vm);
      }
    }

    internal sealed class SubscriptAssignment : AbstractNotification, IEquatable<SubscriptAssignment> {
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

      public bool Equals(SubscriptAssignment other) {
        if (other is null) {
          return false;
        }
        if (ReferenceEquals(this, other)) {
          return true;
        }
        return _name == other._name && _type == other._type &&
               _keyId == other._keyId && _valueId == other._valueId;
      }

      public override bool Equals(object obj) {
        return Equals(obj as SubscriptAssignment);
      }

      public override int GetHashCode() {
        return new { _name, _type, _keyId, _valueId, }.GetHashCode();
      }

      public override string ToString() {
        return $"Notification.{GetType().Name}: '{_name}': {_type} {_keyId} {_valueId}";
      }

      internal override void Notify(VisualizerCenter visualizerCenter, VMProxy vm,
                                    Func<uint, VMValue> getRKValue, uint data, TextRange range) {
        var e = new Event.SubscriptAssignment(_name, _type, new Value(getRKValue(_keyId)),
                                              new Value(getRKValue(_valueId)), range);
        Notify(e, visualizerCenter, vm);
      }
    }

    internal sealed class Unary : AbstractNotification, IEquatable<Unary> {
      private readonly UnaryOperator _op;
      private readonly uint _valueId;
      private readonly uint _resultId;

      internal Unary(UnaryOperator op, uint valueId, uint resultId) {
        _op = op;
        _valueId = valueId;
        _resultId = resultId;
      }

      public bool Equals(Unary other) {
        if (other is null) {
          return false;
        }
        if (ReferenceEquals(this, other)) {
          return true;
        }
        return _op == other._op && _valueId == other._valueId && _resultId == other._resultId;
      }

      public override bool Equals(object obj) {
        return Equals(obj as Unary);
      }

      public override int GetHashCode() {
        return new { _op, _valueId, _resultId, }.GetHashCode();
      }

      public override string ToString() {
        return $"Notification.{GetType().Name}: {_op} {_valueId} {_resultId}";
      }

      internal override void Notify(VisualizerCenter visualizerCenter, VMProxy vm,
                                    Func<uint, VMValue> getRKValue, uint data, TextRange range) {
        var e = new Event.Unary(_op, new Value(getRKValue(_valueId)),
                                new Value(getRKValue(_resultId)), range);
        Notify(e, visualizerCenter, vm);
      }
    }

    internal sealed class Variable : AbstractNotification, IEquatable<Variable> {
      private readonly string _name;
      private readonly VariableType _type;

      internal Variable(string name, VariableType type) {
        _name = name;
        _type = type;
      }

      public bool Equals(Variable other) {
        if (other is null) {
          return false;
        }
        if (ReferenceEquals(this, other)) {
          return true;
        }
        return _name == other._name && _type == other._type;
      }

      public override bool Equals(object obj) {
        return Equals(obj as Variable);
      }

      public override int GetHashCode() {
        return new { _name, _type, }.GetHashCode();
      }

      public override string ToString() {
        return $"Notification.{GetType().Name}: {_name} {_type}";
      }

      internal override void Notify(VisualizerCenter visualizerCenter, VMProxy vm,
                                    Func<uint, VMValue> getRKValue, uint data, TextRange range) {
        Notify(new Event.VariableDefined(_name, _type, range), visualizerCenter, vm);
      }
    }

    public sealed class VTagInfo : IEquatable<VTagInfo> {
      public string Name { get; }
      public string[] Args { get; }
      public uint?[] ValueIds { get; }

      public VTagInfo(string name, string[] args, uint?[] valueIds) {
        Debug.Assert(args.Length == valueIds.Length);
        Name = name;
        Args = args;
        ValueIds = valueIds;
      }

      public bool Equals(VTagInfo other) {
        if (other is null) {
          return false;
        }
        if (ReferenceEquals(this, other)) {
          return true;
        }
        return Name == other.Name &&
               Enumerable.SequenceEqual(Args, other.Args) &&
               Enumerable.SequenceEqual(ValueIds, other.ValueIds);
      }

      public override bool Equals(object obj) {
        return Equals(obj as VTagInfo);
      }

      public override int GetHashCode() {
        var hashCode = new HashCode();
        hashCode.Add(Name);
        foreach (string arg in Args) {
          hashCode.Add(arg);
        }
        foreach (uint? valueId in ValueIds) {
          hashCode.Add(valueId);
        }
        return hashCode.ToHashCode();
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

    internal sealed class VTag : AbstractNotification, IEquatable<VTag> {
      internal enum Status : uint {
        Entered,
        Exited,
      }

      private readonly VTagInfo[] _vTagInfos;

      internal VTag(VTagInfo[] vTagInfos) {
        _vTagInfos = vTagInfos;
      }

      public bool Equals(VTag other) {
        if (other is null) {
          return false;
        }
        if (ReferenceEquals(this, other)) {
          return true;
        }
        return Enumerable.SequenceEqual(_vTagInfos, other._vTagInfos);
      }

      public override bool Equals(object obj) {
        return Equals(obj as VTag);
      }

      public override int GetHashCode() {
        var hashCode = new HashCode();
        foreach (VTagInfo info in _vTagInfos) {
          hashCode.Add(info);
        }
        return hashCode.ToHashCode();
      }

      public override string ToString() {
        return $"Notification.{GetType().Name}: {string.Join<VTagInfo>(", ", _vTagInfos)}";
      }

      internal override void Notify(VisualizerCenter visualizerCenter, VMProxy vm,
                                    Func<uint, VMValue> getRKValue, uint data, TextRange range) {
        var vTags = Array.ConvertAll(_vTagInfos, vTagInfo => {
          var values = Array.ConvertAll(vTagInfo.ValueIds, valueId =>
              valueId.HasValue ? new Value(getRKValue(valueId.Value)) : new Value());
          return new Visualization.VTagInfo(vTagInfo.Name, vTagInfo.Args, values);
        });
        switch ((Status)data) {
          case Status.Entered:
            Notify(new Event.VTagEntered(vTags, range), visualizerCenter, vm);
            break;
          case Status.Exited:
            Notify(new Event.VTagExited(vTags, range), visualizerCenter, vm);
            break;
        }
      }
    }
  }
}
