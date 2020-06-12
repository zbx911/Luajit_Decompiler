namespace Luajit_Decompiler.dis.consts
{
    class BaseConstant
    {
        protected object value;

        public virtual object GetValue()
        {
            return value;
        }
    }
}
