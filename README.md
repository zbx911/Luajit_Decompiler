# Luajit_Decompiler

Decompiles LuaJit 2.0 compiled Lua files into an equivalent representation of Lua source code.

***The dec/ part of this project is currently unfinished.***

# Disassembler (dis/)
- consts/ **[Lua dynamic data types restructured as a class tree]**
  - TableConstant.cs **[Reads a constant table from the LuaJit bytecode]**
  - C(Type).cs **[A container for a variable whos type can vary]**
  - BaseConstant.cs **[Parent of all C(Type).cs files]**
- Disassembler.cs **[Responsible for creating LuaJit prototypes from raw bytes and writing the disassembled prototypes to file]**
- BytecodeInstruction.cs **[A container class for LuaJit bytecode instructions]**
  - Bytecode instructions are read: Opcode, Registers.
  - Registers are read: A, C, B with optional D register that is a union: (registerC U registerB) = regD.
- Opcode.cs **[All Opcodes in LuaJit in their proper order]**
- Prototype.cs **[The main container for each prototype of a LuaJit file. A LuaJit compiled file is a prototype which contains child prototypes]**
- UpValue.cs **[A container class for LuaJit upvalues. An upvalue references a constant (and/or maybe a slot index?) in a parent prototype if the value of TableLocation is 192]**

# Decompiler (dec/)
- Decompiler.cs **[Responsible for decompiling entire files]**
- DecompilePrototype.cs **[Decompiles each prototype that makes up a file. Chunks up bytecode instructions of a prototype into code blocks]**
- dec/data
  - Block.cs **[A chunk of prototype bytecode instructions labeled by index]**
  - ControlFlowGraph.cs **[A graph represented as a matrix. The graph represents the connections between each block jump or comparison opcode's targeted block]**
    - matrix[block1, block2] = 0 -> block1's jump/comparsion instructions does not point to block2
    - matrix[block1, block2] = 1 -> block1's jump/comparsion instructions points to block2
    - matrix[block1, block1] = 2 -> block1 is the start of the loop.
    - matrix[block1, block2] = 3 -> block1 is the body of a loop that starts at block2
- dec/lua_formatting **[High level tools for writing lua source]**
  - DecompiledLua.cs **[Stores the decompiled lua source of a single prototype]**
- dec/state_machine
  - LStateContext.cs **[Contains information required by LState derived classes to write lua]**
  - dec/statemachine/states **[All LState derived states used to write the lua of a particular bytecode instruction]**
  - StateMap.cs **[A mapping of Opcode -> LState]**
  - BlockWriter.cs **[Writes the blocks associated with one prototype]**
  
# Road Map
- [ ] Refactor FileManager.cs
- [ ] Refactor dis/*.cs
- [ ] Finish dec/
  - [ ] Implement all states in dec/states/
  - [ ] Finish dec/lua_formatting/
  - [ ] Finish DecompilePrototype.cs
- [ ] Refactor dec/
  - [ ] Refactor dec/data/*.cs
  - [ ] Refactor dec/lua_formatting/*.cs
  - [ ] Refactor dec/state_machine/*.cs
    - [ ] Refactor dec/state_machine/states/*.cs
  
