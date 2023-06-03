namespace FamiComEmulator.Components
{
    internal class Ram
    {
        public Ram() 
        {
            Array.Clear(Memory, 0, Memory.Length);
        }

        public byte[] Memory = new byte[64 * 1024];
    }
}
