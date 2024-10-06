using FamiComEmulator.Components;

namespace FamiComEmulator
{
    public class InputHandler
    {
        private readonly ICentralBus _bus;

        // Map of keys to controller buttons
        private readonly Dictionary<Keys, byte> _keyToButtonMap = new Dictionary<Keys, byte>
        {
            { Keys.Z, 0x80 },       // Button A
            { Keys.X, 0x40 },       // Button B
            { Keys.Back, 0x20 }, // Select
            { Keys.Space, 0x10 },    // Start
            { Keys.W, 0x08 },       // Up
            { Keys.S, 0x04 },     // Down
            { Keys.A, 0x02 },     // Left
            { Keys.D, 0x01 }     // Right
        };

        public InputHandler(ICentralBus bus)
        {
            _bus = bus;
            _bus.Controller[0] = 0x00;
        }

        public void KeyDown(Keys key)
        {
            if (_keyToButtonMap.TryGetValue(key, out byte value))
            {
                _bus.Controller[0] |= value;
            }
        }

        public void KeyUp(Keys key)
        {
            if (_keyToButtonMap.TryGetValue(key, out byte value))
            {
                _bus.Controller[0] &= (byte)~value;
            }
        }
    }
}
