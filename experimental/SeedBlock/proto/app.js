/**
 * @license
 * Copyright 2021-2022 The SeedV Lab.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

/**
 * @fileoverview The main entry of the block prototype maker.
 */

// @ts-check

import fs from 'fs';
import yargs from 'yargs';
import {hideBin} from 'yargs/helpers';
import {getBlockList, render} from './blocks.js';

/**
 * @type {Object}
 */
const argv = yargs(hideBin(process.argv)).options({
  'block': {
    alias: 'b',
    type: 'string',
    describe: 'The kind of the main block to output.',
    demandOption: true,
    choices: getBlockList(),
  },
  'config': {
    alias: 'c',
    type: 'string',
    describe: 'Optional config string for rendering blocks.',
    demandOption: false,
  },
  'output': {
    alias: 'o',
    type: 'string',
    describe: 'The output SVG file path.',
    demandOption: false,
  },
}).argv;

const svg = render(argv.block, argv.config || null);
if (argv.output) {
  fs.writeFileSync(argv.output, svg);
} else {
  console.log(svg);
}
