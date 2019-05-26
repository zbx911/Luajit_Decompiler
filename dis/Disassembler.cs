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

        //header
        //private int headerLength = 5;
        private byte[] magic = new byte[4];
        private byte[] expectedMagic = { 0x1B, 0x4C, 0x4A, 0x01 };
        private byte flags; //mainly for determining if the debug info has been stripped or not.

        public Disassembler(string outputFilePath, byte[] bytecode)
        {
            path = outputFilePath;
            manager = new OutputManager(path);
            this.bytecode = TrimHeader(bytecode);
        }

        /// <summary>
        /// Consumes the file header and returns the remaining bytes.
        /// </summary>
        /// <param name="bytes">The complete bytes from a file.</param>
        /// <returns>bytes with the header trimmed.</returns>
        public byte[] TrimHeader(byte[] bytes)
        {
            for (int i = 0; i < magic.Length; i++)
                magic[i] = bytes[i];
            flags = bytes[5];
            return bytes.Skip(5).Take(bytes.Length - 5).ToArray();
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
            List<Prototype> prototypes = GetAllPrototypes(bytecode);
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
        private List<Prototype> GetAllPrototypes(byte[] bytes)
        {
            //Error handling is handled by the Disassemble method where it is called.
            List<Prototype> prototypes = new List<Prototype>();
            Tuple<byte[], byte> singleByte = ConsumeByte(bytes);
            Tuple<byte[], byte[]> arrayBytes;
            bytes = singleByte.Item1;
            byte protoSize = singleByte.Item2; //consume first byte
            while (protoSize > 0)
            {
                arrayBytes = ConsumeBytes(bytes, protoSize);
                bytes = arrayBytes.Item1;
                Prototype pro = new Prototype(arrayBytes.Item2, manager);
                singleByte = ConsumeByte(bytes);
                bytes = singleByte.Item1;
                protoSize = singleByte.Item2;
                prototypes.Add(pro);
            }
            return prototypes;
        }

        /// <summary>
        /// Returns a tuple with the first item being the remaining bytes and the second item being the consumed byte.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static Tuple<byte[], byte> ConsumeByte(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                throw new Exception("Disassembler : ConsumeByte :: Byte array is null or length of zero.");
            byte consumed = bytes[0];
            return new Tuple<byte[], byte>(bytes.Skip(1).Take(bytes.Length - 1).ToArray(), consumed);
        }

        /// <summary>
        /// Returns a tuple with the first item being the remaining bytes and the second item being the array of consumed bytes.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static Tuple<byte[], byte[]> ConsumeBytes(byte[] bytes, int length)
        {
            if (length > bytes.Length)
                throw new Exception("Disassembler : ConsumeBytes :: Given length exceeds the length of the bytes array.");
            if (length == 0)
                return null;
            if (bytes == null || bytes.Length == 0)
                throw new Exception("Disassembler : ConsumeBytes :: Given bytes array is empty or null.");
            byte[] results = new byte[length];
            for (int i = 0; i < length; i++)
            {
                results[i] = bytes[i];
            }
            return new Tuple<byte[], byte[]>(bytes.Skip(length).Take(bytes.Length - length).ToArray(), results);
        }
    }
}
