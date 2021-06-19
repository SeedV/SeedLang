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

// @ts-check

// @ts-ignore
import {createSVGWindow} from 'svgdom';
import svgjs from '@svgdotjs/svg.js';

/**
 * The settings for all blocks.
 * @const {!Object}
 */
const GLOBAL_DEFS = {
  font: {
    family: 'Courier',
    size: '14px',
    anchor: 'middle',
    leading: '1em',
  },
  fontBaselineHeight: 2,
  fontCharHeight: 14,
  fontCharWidth: 9,
  margin: 10,
  padding: 5,
  rectRadius: 5,
};

/**
 * The block definitions.
 * @const {!Object}
 */
const BLOCK_DEFS = {
  number: {
    background: '#690',
    color: '#fff',
    delimiter: null,
    delimiterColor: null,
    defaultValue: '3.14',
    renderer: renderRoundRectValue,
  },
  operator: {
    background: '#999',
    color: '#fff',
    defaultValue: '+',
    renderer: renderOctagonToken,
  },
  string: {
    background: '#963',
    color: '#fff',
    delimiter: '"',
    delimiterColor: '#fc0',
    defaultValue: 'Hello',
    renderer: renderRoundRectValue,
  },
  variable: {
    background: '#39f',
    color: '#fff',
    defaultValue: 'counter',
    renderer: renderOctagonToken,
  },
};

/**
 * Returns an array of all block kinds.
 * @return {!Array}
 */
export function getBlockList() {
  return Object.keys(BLOCK_DEFS);
}

/**
 * Renders a main block with or without its children blocks.
 * @param {string} block The kind of the main block.
 * @param {?string} config The config string.
 * @return {string} The rendered SVG string.
 */
export function render(block, config) {
  const window = createSVGWindow();
  svgjs.registerWindow(window, window.document);
  const draw = svgjs.SVG(); // eslint-disable-line

  if (block in BLOCK_DEFS) {
    const blockSize =
        BLOCK_DEFS[block].renderer(draw, BLOCK_DEFS[block], config, null);
    draw.size(blockSize.width + 2 * GLOBAL_DEFS.margin,
        blockSize.height + 2 * GLOBAL_DEFS.margin);
  }

  return draw.svg();
}

/**
 * Renders a center-aligned text on top of a block.
 * @param {!Object} draw The svgjs draw object.
 * @param {string} text The text to be drawn.
 * @param {number} shapeWidth The shape width.
 * @param {number} shapeHeight The shape height.
 * @param {string} color The text color.
 * @param {?svgjs.Point} offset The offset of the block.
 */
function renderCenterText(draw, text, shapeWidth, shapeHeight, color, offset) {
  const textAnchorX = shapeWidth / 2;
  const textAnchorY = shapeHeight - GLOBAL_DEFS.padding -
      GLOBAL_DEFS.fontBaselineHeight;
  draw.text(text)
      .font(GLOBAL_DEFS.font)
      .fill(color)
      .move(offset.x + textAnchorX, offset.y + textAnchorY);
}

/**
 * Renders a number or string value in a round rectangle shape.
 * @param {!Object} draw The svgjs draw object.
 * @param {!Object} blockDef The definition of the block kind.
 * @param {?string} config The config string.
 * @param {?svgjs.Point} offset The offset of the block. If it is null, the
 *     block is the main block and will be positioned to the center of the SVG
 *     canvas.
 * @return {{width: number, height: number}} The calculated size of the block.
 *     Margins are not included.
 */
function renderRoundRectValue(draw, blockDef, config, offset) {
  const valueString = config || blockDef.defaultValue;
  let textLength = valueString.length;
  if (blockDef.delimiter) {
    textLength += 2;
  }

  const shapeWidth = 2 * GLOBAL_DEFS.padding +
      textLength * GLOBAL_DEFS.fontCharWidth;
  const shapeHeight = 2 * GLOBAL_DEFS.padding + GLOBAL_DEFS.fontCharHeight;

  const shapeOffset = offset ||
    new svgjs.Point(GLOBAL_DEFS.margin, GLOBAL_DEFS.margin);

  draw.rect(shapeWidth, shapeHeight)
      .fill(blockDef.background)
      .radius(GLOBAL_DEFS.rectRadius)
      .move(shapeOffset.x, shapeOffset.y);

  renderCenterText(draw, valueString, shapeWidth, shapeHeight, blockDef.color,
      shapeOffset);

  if (blockDef.delimiter) {
    // Iterates for the left delimiter and the right delimiter.
    for (const textAnchorX of [
      GLOBAL_DEFS.padding + GLOBAL_DEFS.fontCharWidth / 2,
      shapeWidth - GLOBAL_DEFS.padding - GLOBAL_DEFS.fontCharWidth / 2,
    ]) {
      const textAnchorY = shapeHeight - GLOBAL_DEFS.padding -
          GLOBAL_DEFS.fontBaselineHeight;
      draw.text(blockDef.delimiter)
          .font(GLOBAL_DEFS.font)
          .fill(blockDef.delimiterColor)
          .move(shapeOffset.x + textAnchorX,
              shapeOffset.y + textAnchorY);
    }
  }

  return {width: shapeWidth, height: shapeHeight};
}

/**
 * Renders a variable or operator token in an octagon block.
 * @param {!Object} draw The svgjs draw object.
 * @param {!Object} blockDef The definition of the block kind.
 * @param {?string} config The config string.
 * @param {?svgjs.Point} offset The offset of the block. If it is null, the
 *     block is the main block and will be positioned to the center of the SVG
 *     canvas.
 * @return {{width: number, height: number}} The calculated size of the block.
 *     Margins are not included.
 */
function renderOctagonToken(draw, blockDef, config, offset) {
  const nameString = config ? config : blockDef.defaultValue;
  const textLength = nameString.length;
  const shapeWidth = 2 * GLOBAL_DEFS.padding +
    textLength * GLOBAL_DEFS.fontCharWidth;
  const shapeHeight = 2 * GLOBAL_DEFS.padding + GLOBAL_DEFS.fontCharHeight;

  const shapeOffset = offset ||
      new svgjs.Point(GLOBAL_DEFS.margin, GLOBAL_DEFS.margin);

  draw.polygon()
      .plot([
        [shapeOffset.x, shapeOffset.y + GLOBAL_DEFS.rectRadius],
        [shapeOffset.x + GLOBAL_DEFS.rectRadius, shapeOffset.y],
        [shapeOffset.x + shapeWidth - GLOBAL_DEFS.rectRadius, shapeOffset.y],
        [shapeOffset.x + shapeWidth, shapeOffset.y + GLOBAL_DEFS.rectRadius],
        [shapeOffset.x + shapeWidth,
          shapeOffset.y + shapeHeight - GLOBAL_DEFS.rectRadius],
        [shapeOffset.x + shapeWidth - GLOBAL_DEFS.rectRadius,
          shapeOffset.y + shapeHeight],
        [shapeOffset.x + GLOBAL_DEFS.rectRadius, shapeOffset.y + shapeHeight],
        [shapeOffset.x, shapeOffset.y + shapeHeight - GLOBAL_DEFS.rectRadius],
      ])
      .fill(blockDef.background)
      .move(shapeOffset.x, shapeOffset.y);

  renderCenterText(draw, nameString, shapeWidth, shapeHeight, blockDef.color,
      shapeOffset);

  return {width: shapeWidth, height: shapeHeight};
}
