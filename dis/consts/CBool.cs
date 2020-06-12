namespace Luajit_Decompiler.dis.consts
{
    class CBool : BaseConstant
    {
        public CBool(bool value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return "Bool{ " + value + " };";
        }
    }
}
