using System;
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

using System.IO;

namespace SeedLang.Runtime {
  // The class to provide access of some system variables or functions.
  //
  // This class is not designed as a static or singleton class to support multiple interrepter
  // instances running in different threads at the same time. One scenario is that xUnit will run
  // all the test cases (some test cases need redirect stdout and verify the output from it) in
  // different threads simultaneously.
  internal class Sys {
    public TextWriter Stdout { get; set; } = Console.Out;
  }
}
