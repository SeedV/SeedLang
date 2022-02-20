# SeedVM

## Introduction

SeedVM is a register-based virtual machine which can interpret and execute the
SeedLang byte code. Register-based virtual machines avoid several push and pop
instructions that stack-based virtual machines need to move values around the
stack. These instructions are particularly expensive, because they involve copy
of the values.

The design intention of SeedVM is to make a stable, secure and fast virtual
machine for the visualizable low-code programming environment that focuses on
educational purposes.

The SeedLang program is translated from SeedAST to byte code by the compiler.
The design of the byte code is derived from the Lua 5.1 virtual machine. Please
refer to [A No-Frills Introduction to Lua 5.1 VM
Instructions](http:#underpop.free.fr/l/lua/docs/a-no-frills-introduction-to-lua-5.1-vm-instructions.pdf).

## SeedLang Value Types

SeedLang is a dynamically typed programming language. Types are attached to
values rather than variables. There are 8 value types in SeedLang: `Nil`,
`Boolean`, `Number`, `String`, `List`, `Table`, `Function` and `Module`.

Values of all types are the first-class values. They can be stored in global
variables, local variables, object properties, list and table fields. They can
be passed as arguments to functions, and returned from functions as well.

All the SeedLang values are objects from SeedLang's perspective. They all
support properties and functions like `name`, `type`, `doc`, `toString()`, etc.
But in SeedVm, `Nil`, `Boolean` and `Number` are implemented with primitive
number types, other types are implemented with objects of the host language.

The description of 8 value types are listed as follows:

- `Nil`: Nil value, only one value `nil`
- `Boolean`: Boolean value, `true` or `false`
- `Number`: Double precision floating point number
- `String`: A sequential collection of characters to represent a text
- `List`: A list of values that can be access by index
- `Table`: A dictionary to hold key and value pairs
- `Function`: Function object which could be a module-level function or a member
  function
- `Module`: Module object which can hold functions and objects in it

## Instruction Basics

All SeedLang instructions are 32 bits fix-sized unsigned integer. There are
three instruction types (`iABC`, `iABx`, `iAsBx`) and 24 (numbered from 0 to 23)
opcodes. The layout of the three instruction types are illustrated as follows:

```text
         0 0 0 0 0 0 0 0 0 0 1 1 1 1 1 1 1 1 1 1 2 2 2 2 2 2 2 2 2 2 3 3
         0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
        -----------------------------------------------------------------
iABC    | Opcode: 6 |     A: 8      |      B: 9       |      C: 9       |
        -----------------------------------------------------------------
iABx    | Opcode: 6 |     A: 8      |              Bx: 18               |
        -----------------------------------------------------------------
iAsBx   | Opcode: 6 |     A: 8      |             sBx: 18               |
        -----------------------------------------------------------------
```

There are at most 64 opcodes for SeedLang byte code, because the opcode occupies
the lower 6 bits of the instruction.

Instruction fields are encoded as simple unsigned integer values, except for
`sBx`. The field sBx represents negative numbers, but it doesn't use 2's
complement. Instead, it has a bias equal to half the maximum integer that can be
represented by its unsigned counterpart, `Bx`. For a field size of 18 bits, `Bx`
can hold a maximum unsigned integer value of 262143, and so the bias is 131071
(calculated as 262143 >> 1). A signed value of x will be encoded as (x +
131071). So the range of `sBx` is [-131071, 131072].

## Instruction Notation

| Notation               | Description                                        |
| ---------------------- | -------------------------------------------------- |
| `PC`                   | Program Counter                                    |
| `R(A)`, `R(B)`, `R(C)` | Register indexed by `A`, `B` or `C`                |
| `RK(B)`, `RK(C)`       | Register or constant indexed by `B` or `C`         |
| `Kst(n)`               | Constant in the constant list indexed by `n`       |
| `Gbl[sym]`             | Global variable named with the string symbol `sym` |
| `sBx`                  | Signed offset for `JMP` instruction                |

For `RK(B)` and `RK(C)`, the `MSB` (most significant bit) of `B` or `C` is used to
indicate if `B` or `C` is the index of the register or constant:

- `MSB` == 0: `B` or `C` is the index of the register
- `MSB` == 1: (`B` - 256) or (`C` - 256) is the index of the constant in the
  constant list

## Instruction Set

All the SeedLang instructions are listed as follows:

| Opcode | Name       | Description                                         |
| :----: | ---------- | --------------------------------------------------- |
|   0    | `MOVE`     | Copy a value between registers                      |
|   1    | `LOADBOOL` | Load a boolean value into a register                |
|   2    | `LOADK`    | Load a constant into a register                     |
|   3    | `GETGLOB`  | Read a global variable into a register              |
|   4    | `SETGLOB`  | Write a register value into a global variable       |
|   5    | `NEWTUPLE` | Create a new tuple with the initial elements        |
|   6    | `NEWLIST`  | Create a new list with the initial elements         |
|   7    | `GETELEM`  | Read a list or table element into a register        |
|   8    | `SETELEM`  | Write a register value into a list or table element |
|   9    | `ADD`      | Addition operation                                  |
|   10   | `SUB`      | Subtract operation                                  |
|   11   | `MUL`      | Multiply operation                                  |
|   12   | `DIV`      | Divide operation                                    |
|   13   | `MOD`      | Modulus (reminder) operation                        |
|   14   | `POW`      | Exponentiation operation                            |
|   15   | `UNM`      | Unary minus operation                               |
|   16   | `NOT`      | Logical not operation                               |
|   17   | `LEN`      | Length operation                                    |
|   18   | `JMP`      | Unconditional jump                                  |
|   19   | `EQ`       | Equality test                                       |
|   20   | `LT`       | Less than test                                      |
|   21   | `LE`       | Less than or equal to test                          |
|   22   | `TEST`     | Boolean test, with conditional jump                 |
|   23   | `TESTSET`  | Boolean test, with conditional jump and assignment  |
|   24   | `FORPREP`  | For loop preparation                                |
|   25   | `FORLOOP`  | For loop check                                      |
|   26   | `EVAL`     | Expression evaluation                               |
|   27   | `CALL`     | Call a function                                     |
|   28   | `RETURN`   | Return from a function call                         |

### Move and Load Constant

```shell
MOVE A B                    # R(A) := R(B)
LOADBOOL A B C              # R(A) := (Bool)B; if C then PC++
LOADK A Bx                  # R(A) := Kst(Bx)
```

`MOVE` instruction is not needed for arithmetic expressions. All arithmetic
operators are in 2 or 3 operand style. The entire local stack frame is already
visible to operands `R(A)`, `R(B)` and `R(C)` so there is no need for any extra
MOVE instructions.

Other places where `MOVE` instruction is needed:

- When moving parameters into place for a function call
- When moving values into place for certain instructions where stack order is
  important, e.g. `GETELEM` and `SETELEM`, etc.
- When copying return values into locals after a function call

`RK(B)` and `RK(C)` can only handle constant index less than 256. `LOADK` is
used to load the constant into a register if the index is greater than or equal
to 256.

### Globals

```shell
GETGLOB A Bx            # R[A] := Gbl[Kst(Bx)]
SETGLOB A Bx            # Gbl[Kst(Bx)] := R[A]
```

### List and Table Operations

```shell
NEWTUPLE A B C          # R(A) := (R(B), R(B+1), ..., R(B+C-1))
NEWLIST A B C           # R(A) := [R(B), R(B+1), ..., R(B+C-1)]
GETELEM A B C           # R(A) := R(B)[RK(C)]
SETELEM A B C           # R(A)[RK(B)] := RK(C)
```

### Arithmetic and Logic Operations

```shell
ADD A B C               # R(A) := RK(B) + RK(C)
SUB A B C               # R(A) := RK(B) - RK(C)
MUL A B C               # R(A) := RK(B) * RK(C)
DIV A B C               # R(A) := RK(B) / RK(C)
MOD A B C               # R(A) := RK(B) % RK(C)
POW A B C               # R(A) := RK(B) ^ RK(C)
UNM A B                 # R(A) := -RK(B)
LEN A B                 # R(A) := length of R(B)
```

The operand `B` of `LEN` instruction can be any SeedLang value type:

- `Nil`, `Boolean`, `Number`, `Function`, `Module`: return 1
- `String`, `List`, `Table`: return the length of `R(B)`

### Rational and Logic Instructions

```shell
NOT A B                 # R(A) := not R(B)
EQ A B C                # if (RK(B) == RK(C)) != A then PC++
LT A B C                # if (RK(B) < RK(C)) != A then PC++
LE A B C                # if (RK(B) <= RK(C)) != A then PC++
TEST A C                # if R(A) == C then PC++
TESTSET A B C           # if R(B) != C then R(A) := R(B) else PC++
```

### For Loop

```shell
FORPREP A sBx           # R(A) -= R(A+2); pc += sBx
FORLOOP A sBx           # R(A) += R(A+2); if R(A) <?= R(A+1) then PC += sBx
```

### Jumps and Calls

```shell
JMP sBx                 # PC += sBx
CALL A                  # call function R(A), parameters are R(A+1), ..., R(A+B)
RETURN A B              # if B == 0 return else return R(A)
```

### Expression Evaluation

```shell
Eval A                  # Evaluate R(A)
```
