using System;
using System.Collections.Generic;
using System.Text;

namespace Luajit_Decompiler.dis
{
    /// <summary>
    /// Disassembles given bytes.
    /// </summary>
    class Disassembler
    {
        private readonly byte[] magic = new byte[4];
        private readonly byte[] expectedMagic = { 0x1B, 0x4C, 0x4A, 0x01 };
        private byte debugInfoFlags;

        //unclassified bytes. In case junk exists before the magic numbers.
        private List<byte> unclassified = new List<byte>();

        private FileManager fileManager;
        public Dictionary<string, List<Prototype>> DisFile { get; }
        public List<Prototype> fileProtos;
        public Stack<Prototype> protoStack;
        public ByteManager bm;

        public Disassembler(FileManager fileManager)
        {
            this.fileManager = fileManager;
            DisFile = new Dictionary<string, List<Prototype>>();
            fileProtos = new List<Prototype>();
            protoStack = new Stack<Prototype>();
        }

        /// <summary>
        /// Begins the disassembling procedure for all prototypes in the given bytes.
        /// </summary>
        private void Disassemble(string fileName, byte[] bytecode)
        {
            bm = new ByteManager(bytecode);
            Stack<Prototype> protoStack = new Stack<Prototype>();
            TrimHeader(bm);
            if (bytecode == null || bytecode.Length == 0)
                throw new Exception("Disassembler : Disassemble :: Byte array is null or length of zero.");
            for (int i = 0; i < magic.Length; i++)
                if (magic[i] != expectedMagic[i])
                    throw new Exception("Disassembler : Disassemble ::  Magic numbers are invalid for luajit bytes. Expected: 0x1B, 0x4C, 0x4A, 0x01");
            WriteAllPrototypes(fileName, bm);
        }

        /// <summary>
        /// Consumes and stores the file header. Offset is adjusted in consume bytes by reference.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        public void TrimHeader(ByteManager bm)
        {
            ReadUnclassifiedBytes(bm);

            for (int i = 0; i < magic.Length; i++)
                magic[i] = bm.ConsumeByte();

            debugInfoFlags = bm.ConsumeByte();
            if (debugInfoFlags == 0) //0x00 flags => no debug info stripped which means we read the name of the file. { length in uleb, read # of bytes of file name }
            {
                int length = bm.ConsumeUleb();
                byte[] dbgFileName = bm.ConsumeBytes(length);
                Console.Out.WriteLine(ASCIIEncoding.Default.GetString(dbgFileName));
            }
        }

        private void ReadUnclassifiedBytes(ByteManager bm)
        {
            for (int i = 0; i < bm.FileBytes[bm.Offset]; i++)
            {
                try
                {
                    if (bm.FileBytes[bm.Offset] == expectedMagic[0])
                        break;
                    unclassified.Add(bm.ConsumeByte());
                }
                catch (IndexOutOfRangeException ioe)
                {
                    Console.Out.WriteLine("Magic bytes not found during unclassified byte scan. -> " + ioe.Message);
                }
            }
        }

        public void DisassembleAll()
        {
            Dictionary<string, byte[]> files = fileManager.GetAllCompiledLuajitBytes();
            foreach (KeyValuePair<string, byte[]> kv in files)
                Disassemble(kv.Key, kv.Value);
        }

        private void WriteAllPrototypes(string fileName, ByteManager bm)
        {
            int protoSize = bm.ConsumeUleb();
            int protoIndex = 0;
            StringBuilder disassembledFile = new StringBuilder("File Name: " + fileName + "\n\n");
            while (protoSize > 0)
            {
                Prototype pro = new Prototype(bm, protoSize, protoStack, debugInfoFlags, protoIndex);
                fileProtos.Add(pro);
                protoStack.Push(pro);
                protoSize = bm.ConsumeUleb();
                disassembledFile.AppendLine("Prototype: " + protoIndex);
                disassembledFile.Append(pro.ToString());
                protoIndex++;
            }
            DisFile.Add(fileName, fileProtos);
            fileManager.WriteDisassembledBytecode(fileName, disassembledFile.ToString());
        }
    }
}
