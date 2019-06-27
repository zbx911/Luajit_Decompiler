using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luajit_Decompiler.dis.Constants;

namespace Luajit_Decompiler.dis
{
    /// <summary>
    /// TODO: Handle debug info.
    /// </summary>
    class Prototype
    {
        private byte[] bytes; //remaining bytes of the bytecode. The file header must be stripped from this list. Assumes next 7 bytes are for the prototype header.

        //Prototype Header Info : These 7 bytes are also from luajit lj_bcwrite.
        private byte flags; //whether or not to strip debug info.
        private byte numberOfParams; //number of params in the method
        private byte frameSize; //# of prototypes - 1 inside the prototype?
        private byte sizeUV; //# of upvalues
        private int sizeKGC; //size of the constants section? number of strings?
        private int sizeKN; //# of constant numbers to be read after strings.
        //Instruction Count: This part of header is handled in PackBCInstructions.

        private Stack<Prototype> protoStack; //the stack of all prototypes.
        private int debugSize; //size of the debug info section
        private int firstLine; //size of the first line of debug info?
        private int numLines; //number of debug info lines?
        private int prototypeSize;

        //These fields define the prototype. Useful for the decompilation module.
        public List<BytecodeInstruction> bytecodeInstructions; //bytecode asm instructions. { OP, A (BC or D) }
        public List<UpValue> upvalues;
        public List<BaseConstant> constantsSection; //entire constants section byte values (excluding upvalues). Order is important as constants operate by index in the bc instruction registers.
        public List<Prototype> prototypeChildren; //references to child prototypes.

        public Prototype(byte[] bytes, ref int offset, int protoSize, Stack<Prototype> protoStack, byte fileFlag) //fileFlag from the file header. 0x02 = strip debug info.
        {
            this.bytes = bytes;
            this.protoStack = protoStack;
            prototypeSize = protoSize;
            bytecodeInstructions = new List<BytecodeInstruction>();
            upvalues = new List<UpValue>();
            constantsSection = new List<BaseConstant>();
            prototypeChildren = new List<Prototype>();

            //prototype header and instructions section
            flags = Disassembler.ConsumeByte(bytes, ref offset); //# of tables for instance?
            numberOfParams = Disassembler.ConsumeByte(bytes, ref offset);
            frameSize = Disassembler.ConsumeByte(bytes, ref offset); //# of functions - 1?
            sizeUV = Disassembler.ConsumeByte(bytes, ref offset);
            sizeKGC = Disassembler.ConsumeUleb(bytes, ref offset);
            sizeKN = Disassembler.ConsumeUleb(bytes, ref offset);
            PackBCInstructions(bytes, ref offset); //must be here since the next bytes are the instruction count & bytecode instruction bytes.
            if ((fileFlag & 0x02) == 0)
            {
                debugSize = Disassembler.ConsumeUleb(bytes, ref offset);
                if(debugSize > 0)
                {
                    firstLine = Disassembler.ConsumeUleb(bytes, ref offset);
                    numLines = Disassembler.ConsumeUleb(bytes, ref offset);
                }
            }
            PackConstants(bytes, ref offset);
        }

        /// <summary>
        /// Interprets the bytecode instructions and packs them up into the list.
        /// </summary>
        private void PackBCInstructions(byte[] bytes, ref int offset)
        {
            int instructionSize = 4; //size of bc instructions.
            int instructionCount = Disassembler.ConsumeUleb(bytes, ref offset) * instructionSize; //# of bytecode instructions. Part of the 7 byte proto header.
            byte[] instructionBytes = Disassembler.ConsumeBytes(bytes, ref offset, instructionCount);
            for(int i = 0; i < instructionBytes.Length; i += 4)
            {
                BytecodeInstruction bci = new BytecodeInstruction(Opcode.ParseOpByte(instructionBytes[i]));
                bci.AddRegister(instructionBytes[i + 1]);
                bci.AddRegister(instructionBytes[i + 2]);
                bci.AddRegister(instructionBytes[i + 3]);
                bytecodeInstructions.Add(bci);
            }
        }

        /// <summary>
        /// Interprets the constants section of the bytecode and packs them into their appropriate lists.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        private void PackConstants(byte[] bytes, ref int offset)
        {
            //upvalues first, then global constants, then numbers. Lastly, any debug info
            if(sizeUV > 0)
            {
                byte[] uv = Disassembler.ConsumeBytes(bytes, ref offset, sizeUV * 2);
                for(int i = 0; i < uv.Length; i += 2)
                    upvalues.Add(new UpValue(uv[i], uv[i + 1]));
            }
            if(sizeKGC > 0)
            {
                for (int i = 0; i < sizeKGC; i++)
                    ReadKGC(bytes, ref offset);
            }
            if(sizeKN > 0)
            {
                for (int i = 0; i < sizeKN; i++)
                    ReadKN(bytes, ref offset);
            }
            if(debugSize > 0)
            {
                //for now, skip over the debug section.
                Disassembler.ConsumeBytes(bytes, ref offset, debugSize);
            }
        }

        private void ReadKGC(byte[] bytes, ref int offset)
        {
            byte typeByte;
            typeByte = Disassembler.ConsumeByte(bytes, ref offset);
            switch (typeByte)
            {
                case 0: //Child Prototype
                    prototypeChildren.Add(protoStack.Pop());
                    break;
                case 1: //Table
                    constantsSection.Add(new CTable(new TableConstant(bytes, ref offset)));
                    break;
                case 2: //sInt64 => Uleb
                    constantsSection.Add(new CInt(Disassembler.ConsumeUleb(bytes, ref offset)));
                    break;
                case 3: //uInt64 => Uleb
                    constantsSection.Add(new CInt(Disassembler.ConsumeUleb(bytes, ref offset))); //May need special treatment; but for now, just read it as an integer.
                    break;
                case 4: //complex number => LuaNumber => 2 Ulebs
                    constantsSection.Add(new CLuaNumber(new LuaNumber(Disassembler.ConsumeUleb(bytes, ref offset), Disassembler.ConsumeUleb(bytes, ref offset))));
                    break;
                default: //string: length is typebyte - 5.
                    constantsSection.Add(new CString(ASCIIEncoding.Default.GetString(Disassembler.ConsumeBytes(bytes, ref offset, typeByte - 5))));
                    break;
            }
        }

        private void ReadKN(byte[] bytes, ref int offset)
        {
            constantsSection.Add(new CInt(Disassembler.ConsumeByte(bytes, ref offset) / 2));
        }

        /// <summary>
        /// Returns this prototype in string format. For use with writing the asm output to a text file.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine(GetHeaderText());
            result.AppendLine("--Bytecode Instructions--");
            for (int i = 0; i < bytecodeInstructions.Count; i++)
                result.AppendLine(bytecodeInstructions[i].ToString());
            result.AppendLine();
            result.AppendLine("--Prototype Children--");
            result.AppendLine("Count{ " + prototypeChildren.Count + " };");
            result.AppendLine();
            result.AppendLine("--Upvalues--");
            for (int i = 0; i < upvalues.Count; i++)
                result.AppendLine(upvalues[i].ToString());
            result.AppendLine();
            result.AppendLine("--Constants--");
            for (int i = 0; i < constantsSection.Count; i++)
                result.AppendLine(constantsSection[i].ToString());
            result.AppendLine();
            result.AppendLine("--End--");
            result.AppendLine();
            return result.ToString();
        }

        /// <summary>
        /// Returns information regarding the prototype header.
        /// </summary>
        /// <returns></returns>
        private string GetHeaderText()
        {
            return "Prototype_Size: " + prototypeSize + "; " + "Flags: " + flags + "; " + "#_Params: " + numberOfParams + "; " + "Frame_Size: " + frameSize + "; " +
                "Upvalue_Size: " + sizeUV + "; " + "KGC_Size: " + sizeKGC + "; " + "KN_Size: " + sizeKN + "; " + "Instruction_Count: " + bytecodeInstructions.Count + ";\n";
        }
    }

    /// <summary>
    /// Storage class for lua upvalues.
    /// </summary>
    class UpValue
    {
        public int value1 { get; set; }
        public int value2 { get; set; }

        public UpValue(int v1, int v2)
        {
            value1 = v1;
            value2 = v2;
        }

        public override string ToString()
        {
            return "Upvalue{ " + value1 + ", " + value2 + " };";
        }
    }
}