namespace Luajit_Decompiler.dis.consts
{
    class CShort : BaseConstant
    {
        public CShort(short value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return "Short{ " + value + " };";
        }
    }
}
