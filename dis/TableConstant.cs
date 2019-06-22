using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luajit_Decompiler.dis
{
    /// <summary>
    /// Based off of luajit's enumeration for reading tables.
    /// </summary>
    enum TabType
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
        private string tableName = "Table ";

        public TableConstant(int tableIndex)
        {
            tableName += tableIndex;
            tableName += ":\n";
        }
        /// <summary>
        /// Returns a string containing formatted text of the table's array part and hash part plus their respective values.
        /// </summary>
        /// <param name="cons"></param>
        /// <param name="consOffset"></param>
        /// <returns></returns>
        public string ReadTable(byte[] cons, ref int consOffset)
        {
            int arrayPartLength = Disassembler.ConsumeUleb(cons, ref consOffset);
            int hashPartLength = Disassembler.ConsumeUleb(cons, ref consOffset);
            StringBuilder result = new StringBuilder(tableName);
            //read the array part
            if (arrayPartLength != 0)
            {
                result.Append("\tArray Part:\n\t\t");
                for(int i = 0; i < arrayPartLength; i++)
                {
                    result.Append("[");
                    result.Append(i + ", ");
                    result.Append(ReadTableValue(cons, ref consOffset));
                    result.Append("]; ");
                }
            }
            //read the hash part
            if (hashPartLength != 0)
            {
                result.Append("\tHash Part:\n\t\t");
                for(int i = 0; i < hashPartLength; i++)
                {
                    result.Append("[");
                    result.Append(ReadTableValue(cons, ref consOffset) + ", ");
                    result.Append(ReadTableValue(cons, ref consOffset));
                    result.Append("]; ");
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// Reads a single key or value from the table array or hash part.
        /// </summary>
        /// <param name="cons"></param>
        /// <param name="consOffset"></param>
        /// <returns></returns>
        private string ReadTableValue(byte[] cons, ref int consOffset)
        {
            int typebyte = Disassembler.ConsumeUleb(cons, ref consOffset);
            switch (typebyte)
            {
                case 0: //nil
                    return "nil";
                case 1: //false
                    return "false";
                case 2: //true
                    return "true";
                case 3: //int
                    int value = Disassembler.ConsumeUleb(cons, ref consOffset);
                    return value.ToString();
                case 4: //lua number
                    return new LuaNumber(Disassembler.ConsumeUleb(cons, ref consOffset), Disassembler.ConsumeUleb(cons, ref consOffset)).ToString();
                default: //string
                    return ASCIIEncoding.Default.GetString(Disassembler.ConsumeBytes(cons, ref consOffset, typebyte - 5));
            }
        }
    }
    class LuaNumber
    {
        public int low { get; }
        public int high { get; }
        public LuaNumber(int low, int high)
        {
            this.low = low;
            this.high = high;
        }
        public override string ToString()
        {
            return "LuaNum:=|" + low + ", "+ high +"| ";
        }
    }
}