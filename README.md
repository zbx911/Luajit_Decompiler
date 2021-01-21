# Luajit_Decompiler

Decompiles LuaJit 2.0 compiled Lua files into an equivalent representation of Lua source code.

***The dec/ part of this project is currently unfinished.***

# Disassembler (dis/)
- consts/ **[Lua dynamic data types restructured as a class tree]**
  - TableConstant.cs **[Reads a constant table from the LuaJit bytecode]**
- Disassembler.cs **[Responsible for creating LuaJit prototypes from raw bytes and writing the disassembled prototypes to file]**
- BytecodeInstruction.cs **[A container class for LuaJit bytecode instructions]**
  - Bytecode instructions are read: Opcode, Registers.
  - Registers are read: A, C, B with optional D register that is a union: (registerC U registerB) = regD.
- Opcode.cs **[All Opcodes in LuaJit in their proper order]**
- Prototype.cs **[The main container for each prototype of a LuaJit file. A LuaJit compiled file is a prototype which contains child prototypes]**
- UpValue.cs **[A container class for LuaJit upvalues. An upvalue references a constant (and/or maybe a slot index?) in a parent prototype if the value of TableLocation is 192]**

# Decompiler (dec/)
