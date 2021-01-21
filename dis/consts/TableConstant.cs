using System.Collections.Generic;
using System.Text;

namespace Luajit_Decompiler.dis.consts
{
    /// <summary>
    /// The purpose of this class is to store whatever values are found inside lua tables and their respective indice.
    /// </summary>
    class TableConstant
    {
        public int arrayPartLength;
        public int hashPartLength;
        public List<BaseConstant> arrayPart;
        public List<BaseConstant> hashPart;

        public TableConstant(ByteManager bm)
        {
            arrayPart = new List<BaseConstant>();
            hashPart = new List<BaseConstant>();
            ReadTable(bm);
        }

        /// <summary>
        /// Tables are comprised of 4 parts; The length of array part, then the length of the hash part, then the array part, then the hash part. hash = key, array = value.
        /// </summary>
        /// <param name="bm"></param>
        public void ReadTable(ByteManager bm)
        {
            arrayPartLength = bm.ConsumeUleb();
            hashPartLength = bm.ConsumeUleb();

            ReadArrayPart(bm, arrayPartLength);
            ReadHashPart(bm, hashPartLength);
        }

        private void ReadArrayPart(ByteManager bm, int arrayPartLength)
        {
            if (arrayPartLength != 0)
                for (int i = 0; i < arrayPartLength; i++)
                    arrayPart.Add(ReadTableValue(bm));
        }

        private void ReadHashPart(ByteManager bm, int hashPartLength)
        {
            if (hashPartLength != 0)
                for (int i = 0; i < hashPartLength; i++)
                    hashPart.Add(new CHash(new HashValue(ReadTableValue(bm), ReadTableValue(bm))));
        }

        /// <summary>
        /// Reads a single key or value from the table array or hash part.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private BaseConstant ReadTableValue(ByteManager bm)
        {
            int typebyte = bm.ConsumeUleb();
            switch (typebyte)
            {
                case 0: //nil
                    return new CNil(); //value = "nil"
                case 1: //false
                    return new CBool(false);
                case 2: //true
                    return new CBool(true);
                case 3: //int
                    return new CInt(bm.ConsumeUleb());
                case 4: //lua number
                    return new CLuaNumber(new LuaNumber(bm.ConsumeUleb(), bm.ConsumeUleb()));
                default: //string
                    return new CString(ASCIIEncoding.Default.GetString(bm.ConsumeBytes(typebyte - 5)));
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