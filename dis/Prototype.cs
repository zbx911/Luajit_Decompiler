using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luajit_Decompiler.dis
{
    /// <summary>
    /// TODO:
    /// Handle constants section.
    /// </summary>
    class Prototype
    {
        private byte[] bytes; //remaining bytes of the bytecode. The initial header must be stripped from this list. Assumes next 6 bytes are for the prototype header.
        private OutputManager manager; //for file output handling.
        private string protoName = "Prototype ";

        //Prototype Header Info : These 7 bytes are also from luajit lj_bcwrite. Note: uleb128 parsing is required for the sizeUV, sizeKGC, and the bytes[6] instructionCount.
        //(for values > 255/256). Also note that if the flags for whether or not to strip debug info is something other than 0x02, then there will be a name before the rest of instructions.
        public byte flags; //whether or not to strip debug info.
        public byte numberOfParams; //number of params in the method
        public byte frameSize; //# of prototypes - 1 inside the prototype?
        public byte sizeUV; //# of upvalues
        public int sizeKGC; //size of the constants section? number of strings?
        public int sizeKN; //# of constant numbers to be read after strings.
        public int instructionCount; //number of bytecode instructions for the prototype.
        public byte[] instructionBytes; //instructions section bytes
        public byte[] constantBytes; //constant section bytes
        public byte[] upvalues; //every 2 bytes is 1 upvalue reference.
        public Prototype child; //the parent of this prototype. (If a parent exists). (pop from protostack).
        private Stack<Prototype> protoStack; //the stack of all prototypes.
        private int tableIndex = 0;

        public Prototype(byte[] bytes, ref int offset, OutputManager manager, int protoSize, Stack<Prototype> protoStack, int nameNDX)
        {
            this.bytes = bytes;
            this.manager = manager;
            this.protoStack = protoStack;
            protoName += nameNDX;
            int instructionSize = 4; //each instruction is 4 bytes.
            int headerSize = 7; //7 bytes in each prototype header.

            //prototype header and instructions section
            flags = Disassembler.ConsumeByte(bytes, ref offset); //# of tables for instance?
            numberOfParams = Disassembler.ConsumeByte(bytes, ref offset);
            frameSize = Disassembler.ConsumeByte(bytes, ref offset); //# of functions - 1?
            sizeUV = Disassembler.ConsumeByte(bytes, ref offset);
            sizeKGC = Disassembler.ConsumeUleb(bytes, ref offset);
            sizeKN = Disassembler.ConsumeUleb(bytes, ref offset);
            //if (nameNDX == 9)//debug work because it is off by 1 byte?
                //offset++;
            instructionCount = Disassembler.ConsumeUleb(bytes, ref offset) * instructionSize;
            instructionBytes = Disassembler.ConsumeBytes(bytes, ref offset, instructionCount);

            //constants section
            int byteTally = headerSize + instructionCount; //tally of both the header and the number of instructions.
            int constantSecSize = protoSize - byteTally; //difference between the size of proto and tally is the constants section byte size.
            constantBytes = Disassembler.ConsumeBytes(bytes, ref offset, constantSecSize);

            //begin writing this prototype.
            DebugWritePrototype();
            //protoStack.Push(this); //after writing the proto, push it.
        }

        /// <summary>
        /// Writes the prototype to the file indicated in the output manager.
        /// </summary>
        /// <returns></returns>
        public void WritePrototype()
        {

        }

        /// <summary>
        /// For debug purposes, write it to stdout.
        /// </summary>
        public void DebugWritePrototype()
        {
            Console.Out.WriteLine(protoName);
            Console.Out.WriteLine(PrintHeader());

            #region bytecode instructions
            OpCodes code;
            for (int i = 0; i < instructionCount; i++)
            {
                if (i % 4 == 0)
                {
                    code = Opcode.ParseOpByte(instructionBytes[i]);
                    Console.Out.Write("(" + code + "): ");
                }
                else
                {
                    Console.Out.Write(BitConverter.ToString(instructionBytes, i, 1));
                    if (i % 4 == 3)
                        Console.Out.WriteLine("; ");
                    else
                        Console.Out.Write(", ");
                }
            }
            #endregion
            Console.Out.WriteLine(ParseConstants(constantBytes));
            Console.Out.WriteLine("=====END INSTRUCTIONS FOR PROTOTYPE=====");
            Console.Out.WriteLine();
        }

        /// <summary>
        /// Parses the constants section bytes and returns a string containing the entire (formatted)? constants section.
        /// </summary>
        /// <param name="cons"></param>
        /// <returns></returns>
        private string ParseConstants(byte[] cons)
        {
            int consOffset = 0;
            StringBuilder result = new StringBuilder();
            if (cons.Length == 0)
                return "No Constants;\n";
            else
                result.Append("---Constants Section---\n");
            //upvalues first, then global constants, then numbers. Lastly, any debug info

            //read upvalues
            if (sizeUV != 0)
            {
                upvalues = Disassembler.ConsumeBytes(cons, ref consOffset, sizeUV * 2);
                result.Append(ReadUpvalues(upvalues));
            }
            else
                result.Append("No upvalues;\n");

            //read KGC constants
            if (sizeKGC != 0)
            {
                int kgc = sizeKGC;
                result.Append("KGC Section: ");
                while (kgc != 0)
                {
                    result.Append(ReadKGC(cons, ref consOffset));
                    kgc--;
                    if (kgc == 0)
                        result.Append("\n");
                }
            }
            else
                result.Append("No KGC constants;\n");

            //read number constants
            if (sizeKN != 0)
            {
                int kn = sizeKN;
                result.Append("Number Section: ");
                while (kn != 0)
                {
                    result.Append(ReadKN(cons, ref consOffset));
                    kn--;
                    if (kn == 0)
                        result.Append(";");
                    else
                        result.Append(", ");
                }
            }
            else
                result.Append("No number constants;\n");
            return result.ToString();
        }

        private string ReadUpvalues(byte[] upvalues)
        {
            StringBuilder result = new StringBuilder();
            result.Append("Upvalues: ");
            for (int i = 0; i < upvalues.Length; i++)
            {
                result.Append(upvalues[i]);
                if (i + 1 == upvalues.Length)
                    result.Append(";\n");
                else
                    result.Append(", ");
            }
            return result.ToString();
        }

        private int ReadKN(byte[] cons, ref int consOffset)
        {
            return Disassembler.ConsumeByte(cons, ref consOffset) / 2;
        }

        //DiLemming discussion and bcwrite format of KGC constants:
        //type == 1 -> table
        //type == 2 -> int64
        //type == 3 -> uint64
        //type == 4 -> a complex number
        //type >= 5 -> a string of length = type - 5
        private string ReadKGC(byte[] cons, ref int consOffset)
        {
            byte typeByte;
            string typeName;
            StringBuilder result = new StringBuilder();
            typeByte = Disassembler.ConsumeByte(cons, ref consOffset);
            switch (typeByte)
            {
                case 0:
                    typeName = "KGC_CHILD";
                    child = protoStack.Pop();
                    result.Append(typeName + ": " + child.protoName + "; ");
                    break;
                case 1:
                    int arrayPartLength = Disassembler.ConsumeUleb(cons, ref consOffset);
                    int hashPartLength = Disassembler.ConsumeUleb(cons, ref consOffset);
                    if(arrayPartLength != 0)
                    {
                        result.Append(ReadTable(cons, ref consOffset, arrayPartLength, false, tableIndex).ToString());
                    }
                    if(hashPartLength != 0)
                    {
                        result.Append(ReadTable(cons, ref consOffset, hashPartLength, true, tableIndex).ToString());
                    }
                    tableIndex++;
                    break;
                case 2:
                    typeName = "Int64";
                    result.Append(typeName + ": " + Disassembler.ConsumeUleb(cons, ref consOffset) + "; ");
                    //rlt.Append(typeName + ": "+ "number" + "; ");
                    break;
                case 3:
                    typeName = "UInt64";
                    result.Append(typeName + ": " + Disassembler.ConsumeUleb(cons, ref consOffset) + "; ");
                    break;
                case 4:
                    typeName = "Complex Number";
                    result.Append(typeName + ": " + Disassembler.ConsumeUleb(cons, ref consOffset) + "; ");
                    break;
                default:
                    typeName = "String";
                    result.Append(typeName + ": " + ASCIIEncoding.Default.GetString(Disassembler.ConsumeBytes(cons, ref consOffset, typeByte - 5)) + "; ");
                    break;
            }
            return result.ToString();
        }
    //else if (tp == BCDUMP_KTAB_NUM) Note: num is two ulebs.
    //o->u32.lo = bcread_uleb128(ls);
    //o->u32.hi = bcread_uleb128(ls);
    private string ReadTable(byte[] cons, ref int consOffset, int length, bool isHash, int nameIndex)
        {
            int unknownLeb = Disassembler.ConsumeUleb(cons, ref consOffset);
            TableConstant tc = new TableConstant(nameIndex, isHash); //next byte index of table itself??
            for (int i = 0; i < length - 1; i++)
            {
                int t = Disassembler.ConsumeUleb(cons, ref consOffset);
                switch(t)
                {
                    case 0:
                        tc.AddKeyValue(TabType._nil, i, Disassembler.ConsumeUleb(cons, ref consOffset));
                        break;
                    case 1:
                        tc.AddKeyValue(TabType._false, i, Disassembler.ConsumeUleb(cons, ref consOffset));
                        break;
                    case 2:
                        tc.AddKeyValue(TabType._true, i, Disassembler.ConsumeUleb(cons, ref consOffset));
                        break;
                    case 3:
                        tc.AddKeyValue(TabType._int, i, Disassembler.ConsumeUleb(cons, ref consOffset));
                        break;
                    case 4:
                        tc.AddKeyValue(TabType._number, i, new LuaNumber(Disassembler.ConsumeUleb(cons, ref consOffset), Disassembler.ConsumeUleb(cons, ref consOffset)));
                        break;
                    default:
                        tc.AddKeyValue(TabType._string, i, ASCIIEncoding.Default.GetString(Disassembler.ConsumeBytes(cons, ref consOffset, t - 5)));
                        break;
                }
            }
            return tc.ToString();
        }

        private string PrintHeader()
        {
            return "Flags: " + flags + "; " + "# Params: " + numberOfParams + "; " + "Frame Size: " + frameSize + "; " +
                "Upvalue Size: " + sizeUV + "; " + "KGC Size: " + sizeKGC + "; " + "KN Size: " + sizeKN + "; " + "Instruction Count: " + instructionCount + ";\n";
        }
    }
}
