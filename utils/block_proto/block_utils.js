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
 *  - An expression, e.g., '(,3.14,+,2.71,),&gt;=,counter'.
 *  - A list literal, e.g., '1,2,3,4,5'.
 *
 * As this is an internal prototype drawing utility, we assume the input list
 * string is correctly built and formatted. Neither syntax parser nor semantic
 * checker is required here.
 * @param {string} listString The comma separated list string.
 * @param {!Object} blockDefs The definition of all blocks.
 * @param {!Array<string>} validOperators The list of valid operators.
 * @return {!Array<Object>} An array of tagged items.
 */
export function splitListItems(listString, blockDefs, validOperators) {
  const itemStrings = listString.split(',');
  const items = [];
  for (const itemString of itemStrings) {
    if (!itemString) {
      break;
    }
    if (validOperators.includes(itemString)) {
      items.push({
        blockDef: blockDefs.operator,
        config: itemString,
      });
    } else if (itemString.match(/^[0-9.eE\-]+$/)) {
      items.push({
        blockDef: blockDefs.number,
        config: itemString,
      });
    } else if (itemString.match(/^[a-zA-Z_]+$/)) {
      items.push({
        blockDef: blockDefs.variable,
        config: itemString,
      });
    } else {
      throw new Error('Not supported item in lists: ' + itemString);
    }
  }
  if (items.length <= 0) {
    throw new Error('Invalid list string: ' + listString);
  }
  return items;
}
