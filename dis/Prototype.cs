using System.Collections.Generic;
using System.Text;
using Luajit_Decompiler.dis.consts;

namespace Luajit_Decompiler.dis
{
    /// <summary>
    /// TODO: Make CUInt base constant and fix ReadKGC's uint64 read.
    /// </summary>
    class Prototype
    {
        private readonly byte[] bytes; //remaining bytes of the bytecode. The file header must be stripped from this list. Assumes next 7 bytes are for the prototype header.

        //Prototype Header Info : These 7 bytes are also from luajit lj_bcwrite.
        private readonly byte flags; //individual prototype flags. I think it also contains if it has debug info?
        private readonly byte numberOfParams; //number of params in the method
        private readonly byte frameSize; //# of prototypes - 1 inside the prototype?
        private readonly byte sizeUV; //# of upvalues
        private readonly int sizeKGC; //size of the constants section? number of strings?
        private readonly int sizeKN; //# of constant numbers to be read after strings.
        //Instruction Count: This part of header is handled in PackBCInstructions.

        private Stack<Prototype> protoStack; //the stack of all prototypes.
        private int debugSize; //size of the debug info section
        private int firstLine; //size of the first line of debug info?
        private int numLines; //number of debug info lines?
        private readonly int prototypeSize;

        //These fields define the prototype. Useful for the decompilation module.
        public List<BytecodeInstruction> bytecodeInstructions; //bytecode asm instructions. { OP, A (BC or D) }
        public List<UpValue> upvalues;
        public List<BaseConstant> constantsSection; //entire constants section byte values (excluding upvalues). Order is important as constants operate by index in the bc instruction registers.
        public List<string> variableNames; //contains variable names for the prototype assuming that debug information is NOT stripped.
        public List<Prototype> prototypeChildren; //references to child prototypes.
        public Prototype parent; //each child will only have 1 parent. or is the root.
        public int index; //naming purposes.

        public byte fileFlag; //If it has debug info or not.
        public bool HasVarNames { get { return (fileFlag & 0x02) == 0; } } //whether or not the prototype has variable names.

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
            this.fileFlag = fileFlag;
            variableNames = new List<string>();

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
                BytecodeInstruction bci = new BytecodeInstruction(Opcode.ParseOpByte(instructionBytes[i]), bciIndex)
                {
                    regA = instructionBytes[i + 1], // {C union B -> D}
                    regC = instructionBytes[i + 2],
                    regB = instructionBytes[i + 3]
                };
                bytecodeInstructions.Add(bci);
                bciIndex++;
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
                byte[] debugSection = Disassembler.ConsumeBytes(bytes, ref offset, debugSize);
                PackVariableNames(debugSection);
            }
        }

        private void PackVariableNames(byte[] debugSection)
        {
            //Format:
            //Line number section which probably map to slots. Duplicates are sometimes present...just skip through until we hit numLines and begin searching for the var names.
            //Var names: ASCII characters terminated with 0x00. 2 bytes of unidentified data after each variable name.

            //Skip to variable names.
            int nameOffset = 0;
            for(int i = 0; i < debugSection.Length; i++)
            {
                if(debugSection[i] == numLines) //could be duplicated line number due to what I think is a LJ optimization for certain in-lines so we check next as well.
                {
                    if (debugSection[i + 1] == numLines)
                        i++;
                    nameOffset = i + 1; //should be on top of the first ASCII char of a var name.
                    break;
                }
            }
            if (nameOffset == 0) throw new System.Exception("Prototype:PackVariableNames::nameOffset is zero.");

            //begin collection of variable names.
            StringBuilder name = new StringBuilder();
            while(nameOffset < debugSection.Length)
            {
                if(debugSection[nameOffset] == 0)
                {
                    variableNames.Add(name.ToString());
                    name = new StringBuilder();
                    nameOffset += 3; //skip over terminator + 2 bytes of unknown data.
                }
                if (nameOffset < debugSection.Length)
                {
                    name.Append((char)debugSection[nameOffset]);
                    nameOffset++;
                }
            }
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
                CDouble.KNUnion knu = new CDouble.KNUnion
                {
                    ulebA = a,
                    ulebB = b
                };
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
        public int TableIndex { get; set; } //which index of the table to look at.
        public int TableLocation { get; set; } //which table to look at. If it is 192, look at the global constants table at tableIndex. 0 means look at the upvalues table at index in the prototype's parent.

        public UpValue(int v1, int v2)
        {
            TableIndex = v1;
            TableLocation = v2;
        }

        public override string ToString()
        {
            return "Upvalue{ " + TableIndex + ", " + TableLocation + " };";
        }
    }
}