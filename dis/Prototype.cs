using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luajit_Decompiler.dis
{
    /// <summary>
    /// TODO:
    /// Return remaining bytes.
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

        public byte[] instructionBytes;

        public Prototype(byte[] bytes, OutputManager manager)
        {
            this.bytes = bytes;
            this.manager = manager;
            int instructionSize = 4; //each instruction is 4 bytes.

            Tuple<byte[], byte> singleByte = Disassembler.ConsumeByte(bytes);
            bytes = singleByte.Item1;
            flags = singleByte.Item2;

            singleByte = Disassembler.ConsumeByte(bytes);
            bytes = singleByte.Item1;
            numberOfParams = singleByte.Item2;

            singleByte = Disassembler.ConsumeByte(bytes);
            bytes = singleByte.Item1;
            frameSize = singleByte.Item2;

            singleByte = Disassembler.ConsumeByte(bytes);
            bytes = singleByte.Item1;
            sizeUV = singleByte.Item2;

            singleByte = Disassembler.ConsumeByte(bytes);
            bytes = singleByte.Item1;
            sizeKGC = singleByte.Item2;

            singleByte = Disassembler.ConsumeByte(bytes);
            bytes = singleByte.Item1;
            sizeKN = singleByte.Item2;

            singleByte = Disassembler.ConsumeByte(bytes);
            bytes = singleByte.Item1;
            instructionCount = singleByte.Item2 * instructionSize;

            Tuple<byte[], byte[]> manyBytes = Disassembler.ConsumeBytes(bytes, instructionCount);
            bytes = manyBytes.Item1;
            instructionBytes = manyBytes.Item2;
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
