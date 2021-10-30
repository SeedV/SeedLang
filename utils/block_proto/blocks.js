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
import * as utils from './block_utils.js';

/**
 * The settings for all blocks.
 * @const {!Object}
 */
const GLOBAL_DEFS = {
  bgColors: {
    flowControlStatement: '#06c',
    expression: '#cc6',
    number: '#690',
    operator: '#9cf',
    parentheses: '#ff9',
    statement: '#9cf',
    string: '#963',
    variable: '#39f',
  },
  colors: {
    dark: '#000',
    delimiter: '#fc0',
    light: '#fff',
  },
  connectorCornerSize: 5,
  connectorHeight: 4,
  connectorWidth: 25,
  font: {
    family: 'Courier',
    size: '14px',
    anchor: 'middle',
    leading: '1em',
  },
  fontBaselineHeight: 2,
  fontCharHeight: 14,
  fontCharWidth: 9,
  margin: 30,
  padding: 6,
  rectRadius: 6,
  statementLeftPadding: 20,
};

/**
 * The block definitions.
 * @const {!Object}
 */
export const BLOCK_DEFS = {
  break: {
    fields: [
      {label: 'break', withInput: false},
    ],
    renderer: renderStatement,
  },
  continue: {
    fields: [
      {label: 'continue', withInput: false},
    ],
    renderer: renderStatement,
  },
  expression: {
    background: GLOBAL_DEFS.bgColors.expression,
    defaultConfig: 'a,+,b',
    renderer: renderExpressionContainer,
  },
  eval: {
    defaultConfig: 'x,+,3.14',
    fields: [
      {label: 'eval', withInput: true},
    ],
    renderer: renderStatement,
  },
  forIn: {
    defaultConfig: '{x|items}{set:y|y,+,x}',
    fields: [
      {label: 'for', withInput: true},
      {label: 'in', withInput: true},
    ],
    renderer: renderFlowControlStatement,
  },
  if: {
    defaultConfig: '{x,>,3}{set:counter|counter,+,1_set:x|x,-,1}',
    fields: [
      {label: 'if', withInput: true},
    ],
    renderer: renderFlowControlStatement,
  },
  ifElse: {
    defaultConfig: '{x,>,3}{set:counter|counter,+,1}{set:counter|counter,-,1}',
    fields: [
      {label: 'if', withInput: true},
    ],
    secondaryFields: [
      {label: 'else', withInput: false},
    ],
    renderer: renderFlowControlStatement,
  },
  number: {
    background: GLOBAL_DEFS.bgColors.number,
    color: GLOBAL_DEFS.colors.light,
    delimiter: null,
    defaultConfig: '3.14',
    renderer: renderRoundRectValue,
  },
  operator: {
    background: GLOBAL_DEFS.bgColors.operator,
    color: GLOBAL_DEFS.colors.dark,
    defaultConfig: '+',
    renderer: renderOctagonToken,
    validValues: [
      '+', '-', '*', '×', '/', '÷', '^',
      '==', '!=', '>', '>=', '<', '<=',
      'and', 'or', 'not',
    ],
  },
  parentheses: {
    background: GLOBAL_DEFS.bgColors.parentheses,
    color: GLOBAL_DEFS.colors.dark,
    defaultConfig: '(',
    renderer: renderOctagonToken,
    validValues: [
      '(', ')',
    ],
  },
  repeat: {
    defaultConfig: '{10}{set:x|x,+,3}',
    fields: [
      {label: 'repeat', withInput: true},
      {label: 'times', withInput: false},
    ],
    renderer: renderFlowControlStatement,
  },
  return: {
    defaultConfig: '3.14',
    fields: [
      {label: 'return', withInput: true},
    ],
    renderer: renderStatement,
  },
  set: {
    defaultConfig: 'counter|x,+,3.14',
    fields: [
      {label: 'set', withInput: true},
      {label: 'to', withInput: true},
    ],
    renderer: renderStatement,
  },
  string: {
    background: GLOBAL_DEFS.bgColors.string,
    color: GLOBAL_DEFS.colors.light,
    delimiter: '"',
    delimiterColor: GLOBAL_DEFS.colors.delimiter,
    defaultConfig: 'Hello',
    renderer: renderRoundRectValue,
  },
  variable: {
    background: GLOBAL_DEFS.bgColors.variable,
    color: GLOBAL_DEFS.colors.light,
    defaultConfig: 'counter',
    renderer: renderOctagonToken,
  },
  while: {
    defaultConfig: '{x,>,3}{set:counter|counter,+,1_set:x|x,-,1}',
    fields: [
      {label: 'while', withInput: true},
    ],
    renderer: renderFlowControlStatement,
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

  if (!(block in BLOCK_DEFS)) {
    throw new Error('Invalid block name.');
  }
  const blockDef = BLOCK_DEFS[block];
  const configString = config || blockDef.defaultConfig;
  if (blockDef.validValues && !blockDef.validValues.includes(configString)) {
    throw new Error('Invalid config string: ' + configString);
  }
  const blockShape =
      blockDef.renderer(draw, BLOCK_DEFS[block], configString, null);
  draw.size(blockShape.width + 2 * GLOBAL_DEFS.margin,
      blockShape.height + 2 * GLOBAL_DEFS.margin);
  return draw.svg();
}

/**
 * A structure to hold the info of a rendered SVG shape (or a shape group).
 */
class RenderedShape {
  /**
   * @param {!Object|!Array<RenderedShape>} shape The rendered svgjs element or
   *     an array of child RenderedShape objects.
   * @param {number} width The calculated width of the rendered shape.
   * @param {number} height The calculated height of the rendered shape.
   */
  constructor(shape, width, height) {
    this.shape = shape;
    this.width = width;
    this.height = height;
  }
};

/**
 * Renders a center-aligned text at the center of a given rectangle area.
 * @param {!Object} draw The svgjs draw object.
 * @param {string} text The text to be drawn.
 * @param {number} shapeWidth The width of the given rectangle area.
 * @param {number} shapeHeight The height of the given rectangle area.
 * @param {string} color The text color.
 * @param {?svgjs.Point} offset The offset of the block.
 * @return {!RenderedShape} The rendered SVG element.
 */
function renderCenterText(draw, text, shapeWidth, shapeHeight, color, offset) {
  const textAnchorX = shapeWidth / 2;
  const textAnchorY = shapeHeight / 2 + GLOBAL_DEFS.fontCharHeight / 2 -
      GLOBAL_DEFS.fontBaselineHeight;
  const shape = draw.text(text)
      .font(GLOBAL_DEFS.font)
      .fill(color)
      .move(offset.x + textAnchorX, offset.y + textAnchorY);
  return new RenderedShape(shape, shapeWidth, shapeHeight);
}

/**
 * Centers a container's children shapes vertically.
 * @param {!Array<RenderedShape>} childrenShapes The rendered children shapes.
 * @param {number} shapeHeight The height of the current container shape.
 * @param {boolean} hasConnectors If the current container shape has top/bottom
 *     connectors.
 */
function centerChildrenVertically(childrenShapes, shapeHeight, hasConnectors) {
  for (const childShape of childrenShapes) {
    const childrenAreaHeight =
        shapeHeight - (hasConnectors ? GLOBAL_DEFS.connectorHeight : 0) -
            2 * GLOBAL_DEFS.padding;
    const delta = (childrenAreaHeight - childShape.height) / 2;
    if (delta > 0) {
      childShape.shape.dy(delta);
    }
  }
}

/**
 * Renders a number or string value in a round rectangle shape.
 * @param {!Object} draw The svgjs draw object.
 * @param {!Object} blockDef The definition of the block kind.
 * @param {?string} config The config string.
 * @param {?svgjs.Point} offset The offset of the block. If it is null, the
 *     block is the main block and will be positioned to the center of the SVG
 *     canvas.
 * @return {!RenderedShape} The rendered shape info.
 */
function renderRoundRectValue(draw, blockDef, config, offset) {
  let textLength = config.length;
  if (blockDef.delimiter) {
    textLength += 2;
  }
  const shapeWidth = 2 * GLOBAL_DEFS.padding +
      textLength * GLOBAL_DEFS.fontCharWidth;
  const shapeHeight = 2 * GLOBAL_DEFS.padding + GLOBAL_DEFS.fontCharHeight;
  const shapeOffset = offset ||
      new svgjs.Point(GLOBAL_DEFS.margin, GLOBAL_DEFS.margin);

  const group = draw.group();
  group.rect(shapeWidth, shapeHeight)
      .fill(blockDef.background)
      .radius(GLOBAL_DEFS.rectRadius)
      .move(shapeOffset.x, shapeOffset.y);

  renderCenterText(group, config, shapeWidth, shapeHeight, blockDef.color,
      shapeOffset);

  if (blockDef.delimiter) {
    // Iterates for the left delimiter and the right delimiter.
    for (const textOffsetX of [
      GLOBAL_DEFS.padding,
      shapeWidth - GLOBAL_DEFS.padding - GLOBAL_DEFS.fontCharWidth,
    ]) {
      const delimiterWidth =
          blockDef.delimiter.length * GLOBAL_DEFS.fontCharWidth;
      renderCenterText(group, blockDef.delimiter,
          delimiterWidth, shapeHeight, blockDef.delimiterColor,
          new svgjs.Point(shapeOffset.x + textOffsetX, shapeOffset.y));
    }
  }

  return new RenderedShape(group, shapeWidth, shapeHeight);
}

/**
 * Renders a variable or operator token in an octagon block.
 * @param {!Object} draw The svgjs draw object.
 * @param {!Object} blockDef The definition of the block kind.
 * @param {?string} config The config string.
 * @param {?svgjs.Point} offset The offset of the block. If it is null, the
 *     block is the main block and will be positioned to the center of the SVG
 *     canvas.
 * @return {!RenderedShape} The rendered shape info.
 */
function renderOctagonToken(draw, blockDef, config, offset) {
  const textLength = config.length;
  const shapeWidth = 2 * GLOBAL_DEFS.padding +
    textLength * GLOBAL_DEFS.fontCharWidth;
  const shapeHeight = 2 * GLOBAL_DEFS.padding + GLOBAL_DEFS.fontCharHeight;
  const shapeOffset = offset ||
      new svgjs.Point(GLOBAL_DEFS.margin, GLOBAL_DEFS.margin);

  const group = draw.group();
  {
    const ox = shapeOffset.x;
    const oy = shapeOffset.y;
    const rr = GLOBAL_DEFS.rectRadius;
    const w = shapeWidth;
    const h = shapeHeight;
    group.polygon()
        .plot([
          [ox, oy + rr], [ox + rr, oy],
          [ox + w - rr, oy], [ox + w, oy + rr],
          [ox + w, oy + h - rr], [ox + w - rr, oy + h],
          [ox + rr, oy + h], [ox, oy + h - rr],
        ])
        .fill(blockDef.background)
        .move(ox, oy);
  }

  renderCenterText(group, config, shapeWidth, shapeHeight, blockDef.color,
      shapeOffset);

  return new RenderedShape(group, shapeWidth, shapeHeight);
}

/**
 * Renders an expression in a round rectangle block.
 * @param {!Object} draw The svgjs draw object.
 * @param {!Object} blockDef The definition of the block kind.
 * @param {?string} config The config string.
 * @param {?svgjs.Point} offset The offset of the block. If it is null, the
 *     block is the main block and will be positioned to the center of the SVG
 *     canvas.
 * @return {!RenderedShape} The rendered shape info.
 */
function renderExpressionContainer(draw, blockDef, config, offset) {
  const children = utils.splitListItems(config, BLOCK_DEFS);
  const shapeOffset = offset ||
      new svgjs.Point(GLOBAL_DEFS.margin, GLOBAL_DEFS.margin);

  /** @type {Array<RenderedShape>} */
  const childrenShapes = [];
  let childOffsetX = shapeOffset.x + GLOBAL_DEFS.padding;
  const childOffsetY = shapeOffset.y + GLOBAL_DEFS.padding;
  let shapeWidth = GLOBAL_DEFS.padding;
  let maxChildHeight = 0;
  const group = draw.group();
  for (const child of children) {
    const childShape =
        child.blockDef.renderer(group, child.blockDef, child.config,
            new svgjs.Point(childOffsetX, childOffsetY));
    childrenShapes.push(childShape);
    childOffsetX += childShape.width + GLOBAL_DEFS.padding;
    shapeWidth += childShape.width + GLOBAL_DEFS.padding;
    if (childShape.height > maxChildHeight) {
      maxChildHeight = childShape.height;
    }
  }
  const shapeHeight = 2 * GLOBAL_DEFS.padding + maxChildHeight;
  centerChildrenVertically(childrenShapes, shapeHeight, true);

  group.rect(shapeWidth, shapeHeight)
      .fill(blockDef.background)
      .radius(GLOBAL_DEFS.rectRadius)
      .move(shapeOffset.x, shapeOffset.y)
      .back();

  return new RenderedShape(group, shapeWidth, shapeHeight);
}

/**
 * Renders the labels and the input items of a statement.
 * @param {!Object} draw The svgjs draw object.
 * @param {!string} labelColor The color of the main block.
 * @param {!Object} fields The definition of the labels and the input items.
 * @param {?string} configString The config string.
 * @param {?svgjs.Point} offset The offset to the header rectangle where the
 *     labels and the input items locate.
 * @return {!RenderedShape} The rendered shape info.
 */
function renderLabelsAndInputs(draw, labelColor, fields, configString, offset) {
  const inputItems = configString ? utils.splitInputItems(configString) : [];
  let inputItemIndex = 0;
  const color = labelColor || GLOBAL_DEFS.colors.dark;
  /** @type {Array<RenderedShape>} */
  const childrenShapes = [];
  let childOffsetX = offset.x + GLOBAL_DEFS.statementLeftPadding;
  const childOffsetY = offset.y + GLOBAL_DEFS.padding;
  let shapeWidth = GLOBAL_DEFS.statementLeftPadding;
  let maxChildHeight = GLOBAL_DEFS.fontCharHeight;

  for (const field of fields) {
    const labelWidth = field.label.length * GLOBAL_DEFS.fontCharWidth;
    const childShape =
        renderCenterText(draw, field.label,
            labelWidth, GLOBAL_DEFS.fontCharHeight, color,
            new svgjs.Point(childOffsetX, childOffsetY));
    childrenShapes.push(childShape);
    childOffsetX += labelWidth + GLOBAL_DEFS.padding;
    shapeWidth += labelWidth + GLOBAL_DEFS.padding;
    if (field.withInput) {
      if (inputItemIndex >= inputItems.length) {
        throw new Error('Need more input items in: ' + configString);
      }
      const inputItem = inputItems[inputItemIndex++];
      const childBlockDef =
          utils.getBlockDefPerConfigString(inputItem, BLOCK_DEFS);
      const childShape =
          childBlockDef.renderer(draw, childBlockDef, inputItem,
              new svgjs.Point(childOffsetX, childOffsetY));
      childrenShapes.push(childShape);
      childOffsetX += childShape.width + GLOBAL_DEFS.padding;
      shapeWidth += childShape.width + GLOBAL_DEFS.padding;
      if (childShape.height > maxChildHeight) {
        maxChildHeight = childShape.height;
      }
    }
  }
  shapeWidth =
      Math.max(shapeWidth,
          2 * GLOBAL_DEFS.statementLeftPadding + GLOBAL_DEFS.padding +
          2 * GLOBAL_DEFS.connectorWidth);
  const shapeHeight = 2 * GLOBAL_DEFS.padding + maxChildHeight;
  centerChildrenVertically(childrenShapes, shapeHeight, true);
  return new RenderedShape(childrenShapes, shapeWidth, shapeHeight);
}

/**
 * Renders a statement in a rectangle block with top and bottom connectors.
 * @param {!Object} draw The svgjs draw object.
 * @param {!Object} blockDef The definition of the block kind.
 * @param {?string} config The config string.
 * @param {?svgjs.Point} offset The offset of the block. If it is null, the
 *     block is the main block and will be positioned to the center of the SVG
 *     canvas.
 * @return {!RenderedShape} The rendered shape info. The returned shape height
 *    includes the height of the top connector.
 */
function renderStatement(draw, blockDef, config, offset) {
  // For a statement block, its anchor point is the top-most point of its
  // left-most edge. Hence, the y position of the anchor point is lower than the
  // highest position of the shape, at the top connector.
  const shapeOffset = offset ||
      new svgjs.Point(GLOBAL_DEFS.margin,
          GLOBAL_DEFS.margin + GLOBAL_DEFS.connectorHeight);
  const background = blockDef.background || GLOBAL_DEFS.bgColors.statement;

  const group = draw.group();
  const shapes =
      renderLabelsAndInputs(group, blockDef.color, blockDef.fields,
          config, shapeOffset);
  {
    const ox = shapeOffset.x;
    const oy = shapeOffset.y;
    const lp = GLOBAL_DEFS.statementLeftPadding;
    const ccs = GLOBAL_DEFS.connectorCornerSize;
    const cw = GLOBAL_DEFS.connectorWidth;
    const ch = GLOBAL_DEFS.connectorHeight;
    const w = shapes.width;
    const h = shapes.height;
    group.polygon()
        .plot([
          // Top-left corner.
          [ox, oy],
          // Top connector.
          [ox + lp, oy], [ox + lp + ccs, oy - ch],
          [ox + lp + cw - ccs, oy - ch], [ox + lp + cw, oy],
          // Top-right corner.
          [ox + w, oy],
          // Bottom-right corner.
          [ox + w, oy + h],
          // Bottom connector.
          [ox + lp + cw, oy + h], [ox + lp + cw - ccs, oy + h - ch],
          [ox + lp + ccs, oy + h - ch], [ox + lp, oy + h],
          // Bottom-left corner.
          [ox, oy + h],
        ])
        .fill(background)
        .back();
  }
  return new RenderedShape(
      group, shapes.width, shapes.height + GLOBAL_DEFS.connectorHeight);
}

/**
 * Renders a group of statements.
 * @param {!Object} draw The svgjs draw object.
 * @param {!Array<Object>} statementGroup An array of statement definitions.
 * @param {?svgjs.Point} offset The offset of the block. If it is null, the
 *     block is the main block and will be positioned to the center of the SVG
 *     canvas.
 * @return {!RenderedShape} The rendered shape info. The returned shape height
 *    includes the height of the top connector and the padding spaces among
 *    statements.
 */
function renderStatementGroup(draw, statementGroup, offset) {
  if (statementGroup == null || statementGroup.length <= 0) {
    throw new Error(
        'A statement group must contain one or more statements.');
  }
  /** @type {Array<RenderedShape>} */
  const statementShapes = [];
  const shapeOffset = offset ||
      new svgjs.Point(GLOBAL_DEFS.margin,
          GLOBAL_DEFS.margin + GLOBAL_DEFS.connectorHeight);
  let currentOffsetY = shapeOffset.y;
  let maxWidth = 0;
  for (const statement of statementGroup) {
    const shape = statement.blockDef.renderer(
        draw, statement.blockDef, statement.blockConfig,
        new svgjs.Point(shapeOffset.x, currentOffsetY));
    statementShapes.push(shape);
    if (shape.width > maxWidth) {
      maxWidth = shape.width;
    }
    const childHeightWithoutTopConnector =
        shape.height - GLOBAL_DEFS.connectorHeight;
    currentOffsetY +=
        childHeightWithoutTopConnector + GLOBAL_DEFS.padding;
  }
  const heightWithTopConnector =
      currentOffsetY - shapeOffset.y -
          GLOBAL_DEFS.padding + GLOBAL_DEFS.connectorHeight;
  return new RenderedShape(statementShapes, maxWidth, heightWithTopConnector);
}

/**
 * Renders a flow control statement.
 * @param {!Object} draw The svgjs draw object.
 * @param {!Object} blockDef The definition of the block kind.
 * @param {?string} config The config string.
 * @param {?svgjs.Point} offset The offset of the block. If it is null, the
 *     block is the main block and will be positioned to the center of the SVG
 *     canvas.
 * @return {!RenderedShape} The rendered shape info.
 */
function renderFlowControlStatement(draw, blockDef, config, offset) {
  const parsedResult =
      utils.splitInputItemsAndStatementGroups(config, BLOCK_DEFS);
  // if, forIn, while, repeat: only one statement group is accepted.
  const statementGroup1 = parsedResult.statementGroups[0];
  // ifElse: two statement groups are accepted.
  const statementGroup2 =
      parsedResult.statementGroups.length > 1 ?
          parsedResult.statementGroups[1] : null;

  const shapeOffset = offset ||
      new svgjs.Point(GLOBAL_DEFS.margin,
          GLOBAL_DEFS.margin + GLOBAL_DEFS.connectorHeight);
  const background =
      blockDef.background || GLOBAL_DEFS.bgColors.flowControlStatement;
  if (!blockDef.color) {
    blockDef.color = GLOBAL_DEFS.colors.light;
  }

  const group = draw.group();
  const header1 =
      renderLabelsAndInputs(group, blockDef.color, blockDef.fields,
          parsedResult.inputConfigString, shapeOffset);

  const bodyOffset1 = new svgjs.Point(
      shapeOffset.x + GLOBAL_DEFS.statementLeftPadding + GLOBAL_DEFS.padding,
      shapeOffset.y + header1.height + GLOBAL_DEFS.padding);
  const body1 = renderStatementGroup(group, statementGroup1, bodyOffset1);

  // For the second statement group of the ifElse statement.
  let header2 = new RenderedShape(null, 0, 0);
  let body2 = new RenderedShape(null, 0, 0);
  if (statementGroup2 != null) {
    const headerOffset2 =
        new svgjs.Point(shapeOffset.x, bodyOffset1.y + body1.height -
            GLOBAL_DEFS.connectorHeight + GLOBAL_DEFS.padding);
    header2 =
        renderLabelsAndInputs(group, blockDef.color, blockDef.secondaryFields,
            null, headerOffset2);
    const bodyOffset2 = new svgjs.Point(
        bodyOffset1.x,
        headerOffset2.y + header2.height + GLOBAL_DEFS.padding);
    body2 = renderStatementGroup(group, statementGroup2, bodyOffset2);
  }

  const footerWidth = 2 * GLOBAL_DEFS.statementLeftPadding +
      GLOBAL_DEFS.padding + 2 * GLOBAL_DEFS.connectorWidth;
  const footerHeight = GLOBAL_DEFS.connectorHeight + GLOBAL_DEFS.padding;

  {
    const ox = shapeOffset.x;
    const oy = shapeOffset.y;
    const p = GLOBAL_DEFS.padding;
    const lp = GLOBAL_DEFS.statementLeftPadding;
    const ccs = GLOBAL_DEFS.connectorCornerSize;
    const cw = GLOBAL_DEFS.connectorWidth;
    const ch = GLOBAL_DEFS.connectorHeight;
    const hw1 = header1.width;
    const hh1 = header1.height;
    const bh1 = body1.height - ch + 2 * p;
    const hw2 = header2.width;
    const hh2 = header2.height;
    const bh2 = (body2.height == 0) ? 0 : body2.height - ch + 2 * p;
    const fw = footerWidth;
    const fh = footerHeight;

    const vertices1 = [
      // Top-left corner of the header.
      [ox, oy],
      // Top connector of the header.
      [ox + lp, oy], [ox + lp + ccs, oy - ch],
      [ox + lp + cw - ccs, oy - ch], [ox + lp + cw, oy],
      // Top-right corner of the header.
      [ox + hw1, oy],
      // Bottom-right corner of the header.
      [ox + hw1, oy + hh1],
      // Bottom connector of the header.
      [ox + 2 * lp + p + cw, oy + hh1],
      [ox + 2 * lp + p + cw - ccs, oy + hh1 - ch],
      [ox + 2 * lp + p + ccs, oy + hh1 - ch],
      [ox + 2 * lp + p, oy + hh1],
      // Inner bottom-left corner of the header.
      [ox + lp, oy + hh1],
    ];
    const vertices2 = (statementGroup2 == null) ? [] : [
      // Inner top-left corner of the secondary header.
      [ox + lp, oy + hh1 + bh1],
      // Top connector of the secondary header.
      [ox + 2 * lp + p, oy + hh1 + bh1],
      [ox + 2 * lp + p + ccs, oy + hh1 + bh1 - ch],
      [ox + 2 * lp + p + cw - ccs, oy + hh1 + bh1 - ch],
      [ox + 2 * lp + p + cw, oy + hh1 + bh1],
      // Top-right corner of the secondary header.
      [ox + hw2, oy + hh1 + bh1],
      // Bottom-right corner of the secondary header.
      [ox + hw2, oy + hh1 + bh1 + hh2],
      // Bottom connector of the secondary header.
      [ox + 2 * lp + p + cw, oy + hh1 + bh1 + hh2],
      [ox + 2 * lp + p + cw - ccs, oy + hh1 + bh1 + hh2 - ch],
      [ox + 2 * lp + p + ccs, oy + hh1 + bh1 + hh2 - ch],
      [ox + 2 * lp + p, oy + hh1 + bh1 + hh2],
      // Inner bottom-left corner of the secondary header.
      [ox + lp, oy + hh1 + bh1 + hh2],
    ];
    const vertices3 = [
      // Inner top-left corner of the footer.
      [ox + lp, oy + hh1 + bh1 + hh2 + bh2],
      // Top connector of the footer.
      [ox + 2 * lp + p, oy + hh1 + bh1 + hh2 + bh2],
      [ox + 2 * lp + p + ccs, oy + hh1 + bh1 + hh2 + bh2 - ch],
      [ox + 2 * lp + p + cw - ccs, oy + hh1 + bh1 + hh2 + bh2 - ch],
      [ox + 2 * lp + p + cw, oy + hh1 + bh1 + hh2 + bh2],
      // Top-right corner of the footer.
      [ox + fw, oy + hh1 + bh1 + hh2 + bh2],
      // Bottom-right corner of the footer.
      [ox + fw, oy + hh1 + bh1 + hh2 + bh2 + fh],
      // Bottom connector of the footer.
      [ox + lp + cw, oy + hh1 + bh1 + hh2 + bh2 + fh],
      [ox + lp + cw - ccs, oy + hh1 + bh1 + hh2 + bh2 + fh - ch],
      [ox + lp + ccs, oy + hh1 + bh1 + hh2 + bh2 + fh - ch],
      [ox + lp, oy + hh1 + bh1 + hh2 + bh2 + fh],
      // Bottom-left corner of the footer.
      [ox, oy + hh1 + bh1 + hh2 + bh2 + fh],
    ];

    group.polygon()
        .plot([].concat(vertices1, vertices2, vertices3))
        .fill(background)
        .back();
  }
  const shapeWidth =
      Math.max(header1.width, header2.width,
          body1.width + GLOBAL_DEFS.statementLeftPadding + GLOBAL_DEFS.padding,
          body2.width + GLOBAL_DEFS.statementLeftPadding + GLOBAL_DEFS.padding,
          footerWidth);
  const shapeHeight = GLOBAL_DEFS.connectorHeight +
      header1.height + header2.height +
      body1.height +
      (2 * GLOBAL_DEFS.padding - GLOBAL_DEFS.connectorHeight) +
      body2.height +
      (body2.height == 0 ? 0 :
          2 * GLOBAL_DEFS.padding - GLOBAL_DEFS.connectorHeight) +
      footerHeight;

  return new RenderedShape(group, shapeWidth, shapeHeight);
}
