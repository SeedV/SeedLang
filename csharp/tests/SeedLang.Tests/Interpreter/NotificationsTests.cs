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
      (assignment == new Notification.Assignment("a", VariableType.Global, 1)).Should().Be(true);
      (assignment != null).Should().Be(true);
      assignment.ToString().Should().Be($"Notification.Assignment: 'a': Global 1");

      var binary = new Notification.Binary(1, BinaryOperator.Add, 2, 3);
      (binary == new Notification.Binary(1, BinaryOperator.Add, 2, 3)).Should().Be(true);
      (binary != null).Should().Be(true);
      binary.ToString().Should().Be("Notification.Binary: 1 Add 2 3");

      var elementLoaded = new Notification.ElementLoaded(1, 2, 3);
      (elementLoaded == new Notification.ElementLoaded(1, 2, 3)).Should().Be(true);
      (elementLoaded != new Notification.ElementLoaded(2, 2, 3)).Should().Be(true);
      elementLoaded.ToString().Should().Be("Notification.ElementLoaded: 1 2 3");

      var function = new Notification.Function("func", 1, 2);
      (function == new Notification.Function("func", 1, 2)).Should().Be(true);
      (function != null).Should().Be(true);
      function.ToString().Should().Be("Notification.Function: func 1 2");

      var globalLoaded = new Notification.GlobalLoaded(1, "global");
      (globalLoaded == new Notification.GlobalLoaded(1, "global")).Should().Be(true);
      (globalLoaded != new Notification.GlobalLoaded(1, "another global")).Should().Be(true);
      globalLoaded.ToString().Should().Be("Notification.GlobalLoaded: 1 'global'");

      var singleStep = new Notification.SingleStep();
      (singleStep == new Notification.SingleStep()).Should().Be(true);
      (singleStep != null).Should().Be(true);
      singleStep.ToString().Should().Be("Notification.SingleStep");

      var subscriptAssign = new Notification.SubscriptAssignment(0, 1, 2);
      (subscriptAssign == new Notification.SubscriptAssignment(0, 1, 2)).Should().Be(true);
      (subscriptAssign != null).Should().Be(true);
      subscriptAssign.ToString().Should().Be("Notification.SubscriptAssignment: 0 1 2");

      var unary = new Notification.Unary(UnaryOperator.Positive, 1, 2);
      (unary == new Notification.Unary(UnaryOperator.Positive, 1, 2)).Should().Be(true);
      (unary != null).Should().Be(true);
      unary.ToString().Should().Be("Notification.Unary: Positive 1 2");

      var info = new VariableInfo("a", VariableType.Global, 1);
      var variableDefined = new Notification.VariableDefined(info);
      (variableDefined == new Notification.VariableDefined(info)).Should().Be(true);
      (variableDefined != null).Should().Be(true);
      variableDefined.ToString().Should().Be("Notification.VariableDefined: 'a' Global 1");

      var variableDeleted = new Notification.VariableDeleted(1);
      (variableDeleted == new Notification.VariableDeleted(1)).Should().Be(true);
      (variableDeleted != new Notification.VariableDeleted(2)).Should().Be(true);
      variableDeleted.ToString().Should().Be("Notification.VariableDeleted: 1");

      var vTagInfos = new Notification.VTagInfo[] {
        new Notification.VTagInfo("VTag", new string[] {"a", "b", "c"}, new uint?[] {1, null, 2}),
      };
      var vTag = new Notification.VTag(vTagInfos);
      (vTag == new Notification.VTag(vTagInfos)).Should().Be(true);
      (vTag != null).Should().Be(true);
      vTag.ToString().Should().Be("Notification.VTag: VTag(a: 1, b: null, c: 2)");

      var dict = new Dictionary<Notification.AbstractNotification, string>() {
        [assignment] = "assignment",
        [binary] = "binary",
        [elementLoaded] = "elementLoaded",
        [function] = "function",
        [globalLoaded] = "globalLoaded",
        [singleStep] = "singleStep",
        [subscriptAssign] = "subscriptAssign",
        [unary] = "unary",
        [variableDefined] = "variableDefined",
        [variableDeleted] = "variableDeleted",
        [vTag] = "vTag",
      };

      dict[assignment].Should().Be("assignment");
      dict[new Notification.Assignment("a", VariableType.Global, 1)].Should().Be("assignment");

      dict[binary].Should().Be("binary");
      dict[new Notification.Binary(1, BinaryOperator.Add, 2, 3)].Should().Be("binary");

      dict[elementLoaded].Should().Be("elementLoaded");
      dict[new Notification.ElementLoaded(1, 2, 3)].Should().Be("elementLoaded");

      dict[function].Should().Be("function");
      dict[new Notification.Function("func", 1, 2)].Should().Be("function");

      dict[globalLoaded].Should().Be("globalLoaded");
      dict[new Notification.GlobalLoaded(1, "global")].Should().Be("globalLoaded");

      dict[singleStep].Should().Be("singleStep");
      dict[new Notification.SingleStep()].Should().Be("singleStep");

      dict[subscriptAssign].Should().Be("subscriptAssign");
      dict[new Notification.SubscriptAssignment(0, 1, 2)].Should().Be("subscriptAssign");

      dict[unary].Should().Be("unary");
      dict[new Notification.Unary(UnaryOperator.Positive, 1, 2)].Should().Be("unary");

      dict[variableDefined].Should().Be("variableDefined");
      info = new VariableInfo("a", VariableType.Global, 1);
      dict[new Notification.VariableDefined(info)].Should().Be("variableDefined");

      dict[variableDeleted].Should().Be("variableDeleted");
      dict[new Notification.VariableDeleted(1)].Should().Be("variableDeleted");

      dict[vTag].Should().Be("vTag");
      vTagInfos = new Notification.VTagInfo[] {
        new Notification.VTagInfo("VTag", new string[] {"a", "b", "c"}, new uint?[] {1, null, 2}),
      };
      dict[new Notification.VTag(vTagInfos)].Should().Be("vTag");
    }
  }
}
