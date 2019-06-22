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

        private Stack<Prototype> protoStack;

        public Disassembler(string outputFilePath, byte[] bytecode)
        {
            path = outputFilePath;
            manager = new OutputManager(path);
            this.bytecode = bytecode;
            TrimHeader(bytecode, ref offset);
            protoStack = new Stack<Prototype>();
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
            WriteAllPrototypes(bytecode, ref offset);
        }

        /// <summary>
        /// TODO:
        /// A problem arises here when there are more than 255 bytes in a prototype. Same problem will probably exist if there are over 255 instructions in a prototype as well inside the header.
        /// Requires the Dilemming discussion on uleb to properly parse.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private void WriteAllPrototypes(byte[] bytes, ref int offset)
        {
            int protoSize = ConsumeUleb(bytes, ref offset);
            int nameNDX = 0; //temp.
            while (protoSize > 0)
            {
                Prototype pro = new Prototype(bytes, ref offset, manager, protoSize, protoStack, nameNDX, flags); //writes in the constructor. temporary parameter for nameNDX. Remove when names implemented.
                protoStack.Push(pro);
                protoSize = ConsumeUleb(bytes, ref offset);
                nameNDX++;
            }
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

        // Reference of DiLemming's code:
        //int uleb(byte[] a) {
        //    int value = 0;
        //    int shift = 1;
        //    for(int i = 0; i < a.length; i++)
        //    {
        //        byte b = a[i++];
        //        byte data = b & 127;
        //        byte cont = b & 128;
        //        value += data * shift
        //        shift *= 128;
        //        if(cont == 0) break;
        //    }
        //    return value;
        //}
        /// <summary>
        /// Consumes bytes that can be encoded in leb128. Returns their integer equivalent. Modified version of DiLemming's code.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static int ConsumeUleb(byte[] bytes, ref int offset)
        {
            int count = 0;
            int shift = 1;
            int cont = 0;
            byte b;
            int data;
            int value = 0;
            do
            {
                b = bytes[offset + count];
                data = b & 127;
                cont = b & 128;
                value += data * shift;
                shift *= 128;
                count++;
            } while (cont != 0);
            offset += count;
            return value;
        }
    }
}
