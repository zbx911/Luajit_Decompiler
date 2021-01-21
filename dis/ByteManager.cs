namespace Luajit_Decompiler.dis
{
    class ByteManager
    {
        public byte[] FileBytes { get; private set; }
        public int Offset { get; private set; }

        public ByteManager(byte[] fileBytes)
        {
            FileBytes = fileBytes;
            Offset = 0;
        }

        /// <summary>
        /// Returns a byte from the given byte array and increments the offset by 1.
        /// </summary>
        /// <param name="bytes">Array of all bytecode.</param>
        /// <param name="offset">Current offset in the bytecode array.</param>
        /// <returns></returns>
        public byte ConsumeByte()
        {
            byte result = FileBytes[Offset];
            Offset++;
            return result;
        }

        /// <summary>
        /// Returns a byte array of consumed bytes from the given array and increments the offset accordingly.
        /// </summary>
        /// <param name="bytes">Array of all bytecode.</param>
        /// <param name="offset">Current offset in the bytecode array.</param>
        /// <param name="length">The number of bytes to be read and returned.</param>
        /// <returns></returns>
        public byte[] ConsumeBytes(int length)
        {
            byte[] result = new byte[length];
            for (int i = 0; i < length; i++)
                result[i] = ConsumeByte();
            return result;
        }

        /// <summary>
        /// Consumes bytes that can be encoded in leb128. Returns their integer equivalent. Modified version of DiLemming's code.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public int ConsumeUleb()
        {
            int count = 0;
            int shift = 1;
            int cont = 0;
            byte b;
            int data;
            int value = 0;
            do
            {
                b = FileBytes[Offset + count];
                data = b & 127;
                cont = b & 128;
                value += data * shift;
                shift *= 128;
                count++;
            } while (cont != 0);
            Offset += count;
            return value;
        }
    }
}
