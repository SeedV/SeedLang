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

using SeedLang.Common;

namespace SeedLang.Runtime {
  // An event which is triggered when a binary expression is evaluated.
  public class BinaryEvent {
    public Value Left { get; }
    public BinaryOperator Op { get; }
    public Value Right { get; }
    public Value Result { get; }
    // The source code range of the binary expression
    public Range Range { get; }

    public BinaryEvent(Value left, BinaryOperator op, Value right, Value result, Range range) {
      Left = left;
      Op = op;
      Right = right;
      Result = result;
      Range = range;
    }
  }

  // An event which is triggered when an assignment statement is executed.
  public class AssignmentEvent {
    public string Identifier { get; }
    public Value Value { get; }
    // The source code range of the assignment statement
    public Range Range { get; }

    public AssignmentEvent(string identifier, Value value, Range range) {
      Identifier = identifier;
      Value = value;
      Range = range;
    }
  }

  // An event which is triggered when an eval statement is executed.
  public class EvalEvent {
    public Value Value { get; }
    // The source code range of the eval statement
    public Range Range { get; }

    public EvalEvent(Value value, Range range) {
      Value = value;
      Range = range;
    }
  }
}
