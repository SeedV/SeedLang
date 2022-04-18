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
 * @fileoverview Unittests for blocks.js.
 */

// @ts-check

import assert from 'assert';
import {getBlockList, render} from '../blocks.js';

/**
 * Asserts that the shape has a certain height.
 * @param {string} svg
 * @param {number} height
 */
function assertShapeHeight(svg, height) {
  const re = RegExp(`<svg.*?height="${height}".*?>`);
  assert.match(svg, re);
}

describe('getBlockList', function() {
  it('supports a certain number of blocks', function() {
    assert.strictEqual(getBlockList().length, 16);
  });
});

describe('renderRoundRectValue', function() {
  it('renders svg tags', function() {
    assert.match(render('number', null), /<svg.*<\/svg>/);
  });

  it('renders a certain height for number and string blocks', function() {
    const expectedHeight = 30 * 2 + 6 * 2 + 14;
    let svg = render('number', null);
    assertShapeHeight(svg, expectedHeight);

    svg = render('string', null);
    assertShapeHeight(svg, expectedHeight);
  });
});

describe('renderWithUserConfig', function() {
  it('renders valid string block', function() {
    assert.match(render('string', '1234567890'), /1234567890/);
  });

  it('throws error for invalid string config', function() {
    assert.throws(() => render('operator', '1234567890'));
  });
});

describe('renderOctagonToken', function() {
  it('renders a certain height for operators', function() {
    const expectedHeight = 30 * 2 + 6 * 2 + 14;
    let svg = render('operator', null);
    assertShapeHeight(svg, expectedHeight);

    svg = render('parentheses', null);
    assertShapeHeight(svg, expectedHeight);
  });
});

describe('renderSimpleStatement', function() {
  it('renders a certain height for simple statements', function() {
    let expectedHeight = 30 * 2 + 6 * 2 + 14 + 4;
    let svg = render('break', null);
    assertShapeHeight(svg, expectedHeight);

    expectedHeight += 2 * 6;
    svg = render('set', 'x|3.14');
    assertShapeHeight(svg, expectedHeight);

    expectedHeight += 2 * 6;
    svg = render('set', 'x|x,+,3.14');
    assertShapeHeight(svg, expectedHeight);
  });
});

describe('renderCompoundStatement', function() {
  it('renders a certain height for repeat statement', function() {
    const headerHeight = 14 + 6 * 4;
    const statementHeight = 14 + 6 * 6;
    const footerHeight = 4 + 6;
    const expectedHeight =
        30 * 2 + 4 + headerHeight + statementHeight + 2 * 6 + footerHeight;
    const svg = render('repeat', '{10}{set:x|x,+,1}');
    assertShapeHeight(svg, expectedHeight);
  });

  it('renders a certain height for ifElse statement', function() {
    const headerHeight1 = 14 + 6 * 6;
    const headerHeight2 = 14 + 6 * 2;
    const statementGroupHeight1 = 14 + 6 * 6 + 14 + 6 * 4 + 6;
    const statementGroupHeight2 = 14 + 6 * 6;
    const footerHeight = 4 + 6;
    const expectedHeight =
        30 * 2 + 4 + headerHeight1 + statementGroupHeight1 + 2 * 6 +
        headerHeight2 + statementGroupHeight2 + 2 * 6 +
        footerHeight;
    const svg = render('ifElse', '{y,>,3}{set:x|x,+,1_set:y|0}{set:x|x,-,1}');
    assertShapeHeight(svg, expectedHeight);
  });
});
