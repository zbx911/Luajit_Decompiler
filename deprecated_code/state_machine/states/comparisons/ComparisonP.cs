namespace Luajit_Decompiler.dec.state_machine.states.comparisons
{
    class ComparisonP : LState
    {
        public override void HandleLua()
        {
            Comparison c = new Comparison(Context, Bci);
            c.HandleLua(c.GetCompP);
        }

        public override void HandleSlots()
        {
            return;
        }
    }
}
