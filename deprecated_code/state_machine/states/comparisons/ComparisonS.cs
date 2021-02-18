namespace Luajit_Decompiler.dec.state_machine.states.comparisons
{
    class ComparisonS : LState
    {
        public override void HandleLua()
        {
            Comparison c = new Comparison(Context, Bci);
            c.HandleLua(c.GetCompS);
        }

        public override void HandleSlots()
        {
            return;
        }
    }
}
