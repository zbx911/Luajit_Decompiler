namespace Luajit_Decompiler.dis.consts
{
    class CShort : BaseConstant
    {
        public CShort(short value)
        {
            this.value = value;
        }

        public static CShort operator -(CShort v) => new CShort((short)-(short)v.GetValue());

        public override string ToString()
        {
            return "Short{ " + value + " };";
        }
    }
}
