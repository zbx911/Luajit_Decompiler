using System.Runtime.InteropServices;

namespace Luajit_Decompiler.dis.consts
{
    class CDouble : BaseConstant
    {
        public CDouble(double value)
        {
            this.value = value;
        }

        public CDouble(KNUnion value)
        {
            this.value = value.knumVal;
        }

        public override string ToString()
        {
            return "Double{ " + value + " };";
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct KNUnion
        {
            [FieldOffset(0)]
            public double knumVal;
            [FieldOffset(0)]
            public int ulebA;
            [FieldOffset(4)] //offset is in bytes.
            public int ulebB;
        }
    }
}
