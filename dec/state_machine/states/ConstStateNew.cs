using Luajit_Decompiler.dis.consts;
using Luajit_Decompiler.dis;
using System;

namespace Luajit_Decompiler.dec.state_machine.states
{
    class ConstStateNew : LState
    {
        private delegate void DelHandleLua();
        private delegate void DelHandleSlots();

        private DelHandleLua hLua;
        private DelHandleSlots hSlots;

        public override void HandleLua()
        {
            SetDelegates();
            hLua();
        }

        public override void HandleSlots()
        {
            hSlots();
        }

        private void SetDelegates()
        {
            switch (Bci.opcode)
            {
                case OpCodes.KPRI: //1 value
                case OpCodes.KNIL: //multi value
                    hLua = NilLua;
                    hSlots = NilSlots;
                    break;
                case OpCodes.KCDATA:
                    throw new Exception("KCData needs to be examined deeper.");
                case OpCodes.KSHORT:
                    hLua = NewNumLua;
                    hSlots = NewNumSlots;
                    break;
            }
        }

        private void NewNumLua()
        {
            (string, bool) dstAndAccessed = Context.varNames.GetVarNameAndCheckAccessed(Bci.registers.a);
            Context.lua.CheckAddAssignmentAndSetAccessed(dstAndAccessed, Bci.registers.d.ToString(), Context);
        }

        private void NewNumSlots()
        {
            Context.slots[Bci.registers.a] = new CShort(Bci.registers.d);
        }

        private void NilLua()
        {
            (string, bool) dstAndAccessed = Context.varNames.GetVarNameAndCheckAccessed(Bci.registers.a);
            Context.lua.CheckAddAssignmentAndSetAccessed(dstAndAccessed, "nil", Context);
        }

        private void NilSlots()
        {
            if (Bci.registers.a < Bci.registers.d)
                for (int i = Bci.registers.a; i < Bci.registers.d; i++)
                    Context.slots[i] = new CNil();
            else
                Context.slots[Bci.registers.a] = new CNil();
        }
    }
}
