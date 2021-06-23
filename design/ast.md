# SeedAST

The SeedAST (Seed Abstract Syntax Tree) is a high-level representation of the
SeedBlock and SeedX program structure. It can be thought of as an abstract
representation of the source code. The specification of the AST nodes is
specified using the [Zephyr Abstract Syntax Definition Language
(ASDL)](https://www.usenix.org/legacy/publications/library/proceedings/dsl97/full_papers/wang/wang.pdf).

The Zephyr Abstract Syntax Description Language (ASDL) is a simple declarative
language designed to describe the tree-like data structures in compilers. Tools
can convert ASDL descriptions into the appropriate data structure definitions
and functions in any target language.

The definition of the AST nodes for SeedAST is found in the file
[grammars/SeedAST.asdl](/grammars/SeedPython.g4).
