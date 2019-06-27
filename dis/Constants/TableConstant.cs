using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luajit_Decompiler.dis.Constants;

namespace Luajit_Decompiler.dis
{
    /// <summary>
    /// Based off of luajit's enumeration for reading tables.
    /// </summary>
    public enum TabType
    {
        _nil,
        _false,
        _true,
        _int,
        _number,
        _string
    };

    /// <summary>
    /// The purpose of this class is to store whatever values are found inside lua tables and their respective indice.
    /// </summary>
    class TableConstant
    {
        public int arrayPartLength;
        public int hashPartLength;
        public List<BaseConstant> arrayPart;
        public List<BaseConstant> hashPart;

        public TableConstant(byte[] bytes, ref int offset)
        {
            arrayPart = new List<BaseConstant>();
            hashPart = new List<BaseConstant>();
            ReadTable(bytes, ref offset);
        }

        public void ReadTable(byte[] bytes, ref int offset)
        {
            arrayPartLength = Disassembler.ConsumeUleb(bytes, ref offset);
            hashPartLength = Disassembler.ConsumeUleb(bytes, ref offset);
            //read the array part
            if (arrayPartLength != 0)
                for (int i = 0; i < arrayPartLength; i++)
                    arrayPart.Add(ReadTableValue(bytes, ref offset));
            //read the hash part
            if (hashPartLength != 0)
                for(int i = 0; i < hashPartLength; i++)
                    hashPart.Add(new CHash(new HashValue(ReadTableValue(bytes, ref offset), ReadTableValue(bytes, ref offset))));
        }

        /// <summary>
        /// Reads a single key or value from the table array or hash part.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private BaseConstant ReadTableValue(byte[] bytes, ref int offset)
        {
            int typebyte = Disassembler.ConsumeUleb(bytes, ref offset);
            switch (typebyte)
            {
                case 0: //nil
                    return new CNil(); //value = "nil"
                case 1: //false
                    return new CBool(false);
                case 2: //true
                    return new CBool(true);
                case 3: //int
                    return new CInt(Disassembler.ConsumeUleb(bytes, ref offset));
                case 4: //lua number
                    return new CLuaNumber(new LuaNumber(Disassembler.ConsumeUleb(bytes, ref offset), Disassembler.ConsumeUleb(bytes, ref offset)));
                default: //string
                    return new CString(ASCIIEncoding.Default.GetString(Disassembler.ConsumeBytes(bytes, ref offset, typebyte - 5)));
            }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine("Table{ ");

            result.AppendLine("\tArray Part{ ");
            for (int i = 0; i < arrayPart.Count; i++)
                result.AppendLine("\t\t" + arrayPart[i].ToString());
            result.AppendLine("\t};");

            result.AppendLine("\tHash Part{ ");
            for (int i = 0; i < hashPart.Count; i++)
                result.AppendLine("\t\t" + hashPart[i].ToString());
            result.AppendLine("\t};");
            result.AppendLine("};");
            return result.ToString();
        }
    }

}