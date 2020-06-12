namespace Luajit_Decompiler.dis.consts
{
    class CNil : BaseConstant
    {
        public CNil()
        {
            value = "nil";
        }

        public override string ToString()
        {
            return "Nil{ };";
        }
    }
}
