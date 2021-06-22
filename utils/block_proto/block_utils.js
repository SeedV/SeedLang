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
 * @fileoverview Utilities for rendering blocks.
 */

// @ts-check

/**
 * Splits a comma separated list string into tagged items.
 *
 * The input list string can be:
 *
 *  - An expression, e.g., '(,3.14,+,2.71,),>=,counter'.
 *  - A list literal, e.g., '1,2,3,4,5'.
 *
 * As this is an internal prototype drawing utility, we assume the input list
 * string is correctly built and formatted. Neither syntax parser nor semantic
 * checker is required here.
 * @param {string} listString The comma separated list string.
 * @param {!Object} blockDefs The definition of all blocks.
 * @return {!Array<Object>} An array of tagged items.
 */
export function splitListItems(listString, blockDefs) {
  const itemStrings = listString.split(',');
  const items = [];
  for (const itemString of itemStrings) {
    if (itemString) {
      items.push({
        blockDef: getBlockDefPerConfigString(itemString, blockDefs),
        config: itemString,
      });
    }
  }
  if (items.length <= 0) {
    throw new Error('Invalid list string: ' + listString);
  }
  return items;
}

/**
 * Splits a '|' separated string into input items for filling a statement.
 *
 * As this is an internal prototype drawing utility, we assume the input string
 * is correctly built and formatted. Neither syntax parser nor semantic checker
 * is required here.
 * @param {string} inputString The string of inputs.
 * @return {!Array<string>} An array of parsed input items.
 */
export function splitInputItems(inputString) {
  const items = inputString.split('|').filter((itemString) => itemString);
  if (items.length <= 0) {
    throw new Error('Invalid input string: ' + inputString);
  }
  return items;
}

/**
 * Determines the block type per a config string, and returns the corresponding
 * block definition.
 * @param {string} config
 * @param {!Object} blockDefs The definition of all blocks.
 * @return {!Object} The definition of the block.
 */
export function getBlockDefPerConfigString(config, blockDefs) {
  if (config.includes(',')) {
    return blockDefs.expression;
  } else if (blockDefs.operator.validValues.includes(config)) {
    return blockDefs.operator;
  } else if (blockDefs.parentheses.validValues.includes(config)) {
    return blockDefs.parentheses;
  } else if (config.match(/^[0-9.eE\-]+$/)) {
    return blockDefs.number;
  } else if (config.match(/^[a-zA-Z_]+$/)) {
    return blockDefs.variable;
  } else {
    throw new Error('Invalid config: ' + config);
  }
}
