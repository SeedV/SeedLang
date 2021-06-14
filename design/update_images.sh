#!/bin/bash
# Copyright 2021 The Aha001 Team.
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

# Re-generate design images from their source files - Graphviz dot files, SVG
# files, etc.
#
# Prerequisites:
#
# (1) Install Graphviz per the guide at https://graphviz.org/download/
#
# (2) Install pngquant per the guide at https://pngquant.org/install.html

# Re-generate design images from Graphviz dot files.
dot -Tpng -ooverview.png overview.gv

# Compress the generated png images.
pngquant --force --ext .png *.png
