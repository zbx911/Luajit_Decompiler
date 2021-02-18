namespace Luajit_Decompiler.dec.state_machine.states.comparisons
{
    class ComparisonN : LState
    {
        public override void HandleLua()
        {
            Comparison c = new Comparison(Context, Bci);
            c.HandleLua(c.GetCompN);
        }

        public override void HandleSlots()
        {
            return;
        }
    }
}
