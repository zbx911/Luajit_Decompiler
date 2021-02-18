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
- Air.cs **[A linear intermediate representation in the format [block](trueBlock or falseBlock) with markers for enclosures and conditional]**
- dec/data
  - Block.cs **[A chunk of prototype bytecode instructions labeled by index]**
  - Condition.cs **[A container for Air conditionals]**
  
# Road Map
- [x] Refactor FileManager.cs
- [x] Refactor dis/*.cs
- [ ] Finish dec/
  - [ ] Regroup bytecode instructions by state based on dis.opcodes grouping.
  - [ ] Implement all states in dec/states/
  - [ ] Finish dec/lua_formatting/
  - [ ] Finish DecompilePrototype.cs
- [ ] Refactor dec/
  - [ ] Refactor dec/data/*.cs
  - [ ] Refactor dec/lua_formatting/*.cs
  - [ ] Refactor dec/state_machine/*.cs
    - [ ] Refactor dec/state_machine/states/*.cs
  
