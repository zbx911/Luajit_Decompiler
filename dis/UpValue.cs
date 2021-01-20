namespace Luajit_Decompiler.dis
{
    class UpValue
    {
        public int TableIndex { get; set; } //which index of the table to look at.
        public int TableLocation { get; set; } //which table to look at. If it is 192, look at the global constants table at tableIndex. 0 means look at the upvalues table at index in the prototype's parent.

        public UpValue(int v1, int v2)
        {
            TableIndex = v1;
            TableLocation = v2;
        }

        public override string ToString()
        {
            return "Upvalue{ " + TableIndex + ", " + TableLocation + " };";
        }
    }
}
