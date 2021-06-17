/**
 * @license
 * Copyright 2021 The Aha001 Team.
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
 * @fileoverview Utilities to define and render blocks.
 */

import {createSVGWindow} from 'svgdom';
import svgjs from '@svgdotjs/svg.js';

/**
 * @const {Object}
 */
const GLOBAL_DEFS = {
  font: {
    family: 'monospace',
    size: 24,
    anchor: 'start',
    leading: 24,
  },
  monospaceTextWidth: 15,
  margin: 10,
  padding: 10,
  rectRadius: 10,
  unitHeight: 32,
};

/**
 * @const {Object}
 */
const BLOCK_DEFS = {
  number: {
    background: '#690',
    color: '#fff',
    renderer: renderNumber,
  },
};

/**
 * Returns an array of all block kinds.
 * @return {Array}
 */
export function getBlockKinds() {
  return Object.keys(BLOCK_DEFS);
}

/**
 * Renders a main block with or without its children blocks.
 * @param {string} block The kind of the main block.
 * @param {string?} config The config string.
 * @return {string} The rendered SVG string.
 */
export function render(block, config) {
  const window = createSVGWindow();
  svgjs.registerWindow(window, window.document);
  const draw = svgjs.SVG(); // eslint-disable-line

  if (block in BLOCK_DEFS) {
    BLOCK_DEFS[block].renderer(draw, BLOCK_DEFS[block], config);
  }

  return draw.svg();
}

/**
 * Renders a number value block.
 * @param {Object} draw The svgjs draw object.
 * @param {Object} defs The definition of the main block kind.
 * @param {string?} config The config string.
 */
function renderNumber(draw, defs, config) {
  const value = '3.14';
  const textLength = value.length;
  const rectWidth = 2 * GLOBAL_DEFS.padding +
    textLength * GLOBAL_DEFS.monospaceTextWidth;
  const rectHeight = 2 * GLOBAL_DEFS.padding + GLOBAL_DEFS.unitHeight;

  draw.rect(rectWidth, rectHeight)
      .fill(defs.background)
      .radius(GLOBAL_DEFS.rectRadius)
      .move(GLOBAL_DEFS.margin, GLOBAL_DEFS.margin);
  draw.text(value)
      .font(GLOBAL_DEFS.font)
      .fill(defs.color)
      .move(GLOBAL_DEFS.margin + GLOBAL_DEFS.padding,
          GLOBAL_DEFS.margin + GLOBAL_DEFS.padding);
}
