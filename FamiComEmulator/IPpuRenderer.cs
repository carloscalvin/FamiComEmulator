using System.Drawing;

namespace FamiComEmulator
{
    public interface IPpuRenderer
    {
        void SetPixel(int x, int y, Color color);
        void Clear();
        void Run();
    }
}
