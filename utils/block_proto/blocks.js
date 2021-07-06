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
  forin: {
    defaultConfig: 'x|items_set:y|y,+,x',
    fields: [
      {label: 'for', withInput: true},
      {label: 'in', withInput: true},
    ],
    renderer: renderFlowControlStatement,
  },
  if: {
    defaultConfig: 'x,>,3_set:counter|counter,+,1_set:x|x,-,1',
    fields: [
      {label: 'if', withInput: true},
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
    defaultConfig: '10_set:x|x,+,3',
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
    defaultConfig: 'x,>,3_set:counter|counter,+,1_set:x|x,-,1',
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

  if (block in BLOCK_DEFS) {
    const blockDef = BLOCK_DEFS[block];
    if (blockDef.validValues && !blockDef.validValues.includes(config)) {
      throw new Error('Invalid config string: ' + config);
    }
    const blockShape = blockDef.renderer(draw, BLOCK_DEFS[block], config, null);
    draw.size(blockShape.width + 2 * GLOBAL_DEFS.margin,
        blockShape.height + 2 * GLOBAL_DEFS.margin);
  }

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
  const valueString = config || blockDef.defaultConfig;
  let textLength = valueString.length;
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

  renderCenterText(group, valueString, shapeWidth, shapeHeight, blockDef.color,
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
  const nameString = config || blockDef.defaultConfig;
  const textLength = nameString.length;
  const shapeWidth = 2 * GLOBAL_DEFS.padding +
    textLength * GLOBAL_DEFS.fontCharWidth;
  const shapeHeight = 2 * GLOBAL_DEFS.padding + GLOBAL_DEFS.fontCharHeight;
  const shapeOffset = offset ||
      new svgjs.Point(GLOBAL_DEFS.margin, GLOBAL_DEFS.margin);

  const group = draw.group();
  group.polygon()
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

  renderCenterText(group, nameString, shapeWidth, shapeHeight, blockDef.color,
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
  const expressionString = config || blockDef.defaultConfig;
  const children = utils.splitListItems(expressionString, BLOCK_DEFS);
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
 * @param {!Object} blockDef The definition of the main block.
 * @param {?string} configString The config string.
 * @param {?svgjs.Point} shapeOffset The offset of the main block.
 * @return {!RenderedShape} The rendered shape info.
 */
function renderLabelAndInputs(draw, blockDef, configString, shapeOffset) {
  const inputItems = configString ? utils.splitInputItems(configString) : [];
  let inputItemIndex = 0;
  const color = blockDef.color || GLOBAL_DEFS.colors.dark;
  /** @type {Array<RenderedShape>} */
  const childrenShapes = [];
  let childOffsetX = shapeOffset.x + GLOBAL_DEFS.statementLeftPadding;
  const childOffsetY = shapeOffset.y + GLOBAL_DEFS.padding;
  let shapeWidth = GLOBAL_DEFS.statementLeftPadding;
  let maxChildHeight = GLOBAL_DEFS.fontCharHeight;

  for (const field of blockDef.fields) {
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
  // The calculated shapeHeight includes the height of the top connector.
  const shapeHeight =
      2 * GLOBAL_DEFS.padding + maxChildHeight + GLOBAL_DEFS.connectorHeight;
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
 * @return {!RenderedShape} The rendered shape info.
 */
function renderStatement(draw, blockDef, config, offset) {
  const configString = config || blockDef.defaultConfig;
  // For a statement block, its anchor point is the top-most point of its
  // left-most edge. Hence, the y position of the anchor point is lower than the
  // highest position of the shape, at the top connector.
  const shapeOffset = offset ||
      new svgjs.Point(GLOBAL_DEFS.margin,
          GLOBAL_DEFS.margin + GLOBAL_DEFS.connectorHeight);
  const background = blockDef.background || GLOBAL_DEFS.bgColors.statement;

  const group = draw.group();
  const labelAndInputShapes =
      renderLabelAndInputs(group, blockDef, configString, shapeOffset);
  const shapeWidth = labelAndInputShapes.width;
  const shapeHeight = labelAndInputShapes.height;

  group.polygon()
      .plot([
        [shapeOffset.x, shapeOffset.y],
        [shapeOffset.x + GLOBAL_DEFS.statementLeftPadding, shapeOffset.y],
        [shapeOffset.x + GLOBAL_DEFS.statementLeftPadding +
            GLOBAL_DEFS.connectorCornerSize,
        shapeOffset.y - GLOBAL_DEFS.connectorHeight],
        [shapeOffset.x + GLOBAL_DEFS.statementLeftPadding +
            GLOBAL_DEFS.connectorWidth - GLOBAL_DEFS.connectorCornerSize,
        shapeOffset.y - GLOBAL_DEFS.connectorHeight],
        [shapeOffset.x + GLOBAL_DEFS.statementLeftPadding +
            GLOBAL_DEFS.connectorWidth,
        shapeOffset.y],
        [shapeOffset.x + shapeWidth, shapeOffset.y],
        [shapeOffset.x + shapeWidth, shapeOffset.y + shapeHeight -
            GLOBAL_DEFS.connectorHeight],
        [shapeOffset.x + GLOBAL_DEFS.statementLeftPadding +
            GLOBAL_DEFS.connectorWidth,
        shapeOffset.y + shapeHeight - GLOBAL_DEFS.connectorHeight],
        [shapeOffset.x + GLOBAL_DEFS.statementLeftPadding +
            GLOBAL_DEFS.connectorWidth - GLOBAL_DEFS.connectorCornerSize,
        shapeOffset.y + shapeHeight - 2 * GLOBAL_DEFS.connectorHeight],
        [shapeOffset.x + GLOBAL_DEFS.statementLeftPadding +
            GLOBAL_DEFS.connectorCornerSize,
        shapeOffset.y + shapeHeight - 2 * GLOBAL_DEFS.connectorHeight],
        [shapeOffset.x + GLOBAL_DEFS.statementLeftPadding,
          shapeOffset.y + shapeHeight - GLOBAL_DEFS.connectorHeight],
        [shapeOffset.x,
          shapeOffset.y + shapeHeight - GLOBAL_DEFS.connectorHeight],
      ])
      .fill(background)
      .back();

  return new RenderedShape(group, shapeWidth, shapeHeight);
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
  const configString = config || blockDef.defaultConfig;
  const parsedResult =
      utils.splitInputItemsAndCompoundStatements(configString, BLOCK_DEFS);
  const inputConfigString = parsedResult.inputConfigString;
  const childStatements = parsedResult.statements;
  // For a statement block, its anchor point is the top-most point of its
  // left-most edge. Hence, the y position of the anchor point is lower than the
  // highest position of the shape, at the top connector.
  const shapeOffset = offset ||
      new svgjs.Point(GLOBAL_DEFS.margin,
          GLOBAL_DEFS.margin + GLOBAL_DEFS.connectorHeight);
  const background =
      blockDef.background || GLOBAL_DEFS.bgColors.flowControlStatement;
  if (!blockDef.color) {
    blockDef.color = GLOBAL_DEFS.colors.light;
  }

  const group = draw.group();
  const labelAndInputShapes =
      renderLabelAndInputs(group, blockDef, inputConfigString, shapeOffset);
  const shapeHeadWidth = labelAndInputShapes.width;
  const shapeHeadHeight = labelAndInputShapes.height;
  const childStatementOffsetX =
      shapeOffset.x + GLOBAL_DEFS.statementLeftPadding + GLOBAL_DEFS.padding;
  // shapeHeadHeight includes the top connector so a connectorHeight need
  // to be excluded when calculating the y offset.
  let childStatementOffsetY =
      shapeOffset.y + shapeHeadHeight - GLOBAL_DEFS.connectorHeight +
      GLOBAL_DEFS.padding;
  let maxChildStatementWidth = 0;
  let shapeBodyHeight = GLOBAL_DEFS.padding;
  for (const childStatement of childStatements) {
    const childStatementShape =
        childStatement.blockDef.renderer(
            group, childStatement.blockDef, childStatement.blockConfig,
            new svgjs.Point(childStatementOffsetX, childStatementOffsetY));
    if (childStatementShape.width > maxChildStatementWidth) {
      maxChildStatementWidth = childStatementShape.width;
    }
    const childHeightWithoutTopConnector = childStatementShape.height -
        GLOBAL_DEFS.connectorHeight;
    childStatementOffsetY +=
    childHeightWithoutTopConnector + GLOBAL_DEFS.padding;
    shapeBodyHeight += childHeightWithoutTopConnector + GLOBAL_DEFS.padding;
  }
  const shapeBodyWidth =
      GLOBAL_DEFS.statementLeftPadding + GLOBAL_DEFS.padding +
      maxChildStatementWidth;
  const shapeFootWidth = 2 * GLOBAL_DEFS.statementLeftPadding +
      GLOBAL_DEFS.padding + GLOBAL_DEFS.connectorWidth +
      GLOBAL_DEFS.statementLeftPadding;
  const shapeFootHeight = GLOBAL_DEFS.connectorHeight + GLOBAL_DEFS.padding;

  group.polygon()
      .plot([
        [shapeOffset.x, shapeOffset.y],
        [shapeOffset.x + GLOBAL_DEFS.statementLeftPadding, shapeOffset.y],
        [shapeOffset.x + GLOBAL_DEFS.statementLeftPadding +
            GLOBAL_DEFS.connectorCornerSize,
        shapeOffset.y - GLOBAL_DEFS.connectorHeight],
        [shapeOffset.x + GLOBAL_DEFS.statementLeftPadding +
            GLOBAL_DEFS.connectorWidth - GLOBAL_DEFS.connectorCornerSize,
        shapeOffset.y - GLOBAL_DEFS.connectorHeight],
        [shapeOffset.x + GLOBAL_DEFS.statementLeftPadding +
            GLOBAL_DEFS.connectorWidth,
        shapeOffset.y],
        [shapeOffset.x + shapeHeadWidth, shapeOffset.y],
        [shapeOffset.x + shapeHeadWidth,
          shapeOffset.y + shapeHeadHeight - GLOBAL_DEFS.connectorHeight],
        [shapeOffset.x + 2 * GLOBAL_DEFS.statementLeftPadding +
            GLOBAL_DEFS.padding + GLOBAL_DEFS.connectorWidth,
        shapeOffset.y + shapeHeadHeight - GLOBAL_DEFS.connectorHeight],
        [shapeOffset.x + 2 * GLOBAL_DEFS.statementLeftPadding +
            GLOBAL_DEFS.padding + GLOBAL_DEFS.connectorWidth -
            GLOBAL_DEFS.connectorCornerSize,
        shapeOffset.y + shapeHeadHeight - 2 * GLOBAL_DEFS.connectorHeight],
        [shapeOffset.x + 2 * GLOBAL_DEFS.statementLeftPadding +
            GLOBAL_DEFS.padding + GLOBAL_DEFS.connectorCornerSize,
        shapeOffset.y + shapeHeadHeight - 2 * GLOBAL_DEFS.connectorHeight],
        [shapeOffset.x + 2 * GLOBAL_DEFS.statementLeftPadding +
            GLOBAL_DEFS.padding,
        shapeOffset.y + shapeHeadHeight - GLOBAL_DEFS.connectorHeight],
        [shapeOffset.x + GLOBAL_DEFS.statementLeftPadding,
          shapeOffset.y + shapeHeadHeight - GLOBAL_DEFS.connectorHeight],
        [shapeOffset.x + GLOBAL_DEFS.statementLeftPadding,
          shapeOffset.y + shapeHeadHeight - GLOBAL_DEFS.connectorHeight +
          shapeBodyHeight],
        [shapeOffset.x + 2 * GLOBAL_DEFS.statementLeftPadding +
          GLOBAL_DEFS.padding,
        shapeOffset.y + shapeHeadHeight - GLOBAL_DEFS.connectorHeight +
          shapeBodyHeight],
        [shapeOffset.x + 2 * GLOBAL_DEFS.statementLeftPadding +
          GLOBAL_DEFS.padding + GLOBAL_DEFS.connectorCornerSize,
        shapeOffset.y + shapeHeadHeight - GLOBAL_DEFS.connectorHeight +
          shapeBodyHeight - GLOBAL_DEFS.connectorHeight],
        [shapeOffset.x + 2 * GLOBAL_DEFS.statementLeftPadding +
          GLOBAL_DEFS.padding + GLOBAL_DEFS.connectorWidth -
          GLOBAL_DEFS.connectorCornerSize,
        shapeOffset.y + shapeHeadHeight - GLOBAL_DEFS.connectorHeight +
          shapeBodyHeight - GLOBAL_DEFS.connectorHeight],
        [shapeOffset.x + 2 * GLOBAL_DEFS.statementLeftPadding +
          GLOBAL_DEFS.padding + GLOBAL_DEFS.connectorWidth,
        shapeOffset.y + shapeHeadHeight - GLOBAL_DEFS.connectorHeight +
          shapeBodyHeight],
        [shapeOffset.x + shapeFootWidth,
          shapeOffset.y + shapeHeadHeight - GLOBAL_DEFS.connectorHeight +
              shapeBodyHeight],
        [shapeOffset.x + shapeFootWidth,
          shapeOffset.y + shapeHeadHeight - GLOBAL_DEFS.connectorHeight +
              shapeBodyHeight + shapeFootHeight],
        [shapeOffset.x + GLOBAL_DEFS.statementLeftPadding +
            GLOBAL_DEFS.connectorWidth,
        shapeOffset.y + shapeHeadHeight - GLOBAL_DEFS.connectorHeight +
            shapeBodyHeight + shapeFootHeight],
        [shapeOffset.x + GLOBAL_DEFS.statementLeftPadding +
            GLOBAL_DEFS.connectorWidth - GLOBAL_DEFS.connectorCornerSize,
        shapeOffset.y + shapeHeadHeight - 2 * GLOBAL_DEFS.connectorHeight +
            shapeBodyHeight + shapeFootHeight],
        [shapeOffset.x + GLOBAL_DEFS.statementLeftPadding +
            + GLOBAL_DEFS.connectorCornerSize,
        shapeOffset.y + shapeHeadHeight - 2 * GLOBAL_DEFS.connectorHeight +
            shapeBodyHeight + shapeFootHeight],
        [shapeOffset.x + GLOBAL_DEFS.statementLeftPadding,
          shapeOffset.y + shapeHeadHeight - GLOBAL_DEFS.connectorHeight +
              shapeBodyHeight + shapeFootHeight],
        [shapeOffset.x,
          shapeOffset.y + shapeHeadHeight - GLOBAL_DEFS.connectorHeight +
              shapeBodyHeight + shapeFootHeight],
      ])
      .fill(background)
      .back();

  const shapeWidth = Math.max(shapeHeadWidth, shapeBodyWidth, shapeFootWidth);
  const shapeHeight = shapeHeadHeight + shapeBodyHeight + shapeFootHeight;
  return new RenderedShape(group, shapeWidth, shapeHeight);
}
