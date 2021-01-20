using System;
using Luajit_Decompiler.dis;

namespace Luajit_Decompiler.dec.lir
{
    /*
 * Plan for Linear/Graphical IR
 * BCI in Blocks -> LIR1 (This class)
 * LIR1 -> LIR2 (applying simple rules to IR1 to group and simplify farther)
 * LIR2 -> GIR1 (Creating a graph to recreate flow of control)
 * GIR1 -> GIR2 (same as LIR1 -> LIR2, simplify farther if possible)
 * GIR2 -> Target Source Code
 */
    public enum IRMap
    {
        Eval,       //evaluate a conditional expression.
        Concat,     //Concatenate Operation
        MathVN,     //variable op constant# (arithmetic)
        MathVV,     //variable op variable (arithmetic)
        MathNV,     //constant# op variable (arithmetic
        Unary,      //move, not, unary minus, length
        Const,      //used to set constants.
        SetUV,      //Set upvalue.
        GetUV,      //Get upvalue
        NewFunc,    //new function
        UVClose,    //close upvalues
        TGet,       //get for tables
        TSet,       //set for tables
        NewTable,   //new table
        CopyTable,  //copy a table
        Call,       //for function calls and their respective tail calls
        Iterate,    //for iteration loop function calls. [pairs(), next()]
        VarArg,     //helps with variable arguments in iterate
        IsNext,     //helps with iteration loop
        Return,     //all return statements w/ or w/o return value(s)
        Goto,       //jump statements
        Loop,       //generic while loop
        FLoop,      //numeric for loop
        Func,       //For all lua functions excluding new function closure.
        CHeadFunc,  //for all psuedo header for C functions/wrapped C functions/etc.
        GTSet,      //set to the global table
        GTGet,      //get from the global table.

        EndOfIIStream //A flag instruction for when the decompiler runs out of instructions to interpret.
    }

    class IRIMap //Intermediate Representation Instruction Mapping
    {
        public IRMap Translate(OpCodes op)
        {
            switch (op)
            {
                case OpCodes.ISLT:
                case OpCodes.ISGE:
                case OpCodes.ISGT:
                case OpCodes.ISLE:
                case OpCodes.ISEQV:
                case OpCodes.ISNEV:
                case OpCodes.ISEQS:
                case OpCodes.ISNES:
                case OpCodes.ISEQN:
                case OpCodes.ISNEN:
                case OpCodes.ISEQP:
                case OpCodes.ISNEP:

                //Unary operation. 'C' copys D to A then jumps.
                case OpCodes.ISTC:
                case OpCodes.IST:
                case OpCodes.ISFC:
                case OpCodes.ISF:
                    return IRMap.Eval;

                #region Arithmetic

                //VN
                case OpCodes.ADDVN:
                case OpCodes.SUBVN:
                case OpCodes.MULVN:
                case OpCodes.DIVVN:
                case OpCodes.MODVN:
                    return IRMap.MathVN;
                
                //NV
                case OpCodes.ADDNV:
                case OpCodes.SUBNV:
                case OpCodes.MULNV:
                case OpCodes.DIVNV:
                case OpCodes.MODNV:
                    return IRMap.MathNV;

                
                //VV
                case OpCodes.SUBVV:
                case OpCodes.ADDVV:
                case OpCodes.MULVV:
                case OpCodes.DIVVV:
                case OpCodes.MODVV:
                case OpCodes.POW: //A = B^C (Similar to VV as it operates with 2 variable slots).
                    return IRMap.MathVV;

                #endregion

                case OpCodes.NOT: //set A to !D
                case OpCodes.UNM: //set A to -D
                case OpCodes.LEN: //set A to D
                case OpCodes.MOV: //set A to #D (Obj Length)
                    return IRMap.Unary;

                //Sets slot A to D.
                case OpCodes.KSTR:
                case OpCodes.KCDATA:
                case OpCodes.KSHORT:
                case OpCodes.KNUM:
                case OpCodes.KPRI:
                case OpCodes.KNIL: //specifically sets A through D to nil.
                    return IRMap.Const;

                //Set (upvalue A) to D.
                case OpCodes.USETV:
                case OpCodes.USETS:
                case OpCodes.USETN:
                case OpCodes.USETP:
                    return IRMap.SetUV;

                //A = B[C]
                case OpCodes.TGETB: 
                case OpCodes.TGETS:
                case OpCodes.TGETV:
                    return IRMap.TGet;

                //B[C] = A
                case OpCodes.TSETB:
                case OpCodes.TSETM:
                case OpCodes.TSETS:
                case OpCodes.TSETV:
                    return IRMap.TSet;

                //CALL and CALLT for the call and tail call, CALLM and CALLMT for multiple result call and its tail call.
                case OpCodes.CALL:
                case OpCodes.CALLM:
                case OpCodes.CALLT:
                case OpCodes.CALLMT:
                    return IRMap.Call;

                //Calls to lua's pair() or next() functions when using a loop to iterate over a set.
                case OpCodes.ITERC:
                case OpCodes.ITERN:
                    return IRMap.Iterate; //The iterator 'for' loop: for vars... in iter, state, ctl do body end => set iter,state,ctl JMP body ITERC ITERL

                case OpCodes.RET:
                case OpCodes.RET0:
                case OpCodes.RET1:
                case OpCodes.RETM:
                    return IRMap.Return;

                case OpCodes.FORI:
                case OpCodes.JFORI:
                case OpCodes.FORL:
                case OpCodes.IFORL:
                case OpCodes.JFORL:
                case OpCodes.ITERL: //iterative for loop
                case OpCodes.IITERL:
                case OpCodes.JITERL:
                    return IRMap.FLoop;

                case OpCodes.LOOP: //includes repeat loop
                case OpCodes.ILOOP:
                case OpCodes.JLOOP:
                    return IRMap.Loop;

                case OpCodes.FUNCF:
                case OpCodes.IFUNCF:
                case OpCodes.JFUNCF:
                case OpCodes.FUNCV:
                case OpCodes.IFUNCV:
                case OpCodes.JFUNCV:
                    return IRMap.Func;

                case OpCodes.FUNCC:
                case OpCodes.FUNCCW:
                    return IRMap.CHeadFunc;

                #region 1:1 Translations

                //sets A to (upvalue D).
                case OpCodes.UGET: 
                    return IRMap.GetUV;

                case OpCodes.CAT: //A = B concat C; A=DST, B=rbase, C=rbase; (From luajit)-> Note: The CAT instruction concatenates all values in variable slots B to C inclusive.
                    return IRMap.Concat;

                case OpCodes.FNEW:
                    return IRMap.NewFunc;

                case OpCodes.UCLO: //apparently has a jump target...probably around the FNEW statement?
                    return IRMap.UVClose; 

                case OpCodes.TNEW:
                    return IRMap.NewTable;

                case OpCodes.TDUP:
                    return IRMap.CopyTable;

                case OpCodes.VARG:
                    return IRMap.VarArg;

                case OpCodes.ISNEXT:
                    return IRMap.IsNext;

                case OpCodes.JMP:
                    return IRMap.Goto; //done

                //GGET and GSET are named 'global' get and set, but actually index the current function environment getfenv(1) (which is usually the same as _G).
                case OpCodes.GGET: //A = _G[D]
                    return IRMap.GTGet; //done

                case OpCodes.GSET: //_G[D] = A
                    return IRMap.GTSet; //done

                #endregion
                default:
                    throw new Exception("Failed to translate opcode to a map equivalent.");
            }
        }
    }
}