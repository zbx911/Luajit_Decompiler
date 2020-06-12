namespace Luajit_Decompiler.dis.consts
{
    class CInt : BaseConstant
    {
        public CInt(int value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return "Int{ " + value + " };";
        }
    }
}
