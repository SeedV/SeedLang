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

namespace SeedLang.Ast {
  // An immutable string value class.
  internal class StringValue : BaseValue {
    private readonly string _value;

    public StringValue(string value) {
      _value = value;
    }

    public override double ToNumber() {
      // TODO: implicit cast from string to number is not allowed (like Python), throw an exception
      // here.
      throw new NotImplementedException();
    }

    public override string ToString() {
      return _value;
    }
  }
}
