namespace Luajit_Decompiler.dis.consts
{
    class CString : BaseConstant
    {
        public CString(string value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return "String{ " + value + " };";
        }
    }
}
