namespace Luajit_Decompiler.dis.consts
{
    class CBool : BaseConstant
    {
        public CBool(bool value)
        {
            this.value = value;
        }

        public static CBool operator !(CBool v) => new CBool(!(bool)v.GetValue());

        public override string ToString()
        {
            return "Bool{ " + value + " };";
        }
    }
}
