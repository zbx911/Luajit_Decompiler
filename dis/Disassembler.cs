using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Luajit_Decompiler.dis
{
    /// <summary>
    /// Disassembles given bytes.
    /// </summary>
    class Disassembler
    {
        //header
        private byte[] magic = new byte[4];
        private byte[] expectedMagic = { 0x1B, 0x4C, 0x4A, 0x01 };
        private byte flags; //mainly for determining if the debug info has been stripped or not.

        //unclassified bytes. Unknown bytes were found before luajit header.
        private List<byte> unclassified = new List<byte>();

        private FileManager fileManager;
        public Dictionary<string, List<Prototype>> disFile { get; }

        public Disassembler(FileManager fileManager)
        {
            this.fileManager = fileManager;
            disFile = new Dictionary<string, List<Prototype>>();
        }

        /// <summary>
        /// Consumes and stores the file header. Offset is adjusted in consume bytes by reference.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        public void TrimHeader(byte[] bytes, ref int offset)
        {
            //read unclassified bytes until we hit the luajit magic bytes.
            for(int i = 0; i < bytes.Length; i++)
            {
                try
                {
                    if (bytes[offset] == expectedMagic[0])
                        break;
                    unclassified.Add(bytes[offset]);
                    offset++;
                }
                catch (IndexOutOfRangeException ioe)
                {
                    Console.Out.WriteLine("Magic bytes not found during unclassified byte scan. -> " + ioe.Message);
                }
            }

            for (int i = 0; i < magic.Length; i++)
                magic[i] = ConsumeByte(bytes, ref offset);
            flags = ConsumeByte(bytes, ref offset);
            if(flags == 0) //0x00 flags => no debug info stripped which means we read the name of the file. { length in uleb, read # of bytes of file name }
            {
                int length = ConsumeUleb(bytes, ref offset);
                byte[] dbgFileName = ConsumeBytes(bytes, ref offset, length);
                Console.Out.WriteLine(ASCIIEncoding.Default.GetString(dbgFileName));
            }
        }

        /// <summary>
        /// Begins the disassembling procedure for all prototypes in the given bytes.
        /// </summary>
        private void Disassemble(string fileName, byte[] bytecode)
        {
            int offset = 0;
            Stack<Prototype> protoStack = new Stack<Prototype>();
            TrimHeader(bytecode, ref offset);
            if (bytecode == null || bytecode.Length == 0)
                throw new Exception("Disassembler : Disassemble :: Byte array is null or length of zero.");
            for (int i = 0; i < magic.Length; i++)
                if(magic[i] != expectedMagic[i])
                    throw new Exception("Disassembler : Disassemble ::  Magic numbers are invalid for luajit bytes. Expected: 0x1B, 0x4C, 0x4A, 0x01");
            WriteAllPrototypes(fileName, bytecode, ref offset, protoStack);
        }

        public void DisassembleAll()
        {
            Dictionary<string, byte[]> files = fileManager.GetAllCompiledLuajitBytes();
            foreach (KeyValuePair<string, byte[]> kv in files)
            {
                Disassemble(kv.Key, kv.Value);
            }
        }

        /// <summary>
        /// TODO:
        /// A problem arises here when there are more than 255 bytes in a prototype. Same problem will probably exist if there are over 255 instructions in a prototype as well inside the header.
        /// Requires the Dilemming discussion on uleb to properly parse.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private void WriteAllPrototypes(string fileName, byte[] bytes, ref int offset, Stack<Prototype> protoStack)
        {
            int protoSize = ConsumeUleb(bytes, ref offset);
            int nameNDX = 0; //used for naming prototypes.
            StringBuilder disassembledFile = new StringBuilder("File Name: " + fileName + "\n\n");
            List<Prototype> fileProtos = new List<Prototype>();
            while (protoSize > 0)
            {
                Prototype pro = new Prototype(bytes, ref offset, protoSize, protoStack, flags, nameNDX);
                fileProtos.Add(pro);
                protoStack.Push(pro);
                protoSize = ConsumeUleb(bytes, ref offset);
                disassembledFile.AppendLine("Prototype: " + nameNDX);
                disassembledFile.AppendLine(pro.ToString()); //append for writing to file.
                nameNDX++;
            }
            disFile.Add(fileName, fileProtos);
            fileManager.WriteDisassembledBytecode(fileName, disassembledFile.ToString());
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
