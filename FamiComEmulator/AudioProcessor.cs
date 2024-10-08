using FamiComEmulator;
using NAudio.Wave;

public class AudioProcessor : IAudioProcessor
{
    private readonly List<float> sampleBuffer = new List<float>();
    private readonly int sampleRate = 44100; // Standard sample rate for audio
    private readonly int samplesPerFrame = 735; // Number of samples to accumulate before playing (corresponding to APU frequency)
    private WaveOutEvent waveOut;
    private BufferedWaveProvider bufferedWaveProvider;

    public AudioProcessor()
    {
        waveOut = new WaveOutEvent();
        bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(sampleRate, 1)); // 1 channel (mono)
        waveOut.Init(bufferedWaveProvider);
        waveOut.Play();
    }

    /// <summary>
    /// Processes the current sample from the APU.
    /// </summary>
    /// <param name="sample">The current audio sample from the APU.</param>
    public void ProcessSample(float sample)
    {
        // Add the sample to the buffer
        sampleBuffer.Add(sample);

        // If we have accumulated enough samples, send them to the audio device
        if (sampleBuffer.Count >= samplesPerFrame)
        {
            // Convert the float samples to bytes (as 16-bit PCM data)
            byte[] byteBuffer = new byte[sampleBuffer.Count * 2];
            for (int i = 0; i < sampleBuffer.Count; i++)
            {
                short pcmValue = (short)(sampleBuffer[i] * short.MaxValue); // Convert float to 16-bit PCM
                byteBuffer[i * 2] = (byte)(pcmValue & 0xff);
                byteBuffer[i * 2 + 1] = (byte)((pcmValue >> 8) & 0xff);
            }

            // Write the samples to the audio buffer
            bufferedWaveProvider.AddSamples(byteBuffer, 0, byteBuffer.Length);
            sampleBuffer.Clear(); // Clear the buffer after sending the samples
        }
    }
}
