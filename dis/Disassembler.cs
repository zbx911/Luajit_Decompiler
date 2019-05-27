using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luajit_Decompiler.dis
{
    /// <summary>
    /// Disassembles given bytes.
    /// </summary>
    class Disassembler
    {
        private string path;
        private OutputManager manager;
        private byte[] bytecode;
        private int offset = 0;

        //header
        private byte[] magic = new byte[4];
        private byte[] expectedMagic = { 0x1B, 0x4C, 0x4A, 0x01 };
        private byte flags; //mainly for determining if the debug info has been stripped or not.

        public Disassembler(string outputFilePath, byte[] bytecode)
        {
            path = outputFilePath;
            manager = new OutputManager(path);
            this.bytecode = bytecode;
            TrimHeader(bytecode, ref offset);
        }

        /// <summary>
        /// Consumes and stores the file header. Offset is adjusted in consume bytes by reference.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        public void TrimHeader(byte[] bytes, ref int offset)
        {
            for (int i = 0; i < magic.Length; i++)
                magic[i] = ConsumeByte(bytes, ref offset);
            flags = ConsumeByte(bytes, ref offset);
        }

        /// <summary>
        /// Begins the disassembling procedure for all prototypes in the given bytes.
        /// </summary>
        public void Disassemble()
        {
            if (bytecode == null || bytecode.Length == 0)
                throw new Exception("Disassembler : Disassemble :: Byte array is null or length of zero.");
            for (int i = 0; i < magic.Length; i++)
                if(magic[i] != expectedMagic[i])
                    throw new Exception("Disassembler : Disassemble ::  Magic numbers are invalid for luajit bytes. Expected: 0x1B, 0x4C, 0x4A, 0x01");
            List<Prototype> prototypes = GetAllPrototypes(bytecode, ref offset);
            foreach (Prototype p in prototypes)
            {
                p.DebugWritePrototype(); //comment this out when output management is implemented.
                //p.WritePrototype(); //The method that will write to the file defined in output management.
            }
        }

        /// <summary>
        /// TODO:
        /// A problem arises here when there are more than 255 bytes in a prototype. Same problem will probably exist if there are over 255 instructions in a prototype as well inside the header.
        /// Requires the Dilemming discussion on uleb to properly parse.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private List<Prototype> GetAllPrototypes(byte[] bytes, ref int offset)
        {
            List<Prototype> prototypes = new List<Prototype>();
            byte protoSize = ConsumeByte(bytes, ref offset);
            while (protoSize > 0)
            {
                Prototype pro = new Prototype(bytes, ref offset, manager, protoSize);
                prototypes.Add(pro);
                protoSize = ConsumeByte(bytes, ref offset);
            }
            return prototypes;
        }

        /// <summary>
        /// Returns a byte from the given byte array and increments the offset by 1.
        /// </summary>
        /// <param name="bytes">Array of all bytecode.</param>
        /// <param name="offset">Current offset in the bytecode array.</param>
        /// <returns></returns>
        public static byte ConsumeByte(byte[] bytes, ref int offset)
        {
            byte result = bytes[offset];
            offset++;
            return result;
        }

        /// <summary>
        /// Returns a byte array of consumed bytes from the given array and increments the offset accordingly.
        /// </summary>
        /// <param name="bytes">Array of all bytecode.</param>
        /// <param name="offset">Current offset in the bytecode array.</param>
        /// <param name="length">The number of bytes to be read and returned.</param>
        /// <returns></returns>
        public static byte[] ConsumeBytes(byte[] bytes, ref int offset, int length)
        {
            byte[] result = new byte[length];
            for (int i = 0; i < length; i++)
                result[i] = ConsumeByte(bytes, ref offset);
            return result;
        }
    }
}
