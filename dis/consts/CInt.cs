namespace Luajit_Decompiler.dis.consts
{
    class CInt : BaseConstant
    {
        public CInt(int value)
        {
            this.value = value;
        }

        public static CInt operator -(CInt v) => new CInt(-(int)v.GetValue()); //unary minus

        public static CInt operator -(CInt v1, CInt v2) => new CInt((int)v1.GetValue() - (int)v2.GetValue());
        public static CInt operator +(CInt v1, CInt v2) => new CInt((int)v1.GetValue() + (int)v2.GetValue());
        public static CInt operator *(CInt v1, CInt v2) => new CInt((int)v1.GetValue() * (int)v2.GetValue());
        public static CInt operator /(CInt v1, CInt v2) => new CInt((int)v1.GetValue() / (int)v2.GetValue());

        public override string ToString()
        {
            return "Int{ " + value + " };";
        }
    }
}
