using System.Collections.Generic;
using Luajit_Decompiler.dis;

namespace Luajit_Decompiler.dec.state_machine.states
{
    class UnaryState : LState
    {
        private Dictionary<OpCodes, string> opMap = new Dictionary<OpCodes, string>()
        {
            { OpCodes.MOV, "" },
            { OpCodes.UNM, "-" },
            { OpCodes.NOT, "~" },
            { OpCodes.LEN, "#" },
        };

        public override void HandleLua()
        {
            (string, bool) dstAndAccessed = Context.varNames.GetVarNameAndCheckAccessed(Bci.registers.a);
            //Context.lua.CheckAddAssignmentAndSetAccessed(dstAndAccessed, opMap[Bci.opcode] + Context.varNames.GetVariableName(Bci.registers.d), Context);
        }

        public override void HandleSlots()
        {
            //throw new NotImplementedException();
        }
    }
}
