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
        private byte[] bytes;

        //header
        //private int headerLength = 5;
        private byte[] magic = new byte[4];
        private byte[] expectedMagic = { 0x1B, 0x4C, 0x4A, 0x01 };
        private byte flags; //mainly for stripping the debug info.

        //error tag(s)
        private string errorClassTag = "Disassembler : ";
        private string errorMethodTagTrimHeader = "TrimHeader :: ";
        private string errorMethodTagConsumeBytes = "ConsumeBytes :: ";
        private string errorMethodTagConsumeByte = "ConsumeByte :: ";

        public Disassembler(string outputFilePath, byte[] bytes)
        {
            path = outputFilePath;
            manager = new OutputManager(path);
            this.bytes = TrimHeader(bytes);
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
            if (bytes == null || bytes.Length == 0)
                throw new Exception(errorClassTag + errorMethodTagTrimHeader + "bytes array is null or length of zero.");
            for (int i = 0; i < magic.Length; i++)
                if(magic[i] != expectedMagic[i])
                    throw new Exception(errorClassTag + errorMethodTagTrimHeader + "Magic numbers are invalid for luajit bytes. Expected: 0x1B, 0x4C, 0x4A, 0x01");
            List<Prototype> prototypes = GetAllPrototypes(bytes);
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
            byte protoSize = ConsumeByte(); //consume first byte
            while(protoSize > 0)
            {
                Prototype pro = new Prototype(ConsumeBytes(protoSize), manager);
                protoSize = ConsumeByte();
                prototypes.Add(pro);
            }
            return prototypes;
        }

        /// <summary>
        /// Consumes the byte at **index 0** from the bytes array and returns both the modified bytes array and the byte it read.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="readByte"></param>
        /// <returns></returns>
        private byte ConsumeByte()
        {
            if (bytes == null || bytes.Length == 0)
                throw new Exception(errorClassTag + errorMethodTagConsumeByte + "bytes array is null or length of zero.");
            byte consumed = bytes[0];
            bytes = bytes.Skip(1).Take(bytes.Length - 1).ToArray();
            return consumed;
        }

        /// <summary>
        /// Consumes bytes starting from **index 0** equivalent to the given length. (Ex: length = 3 means 3 bytes will be consumed).
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private byte[] ConsumeBytes(int length)
        {
            if (length > bytes.Length)
                throw new Exception(errorClassTag + errorMethodTagConsumeBytes + "Given length exceeds the length of the bytes array.");
            if (length == 0)
                return null;
            if (bytes == null || bytes.Length == 0)
                throw new Exception(errorClassTag + errorMethodTagConsumeBytes + "Given bytes array is empty or null.");
            byte[] resultSet = new byte[length];
            for(int i = 0; i < length; i++)
                resultSet[i] = ConsumeByte();
            return resultSet;
        }
    }
}
