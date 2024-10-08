namespace FamiComEmulator.Components
{
    /// <summary>
    /// Interface for the APU (Audio Processing Unit) in the NES emulator.
    /// </summary>
    public interface IApu
    {
        /// <summary>
        /// Writes a byte to the specified APU register.
        /// </summary>
        /// <param name="address">The register address to write to.</param>
        /// <param name="data">The byte to write.</param>
        void Write(ushort address, byte data);

        /// <summary>
        /// Resets the APU.
        /// </summary>
        void Reset();

        /// <summary>
        /// Advances the APU by one clock cycle.
        /// </summary>
        void Clock();

        /// <summary>
        /// Public property that provides the current sample data
        /// </summary>
        float CurrentSample { get; }
    }
}
