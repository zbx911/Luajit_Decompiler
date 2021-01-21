using System.Collections.Generic;
using System.Text;
using Luajit_Decompiler.dis.consts;

namespace Luajit_Decompiler.dis
{
    class Prototype
    {
        //Prototype Header Info : These 7 bytes are also from luajit lj_bcwrite.
        public readonly byte flags;                                         //individual prototype flags.
        public readonly byte numberOfParams;                                //number of params in the method
        public readonly byte frameSize;                                     //# of slots luajit uses to store variable info.
        private readonly byte sizeUV;                                       //# of upvalues
        private readonly int sizeKGC;                                       //size of the global constants section
        private readonly int sizeKN;                                        //# of constant numbers to be read after strings.

        //Instruction Count: This part of header is handled in PackBCInstructions.
        private Stack<Prototype> protoStack;                                
        private int debugSize;                                              //size of the debug info section
        private int firstLine;                                              //size of the first line of debug info
        private int numLines;                                               //number of debug info lines
        private readonly int prototypeSize;

        //These fields define the prototype. Useful for the decompilation module.
        public List<BytecodeInstruction> bytecodeInstructions;              //bytecode asm instructions. { OP, A (BC or D) }
        public List<UpValue> upvalues;                                      
        public List<BaseConstant> constantsSection;                         //entire constants section byte values (excluding upvalues). Order is important as constants operate by index in the bc instruction registers.
        public List<string> variableNames;                                  //contains variable names for the prototype assuming that debug information is NOT stripped.
        public List<Prototype> prototypeChildren;                           //references to child prototypes.
        public Prototype parent;                                            //each child will only have 1 parent. or is the root.
        public int index;                                                   //naming purposes.

        public byte fileFlag;                                               //If it has debug info or not.
        public bool HasVarNames { get { return (fileFlag & 0x02) == 0; } }  //whether or not the prototype has variable names.

        /// <summary>
        /// Stores all information related to a single prototype.
        /// </summary>
        /// <param name="bytes">All bytecode bytes.</param>
        /// <param name="offset">Current offset in the bytecode array.</param>
        /// <param name="protoSize">Size of the prototype in # of bytes.</param>
        /// <param name="protoStack">Stack of all prototypes to determine children of prototypes.</param>
        /// <param name="fileFlag">If debug information is stripped or not.</param>
        /// <param name="index">For naming purposes to determine children.</param>
        public Prototype(ByteManager bm, int protoSize, Stack<Prototype> protoStack, byte fileFlag, int index) //fileFlag from the file header. 0x02 = strip debug info.
        {
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
            flags = bm.ConsumeByte(); //# of tables for instance?
            numberOfParams = bm.ConsumeByte();
            frameSize = bm.ConsumeByte(); //# of functions - 1?
            sizeUV = bm.ConsumeByte();
            sizeKGC = bm.ConsumeUleb();
            sizeKN = bm.ConsumeUleb();
            PackBCInstructions(bm, fileFlag); //must be here since the next bytes are the instruction count & bytecode instruction bytes.
            PackConstants(bm);
        }

        /// <summary>
        /// Interprets the bytecode instructions and packs them up into the list.
        /// </summary>
        private void PackBCInstructions(ByteManager bm, byte fileFlag)
        {
            int instructionSize = 4;
            int instructionCount = bm.ConsumeUleb() * instructionSize;

            if ((fileFlag & 0x02) == 0)
            {
                debugSize = bm.ConsumeUleb();
                if (debugSize > 0)
                {
                    firstLine = bm.ConsumeUleb();
                    numLines = bm.ConsumeUleb();
                }
            }

            byte[] instructionBytes = bm.ConsumeBytes(instructionCount);
            int bciIndex = 0;

            for(int i = 0; i < instructionBytes.Length; i += 4)
            {
                BytecodeInstruction bci = new BytecodeInstruction(Opcode.ParseOpByte(instructionBytes[i]), bciIndex);
                bci.registers.a = instructionBytes[i + 1];
                bci.registers.c = instructionBytes[i + 2];
                bci.registers.b = instructionBytes[i + 3];
                bytecodeInstructions.Add(bci);
                bciIndex++;
            }
        }

        /// <summary>
        /// Interprets the constants section of the bytecode and packs them into their appropriate lists.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        private void PackConstants(ByteManager bm)
        {
            //Upvalues first, then global constants, then numbers. Lastly, any debug info. *Must be in that order*
            if(sizeUV > 0)
            {
                byte[] uv = bm.ConsumeBytes(sizeUV * 2);
                for(int i = 0; i < uv.Length; i += 2)
                    upvalues.Add(new UpValue(uv[i], uv[i + 1]));
            }

            if(sizeKGC > 0)
                for (int i = 0; i < sizeKGC; i++)
                    ReadKGC(bm);

            if(sizeKN > 0)
                for (int i = 0; i < sizeKN; i++)
                    ReadKN(bm);

            if(debugSize > 0)
            {
                byte[] debugSection = bm.ConsumeBytes(debugSize);
                PackVariableNames(debugSection);
            }
        }

        private void PackVariableNames(byte[] debugSection)
        {
            //Format:
            //Line number section which probably map to slots. Duplicates are sometimes present...just skip through until we hit numLines and begin searching for the var names.
            //Var names: ASCII characters terminated with 0x00. 2 bytes of unidentified data after each variable name.

            int nameOffset = SkipToVariableNameOffset(debugSection);

            if (nameOffset == 0)
                return;

            CollectVariableNamesToList(debugSection, ref nameOffset);

            variableNames.RemoveAll(s => s == "");
        }

        private void CollectVariableNamesToList(byte[] debugSection, ref int nameOffset)
        {
            StringBuilder name = new StringBuilder();

            while (nameOffset < debugSection.Length)
            {
                if (debugSection[nameOffset] == 0)
                {
                    variableNames.Add(CleanVariableName(name.ToString()));
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

        private int SkipToVariableNameOffset(byte[] debugSection)
        {
            int nameOffset = 0;
            for (int i = 0; i < debugSection.Length; i++)
            {
                if (debugSection[i] == numLines)
                {
                    while (debugSection[i + 1] == numLines)
                        i++;

                    nameOffset = i + 1;
                    break;
                }
            }
            return nameOffset;
        }

        private void ReadKGC(ByteManager bm)
        {
            byte typeByte;
            typeByte = bm.ConsumeByte();

            switch (typeByte)
            {
                case 0: //Child Prototype
                    Prototype child = protoStack.Pop();
                    child.parent = this;
                    prototypeChildren.Add(child);
                    break;
                case 1: //Table
                    constantsSection.Add(new CTable(new TableConstant(bm)));
                    break;
                case 2: //sInt64 => Uleb
                    constantsSection.Add(new CInt(bm.ConsumeUleb()));
                    break;
                case 3: //uInt64 => Uleb
                    constantsSection.Add(new CInt(bm.ConsumeUleb())); //May need special treatment; but for now, just read it as an integer.
                    break;
                case 4: //complex number => LuaNumber => 2 Ulebs --Note: according to DiLemming, double is 2 ulebs and complex is 4 ulebs.
                    constantsSection.Add(new CLuaNumber(new LuaNumber(bm.ConsumeUleb(), bm.ConsumeUleb())));
                    break;
                default: //string: length is typebyte - 5.
                    constantsSection.Add(new CString(ASCIIEncoding.Default.GetString(bm.ConsumeBytes(typeByte - 5))));
                    break;
            }
        }

        private void ReadKN(ByteManager bm)
        {
            //Slightly modified DiLemming's code for reading number constants.
            int a = bm.ConsumeUleb();
            bool isDouble = (a & 1) > 0;
            a >>= 1;

            if(isDouble)
            {
                int b = bm.ConsumeUleb();
                CDouble.KNUnion knu = new CDouble.KNUnion
                {
                    ulebA = a,
                    ulebB = b
                };
                constantsSection.Add(new CDouble(knu.knumVal));
            }
            else
            {
                constantsSection.Add(new CInt(a));
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

            result.AppendLine(GetBciText());

            result.AppendLine(GetChildProtoText());

            result.AppendLine(GetUpvaluesText());

            result.AppendLine(GetConstantsText());

            result.AppendLine(GetVariableNamesText());

            result.AppendLine("--End--");
            return result.ToString();
        }

        private string GetBciText()
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine("--Bytecode Instructions--");
            for (int i = 0; i < bytecodeInstructions.Count; i++)
                result.AppendLine(bytecodeInstructions[i].ToString());
            return result.ToString();
        }

        private string GetChildProtoText()
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine("--Prototype Children--");
            result.AppendLine("Count{ " + prototypeChildren.Count + " };");
            for (int i = 0; i < prototypeChildren.Count; i++)
                result.AppendLine("Child{ " + "Prototype: " + prototypeChildren[i].index + " };");
            return result.ToString();
        }

        private string GetUpvaluesText()
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine("--Upvalues--");
            for (int i = 0; i < upvalues.Count; i++)
                result.AppendLine(upvalues[i].ToString());
            return result.ToString();
        }

        private string GetConstantsText()
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine("--Constants--");
            for (int i = 0; i < constantsSection.Count; i++)
                result.AppendLine(constantsSection[i].ToString());
            return result.ToString();
        }

        private string GetVariableNamesText()
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine("--Variable Names--");
            for (int i = 0; i < variableNames.Count; i++)
                result.AppendLine(variableNames[i]);
            return result.ToString();
        }

        /// <summary>
        /// Returns information regarding the prototype header.
        /// </summary>
        /// <returns></returns>
        private string GetHeaderText()
        {
            return "Prototype Size: " + prototypeSize + "; " + "Flags: " + flags + "; " + "# of Params: " + numberOfParams + "; " + "Frame Size: " + frameSize + "; " +
                "Upvalue Size: " + sizeUV + "; " + "KGC Size: " + sizeKGC + "; " + "KN Size: " + sizeKN + "; " + "Instruction Count: " + bytecodeInstructions.Count + "; " 
                + "Debug Size: " + debugSize + "; " + "# of Debug Lines: " + numLines + ";\n";
        }

        private string CleanVariableName(string unclean)
        {
            string clean = "";
            for (int i = 0; i < unclean.Length; i++)
                if (unclean[i] > 32)
                    clean += unclean[i];
            return clean;
        }
    }
}