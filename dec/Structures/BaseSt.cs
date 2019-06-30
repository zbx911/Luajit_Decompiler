using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luajit_Decompiler.dis;

namespace Luajit_Decompiler.dec.Structures
{
    class BaseSt
    {
        protected Prototype current;
        protected int bciOffset; //! An issue could be that when the ref is passed here, it just makes a new value instead of storing it as a reference.
        protected int tabLevel;

        public BaseSt(Prototype current, ref int bciOffset, ref int tabLevel)
        {
            this.current = current;
            this.bciOffset = bciOffset;
            this.tabLevel = tabLevel;
        }

        public override string ToString()
        {
            return "--Implementation Unavailable.--";
        }
    }
}
