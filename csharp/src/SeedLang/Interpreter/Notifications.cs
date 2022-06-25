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
using SeedLang.Runtime;
using SeedLang.Visualization;

namespace SeedLang.Interpreter {
  internal static class Notification {
    // The base class of all notification classes, which are used to store information for VISNOTIFY
    // instructions. The VM will create the corresponding event object based on the information and
    // then send to visualizers.
    internal abstract class AbstractNotification {
      internal abstract void Accept(VM vm);
    }

    internal sealed class Assignment : AbstractNotification, IEquatable<Assignment> {
      public string Name { get; }
      public VariableType Type { get; }
      // The constant or register id of the assigned value.
      public uint ValueId { get; }

      internal Assignment(string name, VariableType type, uint valueId) {
        Name = name;
        Type = type;
        ValueId = valueId;
      }

      public static bool operator ==(Assignment lhs, Assignment rhs) {
        return lhs.Equals(rhs);
      }

      public static bool operator !=(Assignment lhs, Assignment rhs) {
        return !(lhs == rhs);
      }

      public bool Equals(Assignment other) {
        if (other is null) {
          return false;
        }
        if (ReferenceEquals(this, other)) {
          return true;
        }
        return Name == other.Name && Type == other.Type && ValueId == other.ValueId;
      }

      public override bool Equals(object obj) {
        return Equals(obj as Assignment);
      }

      public override int GetHashCode() {
        return new { Name, Type, ValueId, }.GetHashCode();
      }

      public override string ToString() {
        return $"Notification.{GetType().Name}: '{Name}' {Type} {ValueId}";
      }

      internal override void Accept(VM vm) {
        vm.HandleAssignment(this);
      }
    }

    internal sealed class Binary : AbstractNotification, IEquatable<Binary> {
      public uint LeftId { get; }
      public BinaryOperator Op { get; }
      public uint RightId { get; }
      public uint ResultId { get; }

      internal Binary(uint leftId, BinaryOperator op, uint rightId, uint resultId) {
        LeftId = leftId;
        Op = op;
        RightId = rightId;
        ResultId = resultId;
      }

      public static bool operator ==(Binary lhs, Binary rhs) {
        return lhs.Equals(rhs);
      }

      public static bool operator !=(Binary lhs, Binary rhs) {
        return !(lhs == rhs);
      }

      public bool Equals(Binary other) {
        if (other is null) {
          return false;
        }
        if (ReferenceEquals(this, other)) {
          return true;
        }
        return LeftId == other.LeftId && Op == other.Op &&
               RightId == other.RightId && ResultId == other.ResultId;
      }

      public override bool Equals(object obj) {
        return Equals(obj as Binary);
      }

      public override int GetHashCode() {
        return new { LeftId, Op, RightId, ResultId, }.GetHashCode();
      }

      public override string ToString() {
        return $"Notification.{GetType().Name}: {LeftId} {Op} {RightId} {ResultId}";
      }

      internal override void Accept(VM vm) {
        vm.HandleBinary(this);
      }
    }

    internal sealed class Comparison : AbstractNotification, IEquatable<Comparison> {
      public uint LeftId { get; }
      public ComparisonOperator Op { get; }
      public uint RightId { get; }

      internal Comparison(uint leftId, ComparisonOperator op, uint rightId) {
        LeftId = leftId;
        Op = op;
        RightId = rightId;
      }

      public static bool operator ==(Comparison lhs, Comparison rhs) {
        return lhs.Equals(rhs);
      }

      public static bool operator !=(Comparison lhs, Comparison rhs) {
        return !(lhs == rhs);
      }

      public bool Equals(Comparison other) {
        if (other is null) {
          return false;
        }
        if (ReferenceEquals(this, other)) {
          return true;
        }
        return LeftId == other.LeftId && Op == other.Op && RightId == other.RightId;
      }

      public override bool Equals(object obj) {
        return Equals(obj as Comparison);
      }

      public override int GetHashCode() {
        return new { LeftId, Op, RightId, }.GetHashCode();
      }

      public override string ToString() {
        return $"Notification.{GetType().Name}: {LeftId} {Op} {RightId}";
      }

      internal override void Accept(VM vm) {
        vm.HandleComparison(this);
      }
    }

    internal sealed class ElementLoaded : AbstractNotification, IEquatable<ElementLoaded> {
      public uint TargetId { get; }
      public uint ContainerId { get; }
      public uint KeyId { get; }

      internal ElementLoaded(uint targetId, uint containerId, uint keyId) {
        TargetId = targetId;
        ContainerId = containerId;
        KeyId = keyId;
      }

      public static bool operator ==(ElementLoaded lhs, ElementLoaded rhs) {
        return lhs.Equals(rhs);
      }

      public static bool operator !=(ElementLoaded lhs, ElementLoaded rhs) {
        return !(lhs == rhs);
      }

      public bool Equals(ElementLoaded other) {
        if (other is null) {
          return false;
        }
        if (ReferenceEquals(this, other)) {
          return true;
        }
        return TargetId == other.TargetId && ContainerId == other.ContainerId &&
               KeyId == other.KeyId;
      }

      public override bool Equals(object obj) {
        return Equals(obj as ElementLoaded);
      }

      public override int GetHashCode() {
        return new { TargetId, ContainerId, KeyId, }.GetHashCode();
      }

      public override string ToString() {
        return $"Notification.{GetType().Name}: {TargetId} {ContainerId} {KeyId}";
      }

      internal override void Accept(VM vm) {
        vm.HandleElementLoaded(this);
      }
    }

    internal sealed class Function : AbstractNotification, IEquatable<Function> {
      internal enum Status : uint {
        Called,
        Returned,
      }

      public string Name { get; }
      public uint FuncId { get; }
      public uint ArgLength { get; }

      internal Function(string name, uint funcId, uint argLength) {
        Name = name;
        FuncId = funcId;
        ArgLength = argLength;
      }

      public static bool operator ==(Function lhs, Function rhs) {
        return lhs.Equals(rhs);
      }

      public static bool operator !=(Function lhs, Function rhs) {
        return !(lhs == rhs);
      }

      public bool Equals(Function other) {
        if (other is null) {
          return false;
        }
        if (ReferenceEquals(this, other)) {
          return true;
        }
        return Name == other.Name && FuncId == other.FuncId && ArgLength == other.ArgLength;
      }

      public override bool Equals(object obj) {
        return Equals(obj as Function);
      }

      public override int GetHashCode() {
        return new { Name, FuncId, ArgLength, }.GetHashCode();
      }

      public override string ToString() {
        return $"Notification.{GetType().Name}: {Name} {FuncId} {ArgLength}";
      }

      internal override void Accept(VM vm) {
        vm.HandleFunction(this);
      }
    }

    internal sealed class GlobalLoaded : AbstractNotification, IEquatable<GlobalLoaded> {
      public uint TargetId { get; }
      public string Name { get; }

      internal GlobalLoaded(uint targetId, string name) {
        TargetId = targetId;
        Name = name;
      }

      public static bool operator ==(GlobalLoaded lhs, GlobalLoaded rhs) {
        return lhs.Equals(rhs);
      }

      public static bool operator !=(GlobalLoaded lhs, GlobalLoaded rhs) {
        return !(lhs == rhs);
      }

      public bool Equals(GlobalLoaded other) {
        if (other is null) {
          return false;
        }
        if (ReferenceEquals(this, other)) {
          return true;
        }
        return TargetId == other.TargetId && Name == other.Name;
      }

      public override bool Equals(object obj) {
        return Equals(obj as GlobalLoaded);
      }

      public override int GetHashCode() {
        return new { TargetId, Name, }.GetHashCode();
      }

      public override string ToString() {
        return $"Notification.{GetType().Name}: {TargetId} '{Name}'";
      }

      internal override void Accept(VM vm) {
        vm.HandleGlobalLoaded(this);
      }
    }

    internal sealed class SingleStep : AbstractNotification, IEquatable<SingleStep> {
      public static bool operator ==(SingleStep lhs, SingleStep rhs) {
        return lhs.Equals(rhs);
      }

      public static bool operator !=(SingleStep lhs, SingleStep rhs) {
        return !(lhs == rhs);
      }

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

      internal override void Accept(VM vm) {
        vm.HandleSingleStep(this);
      }
    }

    internal sealed class SubscriptAssignment :
        AbstractNotification, IEquatable<SubscriptAssignment> {
      public uint ContainerId { get; }
      public uint KeyId { get; }
      public uint ValueId { get; }

      internal SubscriptAssignment(uint containerId, uint keyId, uint valueId) {
        ContainerId = containerId;
        KeyId = keyId;
        ValueId = valueId;
      }

      public static bool operator ==(SubscriptAssignment lhs, SubscriptAssignment rhs) {
        return lhs.Equals(rhs);
      }

      public static bool operator !=(SubscriptAssignment lhs, SubscriptAssignment rhs) {
        return !(lhs == rhs);
      }

      public bool Equals(SubscriptAssignment other) {
        if (other is null) {
          return false;
        }
        if (ReferenceEquals(this, other)) {
          return true;
        }
        return ContainerId == other.ContainerId && KeyId == other.KeyId && ValueId == other.ValueId;
      }

      public override bool Equals(object obj) {
        return Equals(obj as SubscriptAssignment);
      }

      public override int GetHashCode() {
        return new { ContainerId, KeyId, ValueId, }.GetHashCode();
      }

      public override string ToString() {
        return $"Notification.{GetType().Name}: {ContainerId} {KeyId} {ValueId}";
      }

      internal override void Accept(VM vm) {
        vm.HandleSubscriptAssignment(this);
      }
    }

    internal sealed class TempRegisterAllocated :
        AbstractNotification, IEquatable<TempRegisterAllocated> {
      public uint Id { get; }

      internal TempRegisterAllocated(uint id) {
        Id = id;
      }

      public static bool operator ==(TempRegisterAllocated lhs, TempRegisterAllocated rhs) {
        return lhs.Equals(rhs);
      }

      public static bool operator !=(TempRegisterAllocated lhs, TempRegisterAllocated rhs) {
        return !(lhs == rhs);
      }

      public bool Equals(TempRegisterAllocated other) {
        if (other is null) {
          return false;
        }
        if (ReferenceEquals(this, other)) {
          return true;
        }
        return Id == other.Id;
      }

      public override bool Equals(object obj) {
        return Equals(obj as TempRegisterAllocated);
      }

      public override int GetHashCode() {
        return Id.GetHashCode();
      }

      public override string ToString() {
        return $"Notification.{GetType().Name}: {Id}";
      }

      internal override void Accept(VM vm) {
        vm.HandleTempRegisterAllocated(this);
      }
    }

    internal sealed class VariableDefined : AbstractNotification, IEquatable<VariableDefined> {
      public VariableInfo Info { get; }

      internal VariableDefined(VariableInfo info) {
        Info = info;
      }

      public static bool operator ==(VariableDefined lhs, VariableDefined rhs) {
        return lhs.Equals(rhs);
      }

      public static bool operator !=(VariableDefined lhs, VariableDefined rhs) {
        return !(lhs == rhs);
      }

      public bool Equals(VariableDefined other) {
        if (other is null) {
          return false;
        }
        if (ReferenceEquals(this, other)) {
          return true;
        }
        return Info == other.Info;
      }

      public override bool Equals(object obj) {
        return Equals(obj as VariableDefined);
      }

      public override int GetHashCode() {
        return Info.GetHashCode();
      }

      public override string ToString() {
        return $"Notification.{GetType().Name}: {Info}";
      }

      internal override void Accept(VM vm) {
        vm.HandleVariableDefined(this);
      }
    }

    internal sealed class VariableDeleted : AbstractNotification, IEquatable<VariableDeleted> {
      public uint StartId { get; }

      internal VariableDeleted(uint startId) {
        StartId = startId;
      }

      public static bool operator ==(VariableDeleted lhs, VariableDeleted rhs) {
        return lhs.Equals(rhs);
      }

      public static bool operator !=(VariableDeleted lhs, VariableDeleted rhs) {
        return !(lhs == rhs);
      }

      public bool Equals(VariableDeleted other) {
        if (other is null) {
          return false;
        }
        if (ReferenceEquals(this, other)) {
          return true;
        }
        return StartId == other.StartId;
      }

      public override bool Equals(object obj) {
        return Equals(obj as VariableDeleted);
      }

      public override int GetHashCode() {
        return StartId.GetHashCode();
      }

      public override string ToString() {
        return $"Notification.{GetType().Name}: {StartId}";
      }

      internal override void Accept(VM vm) {
        vm.HandleVariableDeleted(this);
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

      public static bool operator ==(VTagInfo lhs, VTagInfo rhs) {
        return lhs.Equals(rhs);
      }

      public static bool operator !=(VTagInfo lhs, VTagInfo rhs) {
        return !(lhs == rhs);
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

      public VTagInfo[] VTagInfos { get; }

      internal VTag(VTagInfo[] vTagInfos) {
        VTagInfos = vTagInfos;
      }

      public static bool operator ==(VTag lhs, VTag rhs) {
        return lhs.Equals(rhs);
      }

      public static bool operator !=(VTag lhs, VTag rhs) {
        return !(lhs == rhs);
      }

      public bool Equals(VTag other) {
        if (other is null) {
          return false;
        }
        if (ReferenceEquals(this, other)) {
          return true;
        }
        return Enumerable.SequenceEqual(VTagInfos, other.VTagInfos);
      }

      public override bool Equals(object obj) {
        return Equals(obj as VTag);
      }

      public override int GetHashCode() {
        var hashCode = new HashCode();
        foreach (VTagInfo info in VTagInfos) {
          hashCode.Add(info);
        }
        return hashCode.ToHashCode();
      }

      public override string ToString() {
        return $"Notification.{GetType().Name}: {string.Join<VTagInfo>(", ", VTagInfos)}";
      }

      internal override void Accept(VM vm) {
        vm.HandleVTag(this);
      }
    }
  }
}
