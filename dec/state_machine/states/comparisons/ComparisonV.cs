namespace Luajit_Decompiler.dec.state_machine.states.comparisons
{
    class ComparisonV : LState
    {
        public override void HandleLua()
        {
            Comparison c = new Comparison(Context, Bci);
            c.HandleLua(c.GetCompV);
        }

        public override void HandleSlots()
        {
            return;
        }
    }
}
