namespace KidPix.API.Importer.tWAV
{
    public record WAVData(WAVEncodings Encoding, ushort SampleRate, uint SampleCount, byte BitsPerSample,
        byte Channels, ushort LoopCount, uint LoopStart, uint LoopEnd) : IDisposable
    {
        public Stream? AudioDataStream { get; internal set; }

        public void Dispose()
        {
            AudioDataStream?.Dispose();
            AudioDataStream = null;
        }
    }
}
