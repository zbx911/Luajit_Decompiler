using Luajit_Decompiler.dis.consts;
using System;
using System.Text;

namespace Luajit_Decompiler.dec.tluastates
{
    class UnaryState : BaseState
    {
        public UnaryState(TLuaState state) : base(state) { }

        //More slot operations...these probably have lua translations associated with them...
        public override void WriteLua(TLuaState state) //write the code first then handle the back end in the constructor...
        {
            switch (state.curII.originalOp)
            {
                case dis.OpCodes.MOV:
                    HandleMov(state);
                    break;
                case dis.OpCodes.NOT:
                    HandleNot(state);
                    break;
                case dis.OpCodes.UNM:
                    HandleUnaryMinus(state);
                    break;
                case dis.OpCodes.LEN:
                    HandleLength(state);
                    break;
            }
        }

        //Copy D into A operation.
        private void HandleMov(TLuaState state)
        {
            //slot operation:  slot[A] = slot[D]
            state.slots[state.regs.regA] = state.slots[state.regs.regD];

            //source
            StringBuilder line = new StringBuilder();
            var dst = state.CheckGetVarName(state.regs.regA);
            var src = state.CheckGetVarName(state.regs.regD);
            state.CheckLocal(dst, ref line);
            line.Append(dst.Item2 + " = " + src.Item2);
            line.AppendLine(" --MOV");
            state.decompLines.Add(line.ToString());

            //debugging
            //FileManager.WriteDebug(line.ToString());
        }

        private void HandleNot(TLuaState state)
        {
            //slot operation: slot[A] = !slot[D]
            if (state.slots[state.regs.regD].GetType() == typeof(CBool))
                state.slots[state.regs.regA] = !(CBool)state.slots[state.regs.regD]; //bug here currently
            else throw new Exception("NOT declared on non-boolean operand.");

            //source
            StringBuilder line = new StringBuilder();
            var dst = state.CheckGetVarName(state.regs.regA);
            var src = state.CheckGetVarName(state.regs.regD);
            state.CheckLocal(dst, ref line);          
            line.Append(dst.Item2 + " = not " + src.Item2);
            line.AppendLine(" --NOT");
            state.decompLines.Add(line.ToString());

            //debugging
            //FileManager.WriteDebug(line.ToString());
        }

        private void HandleUnaryMinus(TLuaState state)
        {
            //slot operation: slot[A] = -slot[D] //TODO: Error checking
            if (state.slots[state.regs.regD].GetType() == typeof(CInt))
                state.slots[state.regs.regA] = -(CInt)state.slots[state.regs.regD];

            else if (state.slots[state.regs.regD].GetType() == typeof(CShort))
                state.slots[state.regs.regA] = -(CShort)state.slots[state.regs.regD];

            else if (state.slots[state.regs.regD].GetType() == typeof(CLuaNumber))
            {
                //state.slots[state.regs.regA] = -(CLuaNumber)state.slots[state.regs.regD];
                throw new NotImplementedException("UNM declared on a lua number. Currently unimplemented.");
            }
            else
                throw new Exception("UNM declared on non-numeric data.");
            
            //source
            StringBuilder line = new StringBuilder();
            var dst = state.CheckGetVarName(state.regs.regA);
            var src = state.CheckGetVarName(state.regs.regD);
            state.CheckLocal(dst, ref line);
            line.Append(dst.Item2 + " = -" + src.Item2);
            line.AppendLine(" --UNM");
            state.decompLines.Add(line.ToString());

            //debugging
            //FileManager.WriteDebug(line.ToString());
        }


        //TODO: Handle if it is accessing a global OR a slot value.
        //Handle non-inline length operations.
        private void HandleLength(TLuaState state)
        {
            StringBuilder line = new StringBuilder();

            //Length operator # is indicated by registers A and D being the same as the previous line's constant's A register.
            //If these conditions are met, we inline a length operator and the constant.

            var pReg = state.prevII.registers;
            var dst = state.CheckGetVarName(state.regs.regA); //should be the same destiation as the last line...
            if (pReg.regA == state.curII.registers.regA && state.curII.registers.regA == state.curII.registers.regD) //inline check
            {
                state.decompLines.RemoveAt(state.decompLines.Count - 1); //remove last entry.

                line.Append("local " + dst.Item2 + " = " + "#" + state.slots[state.regs.regD].GetValue()); //might want to ensure that we are really working with a new variable declaration for length later...and we assume global constants table here too.

                //debugging
                //FileManager.WriteDebug(line.ToString());
            }
            else
            {
                state.CheckLocal(dst, ref line);        
                line.Append(dst.Item2 + " = " + "#" + state.slots[state.regs.regD].GetValue());
            }
            line.AppendLine(" --LEN");
            state.decompLines.Add(line.ToString());
        }
    }
}
