using Luajit_Decompiler.dec.data;
using Luajit_Decompiler.dec.lua_formatting;
using Luajit_Decompiler.dis;
using Luajit_Decompiler.dis.consts;
using System;
using System.Collections.Generic;

namespace Luajit_Decompiler.dec.state_machine
{
    class LStateContext
    {
        public LState State { get; private set; }
        public DecompiledLua lua;
        public BaseConstant[] upvalues;
        public BaseConstant[] slots;

        //global constants organized by frame index.
        public List<BaseConstant> num_G;
        public List<CString> string_G;
        public List<CTable> table_G;
        public List<string> varNames;

        public ControlFlowGraph cfg;
        public string[] upvalueNames;
        //public Block currentBlock;
        public BlockWriter blockWriter;

        private Prototype pt;

        public LStateContext(Prototype pt, ControlFlowGraph cfg, DecompiledLua lua, BlockWriter bw)
        {
            this.pt = pt;
            this.cfg = cfg;
            this.lua = lua;
            varNames = pt.variableNames;
            upvalues = new BaseConstant[pt.upvalues.Count];
            upvalueNames = new string[upvalues.Length];
            slots = new BaseConstant[pt.frameSize];
            blockWriter = bw;
            InitUpvalues();
            PrepareGlobals();
        }

        public void Transition(LState state, BytecodeInstruction bci)
        {
            State = state;
            state.Context = this;
            state.Bci = bci;
            HandleState();
        }

        private void HandleState()
        {
            State.HandleLua();
            State.HandleSlots();
        }

        //TODO: inituvs should also init varnames too

        private void InitUpvalues()
        {
            for (int i = 0; i < pt.upvalues.Count; i++)
                upvalues[i] = RecursiveGetUpvalue(pt, pt.upvalues[i]);
        }

        private BaseConstant RecursiveGetUpvalue(Prototype pt, UpValue uv)
        {
            //apparently the first bit of 192 determines if we look at the constants section table or not. 
            //the second bit of 192 means if it is mutable or not. 1 = immutable upvalue -- whatever that means in terms of upvalues...
            if (uv.TableLocation == 192)
                return pt.parent.constantsSection[uv.TableIndex];
            return RecursiveGetUpvalue(pt.parent, pt.parent.upvalues[uv.TableIndex]);
        }

        private void PrepareGlobals()
        {
            //place into their own individual arrays since LJ accesses constants this way.
            num_G = new List<BaseConstant>();
            string_G = new List<CString>();
            table_G = new List<CTable>();

            foreach (BaseConstant c in pt.constantsSection.ToArray())
            {
                Type t = c.GetType();
                if (t.Equals(typeof(CString)))
                    string_G.Add((CString)c);
                else if (t.Equals(typeof(CTable)))
                    table_G.Add((CTable)c);
                else //numeric most likely
                    num_G.Add(c);
            }

            //reverse the order of strings so that they correctly line up with slot indicies
            string_G.Reverse();
        }
    }
}
