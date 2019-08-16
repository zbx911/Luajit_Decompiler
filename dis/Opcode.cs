using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luajit_Decompiler.dis
{
    /// <summary>
    /// A list of all the op codes for luajit. Order is important. From: lj_bc.h in luajit files.
    /// </summary>
    public enum OpCodes
    {
        ISLT,
        ISGE,
        ISLE,
        ISGT,
        ISEQV,
        ISNEV,
        ISEQS,
        ISNES,
        ISEQN,
        ISNEN,
        ISEQP,
        ISNEP,
        ISTC,
        ISFC,
        IST,
        ISF,
        MOV,
        NOT,
        UNM,
        LEN,
        ADDVN,
        SUBVN,
        MULVN,
        DIVVN,
        MODVN,
        ADDNV,
        SUBNV,
        MULNV,
        DIVNV,
        MODNV,
        ADDVV,
        SUBVV,
        MULVV,
        DIVVV,
        MODVV,
        POW,
        CAT,
        KSTR,
        KCDATA,
        KSHORT,
        KNUM,
        KPRI,
        KNIL,
        UGET,
        USETV,
        USETS,
        USETN,
        USETP,
        UCLO,
        FNEW,
        TNEW,
        TDUP,
        GGET,
        GSET,
        TGETV,
        TGETS,
        TGETB,
        TSETV,
        TSETS,
        TSETB,
        TSETM,
        CALLM,
        CALL,
        CALLMT,
        CALLT,
        ITERC,
        ITERN,
        VARG,
        ISNEXT,
        RETM,
        RET,
        RET0,
        RET1,
        FORI,
        JFORI,
        FORL,
        IFORL,
        JFORL,
        ITERL,
        IITERL,
        JITERL,
        LOOP,
        ILOOP,
        JLOOP,
        JMP,
        FUNCF,
        IFUNCF,
        JFUNCF,
        FUNCV,
        IFUNCV,
        JFUNCV,
        FUNCC,
        FUNCCW,

        //Non-Luajit, IR Ocodes.
        _if,
        ifelse,
        ieie, // if elseif... else
        removed,
        _goto
    }

    class Opcode
    {
        /// <summary>
        /// Returns associated opcode from a given byte.
        /// </summary>
        /// <param name="b">The opcode byte to be parsed.</param>
        /// <returns></returns>
        public static OpCodes ParseOpByte(byte b)
        {
            string[] opcodes = Enum.GetNames(typeof(OpCodes));
            if (b > opcodes.Length)
                throw new Exception("Error: Opcode.ParseOpByte :: Byte exceeds expected length. Are you sure the given byte is an opcode byte?");
            return (OpCodes) Enum.Parse(typeof(OpCodes), opcodes[b]);
        }
    }
}
