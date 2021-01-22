using System;
using Luajit_Decompiler.dec.state_machine.states;
using Luajit_Decompiler.dec.state_machine.states.comparisons;
using Luajit_Decompiler.dis;

namespace Luajit_Decompiler.dec.state_machine
{
    class StateMap
    {
        public static LState GetState(OpCodes op)
        {
            switch (op)
            {
                case OpCodes.ISLT:
                case OpCodes.ISGE:
                case OpCodes.ISGT:
                case OpCodes.ISLE:
                case OpCodes.ISEQV:
                case OpCodes.ISNEV:
                    return new ComparisonV();

                case OpCodes.ISEQS:
                case OpCodes.ISNES:
                    return new ComparisonS();

                case OpCodes.ISEQN:
                case OpCodes.ISNEN:
                    return new ComparisonN();

                case OpCodes.ISEQP:
                case OpCodes.ISNEP:
                    return new ComparisonP();

                case OpCodes.ISTC:
                case OpCodes.IST:
                case OpCodes.ISFC:
                case OpCodes.ISF:
                    throw new Exception("Unhandled comparison. These are more or less unary jumps.");

                #region Arithmetic

                //VN
                case OpCodes.ADDVN:
                case OpCodes.SUBVN:
                case OpCodes.MULVN:
                case OpCodes.DIVVN:
                case OpCodes.MODVN:
                    return new ArithVNState();

                //NV
                case OpCodes.ADDNV:
                case OpCodes.SUBNV:
                case OpCodes.MULNV:
                case OpCodes.DIVNV:
                case OpCodes.MODNV:
                    return new ArithNVState();


                //VV
                case OpCodes.SUBVV:
                case OpCodes.ADDVV:
                case OpCodes.MULVV:
                case OpCodes.DIVVV:
                case OpCodes.MODVV:
                case OpCodes.POW: //A = B^C (Similar to VV as it operates with 2 variable slots).
                    return new ArithVVState();

                #endregion

                case OpCodes.NOT: //set A to !D
                case OpCodes.UNM: //set A to -D
                case OpCodes.LEN: //set A to #D (Obj Length)
                case OpCodes.MOV: //set A to D
                    return new UnaryState();

                //Sets slot A to D.
                case OpCodes.KSTR:
                case OpCodes.KNUM:
                    return new ConstStateRef(); //by reference to something in global constants.

                case OpCodes.KSHORT:
                case OpCodes.KPRI:
                case OpCodes.KNIL: //specifically sets A through D to nil.
                case OpCodes.KCDATA:
                    return new ConstStateNew(); //create new, not by ref.

                //Set (upvalue A) to D.
                case OpCodes.USETV:
                case OpCodes.USETS:
                case OpCodes.USETN:
                case OpCodes.USETP:
                    return new SetUpValueState();

                //A = B[C]
                case OpCodes.TGETB:
                case OpCodes.TGETS:
                case OpCodes.TGETV:
                    return new TableGetState();

                //B[C] = A
                case OpCodes.TSETB:
                case OpCodes.TSETM:
                case OpCodes.TSETS:
                case OpCodes.TSETV:
                    return new TableSetState();

                //CALL and CALLT for the call and tail call, CALLM and CALLMT for multiple result call and its tail call.
                case OpCodes.CALL:
                case OpCodes.CALLM:
                case OpCodes.CALLT:
                case OpCodes.CALLMT:
                    return new CallState();

                //Calls to lua's pair() or next() functions when using a loop to iterate over a set.
                case OpCodes.ITERC:
                case OpCodes.ITERN:
                    return new IterState(); //The iterator 'for' loop: for vars... in iter, state, ctl do body end => set iter,state,ctl JMP body ITERC ITERL

                case OpCodes.RET:
                case OpCodes.RET0:
                case OpCodes.RET1:
                case OpCodes.RETM:
                    return new ReturnState();

                case OpCodes.FORI:
                case OpCodes.JFORI:
                case OpCodes.FORL:
                case OpCodes.IFORL:
                case OpCodes.JFORL:
                case OpCodes.ITERL: //iterative for loop
                case OpCodes.IITERL:
                case OpCodes.JITERL:
                    return new ForLoopState();

                case OpCodes.LOOP: //includes repeat loop
                case OpCodes.ILOOP:
                case OpCodes.JLOOP:
                    return new LoopState();

                case OpCodes.FUNCF:
                case OpCodes.IFUNCF:
                case OpCodes.JFUNCF:
                case OpCodes.FUNCV:
                case OpCodes.IFUNCV:
                case OpCodes.JFUNCV:
                    return new FuncState();

                case OpCodes.FUNCC:
                case OpCodes.FUNCCW:
                    return new CFuncState();

                #region 1:1 Translations

                //sets A to (upvalue D).
                case OpCodes.UGET:
                    return new GetUpValueState();

                case OpCodes.CAT: //A = B concat C; A=DST, B=rbase, C=rbase; (From luajit)-> Note: The CAT instruction concatenates all values in variable slots B to C inclusive.
                    return new ConcatState();

                case OpCodes.FNEW:
                    return new FuncNewState();

                case OpCodes.UCLO: //apparently has a jump target...probably around the FNEW statement?
                    return new UpValueCloseState();

                case OpCodes.TNEW:
                    return new TableNewState();

                case OpCodes.TDUP:
                    return new DuplicateTableState();

                case OpCodes.VARG:
                    return new VarArgState();

                case OpCodes.ISNEXT:
                    return new IsNextState();

                case OpCodes.JMP:
                    return new JumpState();

                //GGET and GSET are named 'global' get and set, but actually index the current function environment getfenv(1) (which is usually the same as _G).
                case OpCodes.GGET: //A = _G[D]
                    return new GTGetState();

                case OpCodes.GSET: //_G[D] = A
                    return new GTSetState();

                #endregion
                default:
                    throw new Exception("Failed to translate opcode to a map equivalent.");
            }
        }
    }
}
