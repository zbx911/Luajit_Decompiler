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


        public List<BytecodeInstruction> bcis;                              //bytecode asm instructions. { OP, A (BC or D) }
        public List<Prototype> prototypeChildren;                           //references to child prototypes.
        public Prototype parent;                                            //each child will only have 1 parent. or is the root.
        public int index;                                                   //naming purposes.
        public List<UpValue> upvalueTargets;
        public List<BaseConstant> constants;
        public List<string> symbols;
        public List<(string, BaseConstant)> upvalues;

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
            bcis = new List<BytecodeInstruction>();
            prototypeChildren = new List<Prototype>();
            constants = new List<BaseConstant>();
            upvalueTargets = new List<UpValue>();

            this.index = index;
            this.fileFlag = fileFlag;

            //prototype header and instructions section
            flags = bm.ConsumeByte(); //# of tables for instance?
            numberOfParams = bm.ConsumeByte();
            frameSize = bm.ConsumeByte(); //# of functions - 1?
            sizeUV = bm.ConsumeByte();
            sizeKGC = bm.ConsumeUleb();
            sizeKN = bm.ConsumeUleb();
            PackBCInstructions(bm, fileFlag); //must be here since the next bytes are the instruction count & bytecode instruction bytes.
            PackConstants(bm);
            SetUpvalueNames();
        }

        private void SetUpvalueNames()
        {
            upvalues = new List<(string, BaseConstant)>();
            for(int i = 0; i < upvalueTargets.Count; i++)
                upvalues.Add(RecursiveGetUpvalue(this, upvalueTargets[i]));
        }

        private (string, BaseConstant) RecursiveGetUpvalue(Prototype pt, UpValue uv)
        {
            //apparently the first bit of 192 determines if we look at the constants section table or not. 
            //the second bit of 192 means if it is mutable or not. 1 = immutable upvalue -- whatever that means in terms of upvalues...
            if (uv.TableLocation == 192)
                return (pt.parent.symbols[uv.TableIndex], pt.parent.constants[uv.TableIndex]); //possible array index out of bounds here...
            return RecursiveGetUpvalue(pt.parent, pt.parent.upvalueTargets[uv.TableIndex]);
        }

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
                bcis.Add(bci);
                bciIndex++;
            }
        }

        private void PackConstants(ByteManager bm)
        {
            //Upvalues first, then global constants, then numbers. Lastly, any debug info. *Must be in that order*
            if(sizeUV > 0)
            {
                byte[] uv = bm.ConsumeBytes(sizeUV * 2);
                for(int i = 0; i < uv.Length; i += 2)
                    upvalueTargets.Add(new UpValue(uv[i], uv[i + 1]));
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
                PackSymbols(debugSection);
            }
        }

        private void PackSymbols(byte[] debugSection)
        {
            //Format:
            //Line number section which probably map to slots. Duplicates are sometimes present...just skip through until we hit numLines and begin searching for the var names.
            //Var names: ASCII characters terminated with 0x00. 2 bytes of unidentified data after each variable name.
            int nameOffset = SkipToSymbolNameOffset(debugSection);

            if (nameOffset == 0)
                return;

            symbols = CollectSymbols(debugSection, ref nameOffset);
            symbols.RemoveAll(s => s == "");
        }

        private List<string> CollectSymbols(byte[] debugSection, ref int nameOffset)
        {
            StringBuilder name = new StringBuilder();
            List<string> symbols = new List<string>();

            while (nameOffset < debugSection.Length)
            {
                if (debugSection[nameOffset] == 0)
                {
                    symbols.Add(CleanVariableName(name.ToString()));
                    name = new StringBuilder();
                    nameOffset += 3; //skip over terminator + 2 bytes of unknown data.
                }

                if (nameOffset < debugSection.Length)
                {
                    name.Append((char)debugSection[nameOffset]);
                    nameOffset++;
                }
            }
            return symbols;
        }

        private int SkipToSymbolNameOffset(byte[] debugSection)
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
                    constants.Add(new CTable(new TableConstant(bm)));
                    break;
                case 2: //sInt64 => Uleb
                    constants.Add(new CInt(bm.ConsumeUleb()));
                    break;
                case 3: //uInt64 => Uleb
                    constants.Add(new CInt(bm.ConsumeUleb())); //May need special treatment; but for now, just read it as an integer.
                    break;
                case 4: //complex number => LuaNumber => 2 Ulebs --Note: according to DiLemming, double is 2 ulebs and complex is 4 ulebs.
                    constants.Add(new CLuaNumber(new LuaNumber(bm.ConsumeUleb(), bm.ConsumeUleb())));
                    break;
                default: //string: length is typebyte - 5.
                    constants.Add(new CString(ASCIIEncoding.Default.GetString(bm.ConsumeBytes(typeByte - 5))));
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
                constants.Add(new CDouble(knu.knumVal));
            }
            else
            {
                constants.Add(new CInt(a));
            }
        }

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
            for (int i = 0; i < bcis.Count; i++)
                result.AppendLine(bcis[i].ToString());
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
            for (int i = 0; i < upvalueTargets.Count; i++)
                result.AppendLine(upvalueTargets[i].ToString());
            return result.ToString();
        }

        private string GetConstantsText()
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine("--Constants--");
            for (int i = 0; i < constants.Count; i++)
                result.AppendLine(constants[i].ToString());
            return result.ToString();
        }

        private string GetVariableNamesText()
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine("--Variable Names--");
            for (int i = 0; i < symbols.Count; i++)
                result.AppendLine(symbols[i]);
            return result.ToString();
        }

        private string GetHeaderText()
        {
            return "Prototype Size: " + prototypeSize + "; " + "Flags: " + flags + "; " + "# of Params: " + numberOfParams + "; " + "Frame Size: " + frameSize + "; " +
                "Upvalue Size: " + sizeUV + "; " + "KGC Size: " + sizeKGC + "; " + "KN Size: " + sizeKN + "; " + "Instruction Count: " + bcis.Count + "; " 
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