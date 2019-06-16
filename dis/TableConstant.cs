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
        //contains their index in the table and their respective values.
        public Queue<KeyValuePair<int, int>> nils; //1 if there is a nil.
        public Queue<KeyValuePair<int, int>> falses; //1 if there is a false.
        public Queue<KeyValuePair<int, int>> trues; //1 if there is a true value.
        public Queue<KeyValuePair<int, int>> integers; //index and value
        public Queue<KeyValuePair<int, LuaNumber>> numbers; //index and value
        public Queue<KeyValuePair<int, string>> strings; //index and value
        public int count = 0; //number of table items.
        private Queue<TabType> order; //the order/index of which types are stored.
        public int tableIndex; //index of the table for naming.
        public bool isHash;

        /// <summary>
        /// Requires the index of the table in the bytecode and if it is the hash part or not. Use the same index if it is the hash part of the array. Note: some queues can be null. (To reduce spatial complexity).
        /// </summary>
        /// <param name="tableIndex"></param>
        /// <param name="isHash"></param>
        public TableConstant(int tableIndex, bool isHash)
        {
            this.tableIndex = tableIndex;
            this.isHash = isHash;
            order = new Queue<TabType>();
        }

        public void AddKeyValue(TabType type, int index, int value)
        {
            switch(type)
            {
                case TabType._nil:
                    if (nils == null)
                        nils = new Queue<KeyValuePair<int, int>>();
                    nils.Enqueue(new KeyValuePair<int, int>(index, value));
                    count++;
                    break;
                case TabType._false:
                    if (falses == null)
                        falses = new Queue<KeyValuePair<int, int>>();
                    falses.Enqueue(new KeyValuePair<int, int>(index, value));
                    count++;
                    break;
                case TabType._true:
                    if (trues == null)
                        trues = new Queue<KeyValuePair<int, int>>();
                    trues.Enqueue(new KeyValuePair<int, int>(index, value));
                    count++;
                    break;
                default:
                    if (integers == null)
                        integers = new Queue<KeyValuePair<int, int>>();
                    integers.Enqueue(new KeyValuePair<int, int>(index, value));
                    count++;
                    break;
            }
            order.Enqueue(type);
        }

        public void AddKeyValue(TabType type, int index, LuaNumber value)
        {
            if (numbers == null)
                numbers = new Queue<KeyValuePair<int, LuaNumber>>();
            if (type == TabType._number)
            {
                numbers.Enqueue(new KeyValuePair<int, LuaNumber>(index, value));
                count++;
            }
            else throw new Exception("TableConstant: AddToDict:: Bad Type.");
            order.Enqueue(type);
        }

        public void AddKeyValue(TabType type, int index, string value)
        {
            if (strings == null)
                strings = new Queue<KeyValuePair<int, string>>();
            if (type == TabType._string)
            {
                strings.Enqueue(new KeyValuePair<int, string>(index, value));
                count++;
            }
            else throw new Exception("TableConstant: AddToDict:: Bad Type.");
            order.Enqueue(type);
        }

        /// <summary>
        /// Returns a string representation of the key/values of the lua table.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder result = new StringBuilder("\nTable " + tableIndex + " Contents: [index, value];\n");
            if (isHash)
                result.Append("\tHash Part:\n\t\t");
            else
                result.Append("\tArray Part:\n\t\t");
            for(int i = 0; i < count; i++)
            {
                TabType t = order.Dequeue();
                string value;
                switch(t)
                {
                    case TabType._nil:
                        value = " [" + nils.Dequeue().Key + "," + " nil]";
                        break;
                    case TabType._false:
                        value = " [" + falses.Dequeue().Key + "," + " false]";
                        break;
                    case TabType._true:
                        value = " [" + trues.Dequeue().Key + "," + " true]";
                        break;
                    case TabType._int:
                        value = " " + integers.Dequeue().ToString();
                        break;
                    case TabType._number:
                        value = " " + numbers.Dequeue().ToString();
                        break;
                    default:
                        value = " " + strings.Dequeue().ToString();
                        break;

                }
                result.Append(value);
                if (i < count)
                    result.Append("; ");
                else
                    result.Append(" : ");
            }
            return result.ToString();
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