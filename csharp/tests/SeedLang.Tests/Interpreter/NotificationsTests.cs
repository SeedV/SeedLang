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
using FluentAssertions;
using SeedLang.Runtime;
using SeedLang.Visualization;
using Xunit;

namespace SeedLang.Interpreter.Tests {
  public class NotificationTests {
    [Fact]
    public void TestNotifications() {
      var assignment = new Notification.Assignment("a", VariableType.Global, 1);
      assignment.ToString().Should().Be($"Notification.Assignment: 'a': Global 1");
      var binary = new Notification.Binary(1, BinaryOperator.Add, 2, 3);
      binary.ToString().Should().Be("Notification.Binary: 1 Add 2 3");
      var function = new Notification.Function("func", 1, 2);
      function.ToString().Should().Be("Notification.Function: func 1 2");
      var singleStep = new Notification.SingleStep();
      singleStep.ToString().Should().Be("Notification.SingleStep");
      var subscriptAssign = new Notification.SubscriptAssignment("a", VariableType.Local, 1, 2);
      subscriptAssign.ToString().Should().Be("Notification.SubscriptAssignment: 'a': Local 1 2");
      var unary = new Notification.Unary(UnaryOperator.Positive, 1, 2);
      unary.ToString().Should().Be("Notification.Unary: Positive 1 2");
      var variable = new Notification.Variable("a", VariableType.Global);
      variable.ToString().Should().Be("Notification.Variable: a Global");
      var vTagInfos = new Notification.VTagInfo[] {
        new Notification.VTagInfo("VTag", new string[] {"a", "b", "c"}, new uint?[] {1, null, 2}),
      };
      var vTag = new Notification.VTag(vTagInfos);
      vTag.ToString().Should().Be("Notification.VTag: VTag(a: 1, b: null, c: 2)");

      var dict = new Dictionary<Notification.AbstractNotification, int>() {
        [assignment] = 1,
        [binary] = 2,
        [function] = 3,
        [singleStep] = 4,
        [subscriptAssign] = 5,
        [unary] = 6,
        [variable] = 7,
        [vTag] = 8,
      };
      dict[assignment].Should().Be(1);
      dict[new Notification.Assignment("a", VariableType.Global, 1)].Should().Be(1);
      dict[binary].Should().Be(2);
      dict[new Notification.Binary(1, BinaryOperator.Add, 2, 3)].Should().Be(2);
      dict[function].Should().Be(3);
      dict[new Notification.Function("func", 1, 2)].Should().Be(3);
      dict[singleStep].Should().Be(4);
      dict[new Notification.SingleStep()].Should().Be(4);
      dict[subscriptAssign].Should().Be(5);
      dict[new Notification.SubscriptAssignment("a", VariableType.Local, 1, 2)].Should().Be(5);
      dict[unary].Should().Be(6);
      dict[new Notification.Unary(UnaryOperator.Positive, 1, 2)].Should().Be(6);
      dict[variable].Should().Be(7);
      dict[new Notification.Variable("a", VariableType.Global)].Should().Be(7);
      dict[vTag].Should().Be(8);
      vTagInfos = new Notification.VTagInfo[] {
        new Notification.VTagInfo("VTag", new string[] {"a", "b", "c"}, new uint?[] {1, null, 2}),
      };
      dict[new Notification.VTag(vTagInfos)].Should().Be(8);
    }
  }
}
