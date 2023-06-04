namespace FamiComEmulator.Components
{
    public class Ram
    {
        public Ram() 
        {
            Memory = new byte[64 * 1024];
            Array.Clear(Memory, 0, Memory.Length);
        }

        public byte[] Memory;
    }
}
