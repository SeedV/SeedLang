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
#
# Prerequisites:
# * Graphviz
# * Node.js

# Re-generate design images from Graphviz dot files.
dot -Tsvg -ooverview.svg overview.gv
dot -Tsvg -oseed_block_inter_module_view.svg seed_block_inter_module_view.gv
dot -Tsvg -oseed_block_inter_object_view.svg seed_block_inter_object_view.gv
dot -Tsvg -oseed_block_code_view.svg seed_block_code_view.gv

# Re-generate SeedBlock example images with the block prototype util.
node ../utils/block_proto -b break -o example_block_images/break_block.svg
node ../utils/block_proto -b if -o example_block_images/if_block.svg
node ../utils/block_proto -b ifElse -o example_block_images/ifelse_block.svg
node ../utils/block_proto -b number -o example_block_images/number_block.svg
node ../utils/block_proto -b set -o example_block_images/set_block.svg
node ../utils/block_proto -b string -o example_block_images/string_block.svg
