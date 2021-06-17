import assert from 'assert';
import {getBlockList, render} from '../blocks.js';

describe('getBlockList', function() {
  it('checkNumber', function() {
    assert.strictEqual(getBlockList().length, 1);
  });
});

describe('render', function() {
  it('checkSVG', function() {
    assert(render('number').match('<svg.*</svg>'));
  });
});
