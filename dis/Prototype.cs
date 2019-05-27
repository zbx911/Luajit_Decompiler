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

        //Prototype Header Info : These 7 bytes are also from luajit lj_bcwrite. Note: uleb128 parsing is required for the sizeUV, sizeKGC, and the bytes[6] instructionCount.
        //(for values > 255/256). Also note that if the flags for whether or not to strip debug info is something other than 0x02, then there will be a name before the rest of instructions.
        public byte flags; //whether or not to strip debug info.
        public byte numberOfParams; //number of params in the method
        public byte frameSize; //??
        public byte sizeUV; //size of upvalues section?
        public byte sizeKGC; //size of the constants section.
        public byte sizeKN; //size of constant numbers???
        public int instructionCount; //number of bytecode instructions for the prototype.
        public byte[] instructionBytes; //instructions section bytes
        public byte[] constantBytes; //constant section bytes

        public Prototype(byte[] bytes, ref int offset, OutputManager manager, int protoSize) //!! ProtoSize parameter is temporary. Remove when constants are handled.
        {
            this.bytes = bytes;
            this.manager = manager;
            int instructionSize = 4; //each instruction is 4 bytes.
            flags = Disassembler.ConsumeByte(bytes, ref offset);
            numberOfParams = Disassembler.ConsumeByte(bytes, ref offset);
            frameSize = Disassembler.ConsumeByte(bytes, ref offset);
            sizeUV = Disassembler.ConsumeByte(bytes, ref offset);
            sizeKGC = Disassembler.ConsumeByte(bytes, ref offset);
            sizeKN = Disassembler.ConsumeByte(bytes, ref offset);
            instructionCount = Disassembler.ConsumeByte(bytes, ref offset) * instructionSize;
            instructionBytes = Disassembler.ConsumeBytes(bytes, ref offset, instructionCount);
            //TODO: Handle constants section. AKA: UV, KGC, KN

            //THESE NEXT LINES ARE COMPLETELY TEMPORARY. CODE ASSUMES CONSTANTS SECTIONS ARE ALREADY HANDLED AND OFFSET IS ADJUSTED ACCORDINGLY.
            //ADJUSTING OFFSET SO THAT ALL PROTOTYPES IN A FILE ARE CAUGHT. REMOVE PROTOSIZE PARAMETER WHEN CONSTANTS ARE HANDLED.
            int byteTally = 7 + instructionCount; //7 from header and all instruction bytes
            offset += protoSize - byteTally; //difference between the size of proto and tally is the constants section byte size.
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
            OpCodes code;
            for (int i = 0; i < instructionCount; i++)
            {
                if (i % 4 == 0)
                {
                    code = Opcode.ParseOpByte(instructionBytes[i]);
                    Console.Out.WriteLine(code);
                }
                else
                {
                    Console.Out.WriteLine(BitConverter.ToString(instructionBytes, i, 1));
                }
            }
            Console.Out.WriteLine("=====END INSTRUCTIONS FOR PROTOTYPE=====");
        }
    }
}
