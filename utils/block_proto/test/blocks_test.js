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
import {getBlockList, render} from '../blocks.js';

describe('getBlockList', function() {
  it('checkNumber', function() {
    assert.strictEqual(getBlockList().length, 6);
  });
});

describe('renderRoundRectValue', function() {
  it('checkSVG', function() {
    assert.match(render('number', null), /<svg.*<\/svg>/);
  });
  it('checkNumberAndString', function() {
    // The SVG code of Number blocks must not contain string delimiters.
    assert.doesNotMatch(render('number', null), /#ffcc00/);
    // On the other hand, string blocks always have the delimiter color string.
    assert.match(render('string', null), /#ffcc00/);
  });
});

describe('renderWithUserConfig', function() {
  it('validUserConfig', function() {
    assert.match(render('string', '1234567890'), /1234567890/);
  });
  it('invalidUserConfig', function() {
    const svg = render('operator', '1234567890');
    assert.doesNotMatch(svg, /1234567890/);
    assert.match(svg, /\+/);
  });
});
