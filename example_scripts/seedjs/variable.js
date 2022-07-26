// All variables must be declared with "let" before using.
// The language doesn't support "const", "var", and hoisting.

let a = "hello"; // Global scope

function greet1() {
  a = 3;
}

a; // hello
greet1();
a; // 3

a = "hello";

function greet2() {
  let b = "World"; // Local scope
  a + b;
}

greet2();

let x = 1;

if (x == 1) {
  let y = 2; // Block scope
  x + y; // 3
}

function greet3() {
  let y = 3; // Local scope
  x + y; // 4
  if (y == 3) {
    let z = 4; // Block scope
    x + y + z; // 8
  }
}

greet3();
