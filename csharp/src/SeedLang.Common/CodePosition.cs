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

using System;

namespace SeedLang.Common {
  // The common operations of the code position classes.
  public abstract class CodePostion : IComparable<CodePostion>, IEquatable<CodePostion> {
    public abstract override int GetHashCode();

    public abstract override string ToString();

    // Compares the current instance with another code position. The concrete class may throw a
    // NotSupportedException instance if the two positions are not comparable.
    public abstract int CompareTo(CodePostion pos);

    public virtual bool Equals(CodePostion pos) {
      return CompareTo(pos) == 0;
    }

    public override bool Equals(object obj) {
      return (obj is CodePostion objCodePosition) && Equals(objCodePosition);
    }

    public static bool operator ==(CodePostion pos1, CodePostion pos2) {
      return pos1.Equals(pos2);
    }

    public static bool operator !=(CodePostion pos1, CodePostion pos2) {
      return !(pos1 == pos2);
    }

    public static bool operator <(CodePostion pos1, CodePostion pos2) {
      return pos1.CompareTo(pos2) < 0;
    }

    public static bool operator <=(CodePostion pos1, CodePostion pos2) {
      return pos1.CompareTo(pos2) <= 0;
    }

    public static bool operator >(CodePostion pos1, CodePostion pos2) {
      return pos1.CompareTo(pos2) > 0;
    }

    public static bool operator >=(CodePostion pos1, CodePostion pos2) {
      return pos1.CompareTo(pos2) >= 0;
    }
  }
}
