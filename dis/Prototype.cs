using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luajit_Decompiler.dis.Constants;

namespace Luajit_Decompiler.dis
{
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
        public Prototype parent; //each child will only have 1 parent. or is the root.
        public int index; //naming purposes.

        /// <summary>
        /// Stores all information related to a single prototype.
        /// </summary>
        /// <param name="bytes">All bytecode bytes.</param>
        /// <param name="offset">Current offset in the bytecode array.</param>
        /// <param name="protoSize">Size of the prototype in # of bytes.</param>
        /// <param name="protoStack">Stack of all prototypes to determine children of prototypes.</param>
        /// <param name="fileFlag">If debug information is stripped or not.</param>
        /// <param name="index">For naming purposes to determine children.</param>
        public Prototype(byte[] bytes, ref int offset, int protoSize, Stack<Prototype> protoStack, byte fileFlag, int index) //fileFlag from the file header. 0x02 = strip debug info.
        {
            this.bytes = bytes;
            this.protoStack = protoStack;
            prototypeSize = protoSize;
            bytecodeInstructions = new List<BytecodeInstruction>();
            upvalues = new List<UpValue>();
            constantsSection = new List<BaseConstant>();
            prototypeChildren = new List<Prototype>();
            this.index = index;

            //prototype header and instructions section
            flags = Disassembler.ConsumeByte(bytes, ref offset); //# of tables for instance?
            numberOfParams = Disassembler.ConsumeByte(bytes, ref offset);
            frameSize = Disassembler.ConsumeByte(bytes, ref offset); //# of functions - 1?
            sizeUV = Disassembler.ConsumeByte(bytes, ref offset);
            sizeKGC = Disassembler.ConsumeUleb(bytes, ref offset);
            sizeKN = Disassembler.ConsumeUleb(bytes, ref offset);
            PackBCInstructions(bytes, ref offset, fileFlag); //must be here since the next bytes are the instruction count & bytecode instruction bytes.
            PackConstants(bytes, ref offset);
        }

        /// <summary>
        /// Interprets the bytecode instructions and packs them up into the list.
        /// </summary>
        private void PackBCInstructions(byte[] bytes, ref int offset, byte fileFlag)
        {
            //Console.Out.WriteLine(GetHeaderText()); //debug
            int instructionSize = 4; //size of bc instructions.
            int instructionCount = Disassembler.ConsumeUleb(bytes, ref offset) * instructionSize; //# of bytecode instructions. Part of the 7 byte proto header.
            //There is debug info here if flags are present.
            if ((fileFlag & 0x02) == 0)
            {
                debugSize = Disassembler.ConsumeUleb(bytes, ref offset);
                if (debugSize > 0)
                {
                    firstLine = Disassembler.ConsumeUleb(bytes, ref offset);
                    numLines = Disassembler.ConsumeUleb(bytes, ref offset);
                }
            }
            byte[] instructionBytes = Disassembler.ConsumeBytes(bytes, ref offset, instructionCount);
            int bciIndex = 0;
            for(int i = 0; i < instructionBytes.Length; i += 4)
            {
                BytecodeInstruction bci = new BytecodeInstruction(Opcode.ParseOpByte(instructionBytes[i]), bciIndex);
                bci.AddRegister(instructionBytes[i + 1]); //A
                bci.AddRegister(instructionBytes[i + 2]); //C {C + B = D}
                bci.AddRegister(instructionBytes[i + 3]); //B
                bytecodeInstructions.Add(bci);
                bciIndex++;
               //Console.Out.WriteLine(bci); //debug
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
            #region constants debugging
            //foreach (UpValue v in upvalues)
            //    Console.Out.WriteLine(v);
            //foreach (BaseConstant b in constantsSection)
            //    Console.Out.WriteLine(b);
            #endregion
        }

        private void ReadKGC(byte[] bytes, ref int offset)
        {
            byte typeByte;
            typeByte = Disassembler.ConsumeByte(bytes, ref offset);
            switch (typeByte)
            {
                case 0: //Child Prototype
                    Prototype child = protoStack.Pop();
                    child.parent = this; //store a reference to this parent in the child proto.
                    prototypeChildren.Add(child);
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
                case 4: //complex number => LuaNumber => 2 Ulebs --Note: according to DiLemming, double is 2 ulebs and complex is 4 ulebs.
                    constantsSection.Add(new CLuaNumber(new LuaNumber(Disassembler.ConsumeUleb(bytes, ref offset), Disassembler.ConsumeUleb(bytes, ref offset))));
                    break;
                default: //string: length is typebyte - 5.
                    constantsSection.Add(new CString(ASCIIEncoding.Default.GetString(Disassembler.ConsumeBytes(bytes, ref offset, typeByte - 5))));
                    break;
            }
        }

        private void ReadKN(byte[] bytes, ref int offset)
        {
            //Slightly modified DiLemming's code for reading number constants.
            int a = Disassembler.ConsumeUleb(bytes, ref offset);
            bool isDouble = (a & 1) > 0;
            a >>= 1;
            if(isDouble)
            {
                int b = Disassembler.ConsumeUleb(bytes, ref offset);
                CDouble.KNUnion knu = new CDouble.KNUnion();
                knu.ulebA = a;
                knu.ulebB = b;
                //Console.Out.WriteLine(knu.knumVal); //debug
                constantsSection.Add(new CDouble(knu.knumVal));
            }
            else //is integer
            {
                constantsSection.Add(new CInt(a));
                //Console.Out.WriteLine(a); //debug
            }
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
            for (int i = 0; i < prototypeChildren.Count; i++)
                result.AppendLine("Child{ " + "Prototype: " + prototypeChildren[i].index + " };");
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

        /// <summary>
        /// Generates a somewhat unique ID label by summing header information and hashing it, then returning the first 5 characters of the hash.
        /// </summary>
        /// <returns></returns>
        public string GetIdFromHeader()
        {
            int sum = prototypeSize + flags + numberOfParams + frameSize + sizeUV + sizeKGC + sizeKN + bytecodeInstructions.Count;
            char[] hash = sum.GetHashCode().ToString().ToCharArray();
            return hash.ToString();
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