namespace Luajit_Decompiler.dec.tluastates
{
    abstract class BaseState
    {
        protected TLuaState tLuaState;
        public BaseState(TLuaState state) { tLuaState = state; }
        public abstract void NextState();
        public abstract void WriteLua();
    }
}
