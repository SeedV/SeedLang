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
 * @fileoverview Unittests for blocks.js.
 */

// @ts-check

import assert from 'assert';
import * as utils from '../block_utils.js';
import {BLOCK_DEFS} from '../blocks.js';

describe('splitListItems', function() {
  it('checkSplit', function() {
    assert.throws(() => utils.splitListItems('', BLOCK_DEFS));

    let items = utils.splitListItems('a,b,c', BLOCK_DEFS);

    assert.strictEqual(items.length, 3);
    assert.strictEqual(items[0].config, 'a');
    assert.strictEqual(items[0].blockDef.background,
        BLOCK_DEFS.variable.background);
    assert.strictEqual(items[1].config, 'b');
    assert.strictEqual(items[1].blockDef.background,
        BLOCK_DEFS.variable.background);
    assert.strictEqual(items[2].config, 'c');
    assert.strictEqual(items[2].blockDef.background,
        BLOCK_DEFS.variable.background);

    items = utils.splitListItems('(,3.14,+,2.71,),>=,counter', BLOCK_DEFS);
    assert.strictEqual(items.length, 7);
    assert.strictEqual(items[0].config, '(');
    assert.strictEqual(items[0].blockDef.background,
        BLOCK_DEFS.parentheses.background);
    assert.strictEqual(items[1].config, '3.14');
    assert.strictEqual(items[1].blockDef.background,
        BLOCK_DEFS.number.background);
    assert.strictEqual(items[2].config, '+');
    assert.strictEqual(items[2].blockDef.background,
        BLOCK_DEFS.operator.background);
    assert.strictEqual(items[6].config, 'counter');
    assert.strictEqual(items[6].blockDef.background,
        BLOCK_DEFS.variable.background);
  });
});

describe('splitInputItems', function() {
  it('checkSplit', function() {
    assert.throws(() => utils.splitInputItems(''));
    assert.throws(() => utils.splitInputItems('|'));

    const items = utils.splitInputItems('a|b|c');
    assert.strictEqual(items.length, 3);
    assert.strictEqual(items[0], 'a');
    assert.strictEqual(items[1], 'b');
    assert.strictEqual(items[2], 'c');
  });
});

describe('splitInputItemsAndCompoundStatements', function() {
  it('checkSplit1', function() {
    const s = 'x,>,3_set:counter|counter,+,1_set:x|x,-,1';
    const ret = utils.splitInputItemsAndCompoundStatements(s, BLOCK_DEFS);
    assert.strictEqual(ret.inputConfigString, 'x,>,3');
    assert.strictEqual(ret.statements.length, 2);
    assert.strictEqual(ret.statements[0].blockConfig, 'counter|counter,+,1');
    assert.strictEqual(ret.statements[1].blockConfig, 'x|x,-,1');
  });

  it('checkSplit2', function() {
    const s = 'x|items_set:y|y,+,x';
    const ret = utils.splitInputItemsAndCompoundStatements(s, BLOCK_DEFS);
    assert.strictEqual(ret.inputConfigString, 'x|items');
    assert.strictEqual(ret.statements.length, 1);
    assert.strictEqual(ret.statements[0].blockConfig, 'y|y,+,x');
  });
});

describe('getBlockDefPerConfigString', function() {
  it('checkTypes', function() {
    let blockDef = utils.getBlockDefPerConfigString('1,+,2', BLOCK_DEFS);
    assert.strictEqual(blockDef.background, BLOCK_DEFS.expression.background);
    blockDef = utils.getBlockDefPerConfigString('==', BLOCK_DEFS);
    assert.strictEqual(blockDef.background, BLOCK_DEFS.operator.background);
    blockDef = utils.getBlockDefPerConfigString(')', BLOCK_DEFS);
    assert.strictEqual(blockDef.background, BLOCK_DEFS.parentheses.background);
    blockDef = utils.getBlockDefPerConfigString('-3.14e0', BLOCK_DEFS);
    assert.strictEqual(blockDef.background, BLOCK_DEFS.number.background);
    blockDef = utils.getBlockDefPerConfigString('foo', BLOCK_DEFS);
    assert.strictEqual(blockDef.background, BLOCK_DEFS.variable.background);
  });
});
