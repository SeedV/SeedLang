#!/bin/bash
# Copyright 2021-2022 The SeedV Lab.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

markdown_name="$1/Benchmark.md"
results_path=./BenchmarkDotNet.Artifacts/results

old_md5=$(md5sum "${markdown_name}")

rm -f "${markdown_name}"

{
  cat <<END
# Benchmark

## Fibonacci

END

  cat ${results_path}/SeedLang.Benchmark.FibBenchmark-report-github.md

  cat <<END

## Parser

END

  cat ${results_path}/SeedLang.Benchmark.ParserBenchmark-report-github.md

  cat <<END

## Sum

END

  cat ${results_path}/SeedLang.Benchmark.SumBenchmark-report-github.md
} >>"${markdown_name}"

new_md5=$(md5sum "${markdown_name}")

echo "Old md5 sum: ${old_md5}; New md5 sum: ${new_md5}"

if [ "${old_md5}" != "${new_md5}" ]; then
  cd "$1" || exit
  git config --global user.email "codingpotato@gmail.com"
  git config --global user.name "codingpotato"
  git add Benchmark.md
  git commit -m "Update Benchmark.ms at $(date)"
  git push origin master
  echo "Benchmark.mk updated."
fi
