namespace Luajit_Decompiler.dis.consts
{
    class CTable : BaseConstant
    {
        public CTable(TableConstant value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }
}
