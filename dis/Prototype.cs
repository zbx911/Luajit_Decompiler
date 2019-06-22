﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luajit_Decompiler.dis
{
    /// <summary>
    /// TODO: Handle debug info.
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
        //public byte[] constantBytes; //constant section bytes
        public byte[] upvalues; //every 2 bytes is 1 upvalue reference.
        public Prototype child; //the parent of this prototype. (If a parent exists). (pop from protostack).
        private Stack<Prototype> protoStack; //the stack of all prototypes.
        private int tableIndex = 0; //for naming tables in lua files.
        private int debugSize; //size of the debug info section
        private int firstLine; //size of the first line of debug info?
        private int numLines; //number of debug info lines?

        public Prototype(byte[] bytes, ref int offset, OutputManager manager, int protoSize, Stack<Prototype> protoStack, int nameNDX, byte fileFlag) //fileFlag from the file header. 0x02 = strip debug info.
        {
            this.bytes = bytes;
            this.manager = manager;
            this.protoStack = protoStack;
            protoName += nameNDX;
            int instructionSize = 4; //each instruction is 4 bytes.
            //int headerSize = 7; //7 bytes in each prototype header.

            //prototype header and instructions section
            flags = Disassembler.ConsumeByte(bytes, ref offset); //# of tables for instance?
            numberOfParams = Disassembler.ConsumeByte(bytes, ref offset);
            frameSize = Disassembler.ConsumeByte(bytes, ref offset); //# of functions - 1?
            sizeUV = Disassembler.ConsumeByte(bytes, ref offset);
            sizeKGC = Disassembler.ConsumeUleb(bytes, ref offset);
            sizeKN = Disassembler.ConsumeUleb(bytes, ref offset);
            instructionCount = Disassembler.ConsumeUleb(bytes, ref offset) * instructionSize;
            instructionBytes = Disassembler.ConsumeBytes(bytes, ref offset, instructionCount);
            //From luajit's bcread. read the debug info part of the header if necessary.
            if ((fileFlag & 0x02) == 0)
            {
                debugSize = Disassembler.ConsumeUleb(bytes, ref offset);
                if(debugSize > 0)
                {
                    firstLine = Disassembler.ConsumeUleb(bytes, ref offset);
                    numLines = Disassembler.ConsumeUleb(bytes, ref offset);
                }
            }

            //begin writing this prototype.
            DebugWritePrototype(ref offset);
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
        public void DebugWritePrototype(ref int offset)
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
            //Console.Out.WriteLine(ParseConstants(constantBytes));
            ParseConstants(bytes, ref offset);
            Console.Out.WriteLine("=====END INSTRUCTIONS FOR PROTOTYPE=====");
            Console.Out.WriteLine();
        }

        /// <summary>
        /// Parses the constants section bytes and returns a string containing the entire (formatted)? constants section.
        /// </summary>
        /// <param name="bytes"></param>
        private void ParseConstants(byte[] bytes, ref int offset)
        {
            Console.Out.WriteLine("---Constants Section---");
            //result.Append("---Constants Section---\n");

            //upvalues first, then global constants, then numbers. Lastly, any debug info

            //read upvalues
            if (sizeUV > 0)
            {
                upvalues = Disassembler.ConsumeBytes(bytes, ref offset, sizeUV * 2);
                //result.Append(ReadUpvalues(upvalues));
                Console.Out.WriteLine(ReadUpvalues(upvalues));
            }
            else
                Console.Out.Write("No upvalues;\n");
                //result.Append("No upvalues;\n");

            //read KGC constants
            if (sizeKGC > 0)
            {
                int kgc = sizeKGC;
                Console.Out.Write("KGC Section: ");
                //result.Append("KGC Section: ");
                while (kgc != 0)
                {
                    //result.Append(ReadKGC(cons, ref consOffset));
                    Console.Out.WriteLine(ReadKGC(bytes, ref offset));
                    kgc--;
                    if (kgc == 0)
                        Console.Out.WriteLine();
                        //result.Append("\n");
                }
            }
            else
                Console.Out.WriteLine("No KGC constants;\n");
                //result.Append("No KGC constants;\n");

            //read number constants
            if (sizeKN > 0)
            {
                int kn = sizeKN;
                Console.Out.Write("Number Section: ");
                //result.Append("Number Section: ");
                while (kn != 0)
                {
                    //result.Append(ReadKN(cons, ref consOffset));
                    Console.Out.Write(ReadKN(bytes, ref offset));
                    kn--;
                    if (kn == 0)
                        Console.Out.Write(";");
                    //result.Append(";");
                    else
                        Console.Out.Write(", ");
                        //result.Append(", ");
                }
            }
            else
                Console.Out.WriteLine("No number constants;\n");
                //result.Append("No number constants;\n");

            //read debug info if debug info is present
            if (debugSize > 0)
            {
                //Testing out this method first I suppose. Reading the bytes for the count of debugSize.
                Disassembler.ConsumeBytes(bytes, ref offset, debugSize);
            }
            //return result.ToString();
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

        private int ReadKN(byte[] bytes, ref int offset)
        {
            return Disassembler.ConsumeByte(bytes, ref offset) / 2;
        }

        //DiLemming discussion and bcwrite format of KGC constants:
        //type == 1 -> table
        //type == 2 -> int64
        //type == 3 -> uint64
        //type == 4 -> a complex number
        //type >= 5 -> a string of length = type - 5
        /// <summary>
        /// TODO: Table hash part is not implemented correctly.
        /// </summary>
        /// <param name="cons"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private string ReadKGC(byte[] bytes, ref int offset)
        {
            byte typeByte;
            string typeName;
            StringBuilder result = new StringBuilder();
            typeByte = Disassembler.ConsumeByte(bytes, ref offset);
            switch (typeByte)
            {
                case 0:
                    typeName = "KGC_CHILD";
                    if (protoStack.Count > 0)
                    {
                        child = protoStack.Pop();
                        result.Append(typeName + ": " + child.protoName + "; ");
                    }
                    else
                        result.Append("Unknown Child Prototype.");
                    break;
                case 1:
                    TableConstant tc = new TableConstant(tableIndex);
                    result.Append(tc.ReadTable(bytes, ref offset));
                    tableIndex++;
                    break;
                case 2:
                    typeName = "Int64";
                    result.Append(typeName + ": " + Disassembler.ConsumeUleb(bytes, ref offset) + "; ");
                    break;
                case 3:
                    typeName = "UInt64";
                    result.Append(typeName + ": " + Disassembler.ConsumeUleb(bytes, ref offset) + "; ");
                    break;
                case 4:
                    typeName = "Complex Number";
                    result.Append(typeName + ": " + Disassembler.ConsumeUleb(bytes, ref offset) + "; ");
                    break;
                default:
                    typeName = "String";
                    result.Append(typeName + ": " + ASCIIEncoding.Default.GetString(Disassembler.ConsumeBytes(bytes, ref offset, typeByte - 5)) + "; ");
                    break;
            }
            return result.ToString();
        }
    //else if (tp == BCDUMP_KTAB_NUM) Note: num is two ulebs.
    //o->u32.lo = bcread_uleb128(ls);
    //o->u32.hi = bcread_uleb128(ls);
    //private string ReadTable(byte[] cons, ref int consOffset, int length, bool isHash, int nameIndex)
    //    {
    //        //This byte represents the zero index of a lua table. DiLemming pointed out that since lua is a 1 based indexing language, but luajit users are used to 0 based indexing, this extra byte is always here.
    //        //by purposefully setting the 0 index of a lua table by: table6 = {[0] = "zero", "one", "two"}
    //        //the resulting bytecode will break the program.
    //        //int zeroIndexOfTable = Disassembler.ConsumeByte(cons, ref consOffset);
    //        TableConstant tc = new TableConstant(nameIndex, isHash);
    //        for (int i = 0; i < length - 1; i++)
    //        {
    //            int t = Disassembler.ConsumeUleb(cons, ref consOffset);
    //            switch(t)
    //            {
    //                case 0:
    //                    tc.AddKeyValue(TabType._nil, i, 0);
    //                    break;
    //                case 1:
    //                    tc.AddKeyValue(TabType._false, i, Disassembler.ConsumeUleb(cons, ref consOffset));
    //                    break;
    //                case 2:
    //                    tc.AddKeyValue(TabType._true, i, Disassembler.ConsumeUleb(cons, ref consOffset));
    //                    break;
    //                case 3:
    //                    tc.AddKeyValue(TabType._int, i, Disassembler.ConsumeUleb(cons, ref consOffset));
    //                    break;
    //                case 4:
    //                    tc.AddKeyValue(TabType._number, i, new LuaNumber(Disassembler.ConsumeUleb(cons, ref consOffset), Disassembler.ConsumeUleb(cons, ref consOffset)));
    //                    break;
    //                default:
    //                    tc.AddKeyValue(TabType._string, i, ASCIIEncoding.Default.GetString(Disassembler.ConsumeBytes(cons, ref consOffset, t - 5)));
    //                    break;
    //            }
    //        }
    //        return tc.ToString();
    //    }

        private string PrintHeader()
        {
            return "Flags: " + flags + "; " + "# Params: " + numberOfParams + "; " + "Frame Size: " + frameSize + "; " +
                "Upvalue Size: " + sizeUV + "; " + "KGC Size: " + sizeKGC + "; " + "KN Size: " + sizeKN + "; " + "Instruction Count: " + instructionCount + ";\n";
        }
    }
}
