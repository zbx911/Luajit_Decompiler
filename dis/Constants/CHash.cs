using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luajit_Decompiler.dis.Constants
{
    /// <summary>
    /// Storage for hash map key/value pairings.
    /// </summary>
    class HashValue
    {
        BaseConstant key;
        BaseConstant value;

        public HashValue(BaseConstant key, BaseConstant value)
        {
            this.key = key;
            this.value = value;
        }

        /// <summary>
        /// Returns key/value pairing in format: key:value
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Hash{ " + key + " -> " + value + " };";
        }
    }

    class CHash : BaseConstant
    {
        public CHash(HashValue value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }
}
