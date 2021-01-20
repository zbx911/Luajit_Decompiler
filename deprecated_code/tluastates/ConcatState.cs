using System.Text;

namespace Luajit_Decompiler.dec.tluastates
{
    class ConcatState : BaseState
    {
        public ConcatState(TLuaState state) : base(state)
        {
        }

        public override void WriteLua(TLuaState state)
        {
            StringBuilder line = new StringBuilder();
            var dst = state.CheckGetVarName(state.regs.regA);
            var b = state.CheckGetVarName(state.regs.regB);

            state.CheckLocal(dst, ref line);
            line.Append(dst.Item2 + " = " + b.Item2 + " .. ");

            //concatenate variables in slots B->C INCLUSIVE -- Lua string concat operator is ".."
            for(int i = state.regs.regB + 1; i <= state.regs.regC; i++)
            {
                var nextCat = state.CheckGetVarName(i); //I guess we assume all the slots for the concat operation are loaded?
                line.Append(nextCat.Item2);
                if (i + 1 <= state.regs.regC)
                    line.Append(" .. ");
            }
            line.Append(" --CAT");
            state.decompLines.Add(line.ToString() + "\n");
        }
    }
}
