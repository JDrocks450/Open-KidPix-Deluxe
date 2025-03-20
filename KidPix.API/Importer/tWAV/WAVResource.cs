using KidPix.API.Importer.Mohawk;

namespace KidPix.API.Importer.tWAV
{

    /// <summary>
    /// A WAVE resource file (tWAV)
    /// </summary>
    public class WAVResource : KidPixResource, IDisposable
    {        
        /// <summary>
        /// The header of this Wave file, which contains its resource name -- if included in the archive
        /// </summary>
        public WAVHeader? Header { get; internal set; }
        /// <summary>
        /// The resource file name, taken from the <see cref="Header"/>
        /// </summary>
        public string? FileName => Header?.FileName ?? ParentEntry?.Name;
        /// <summary>
        /// The resource ID of this <see cref="WAVResource"/>, provided from the <see cref="MHWKFile"/> it was read from in the file table
        /// </summary>
        public uint ID => ParentEntry?.Id ?? 0;

        /// <summary>
        /// A stream containing WaveCue data -- not supported yet
        /// </summary>
        public Stream? WaveCueStream { get; internal set; }
        /// <summary>
        /// A <see cref="WAVData"/> resource container which holds the Raw Wave data, bitrate, etc.
        /// </summary>
        public WAVData? WaveData { get; internal set; }

        /// <summary>
        /// True if <see cref="Header"/> is not null
        /// </summary>
        public bool HeaderPresent => Header != null;
        /// <summary>
        /// True if <see cref="WaveCueStream"/> is not null
        /// </summary>
        public bool WaveCuePresent => WaveCueStream != null;
        /// <summary>
        /// True if <see cref="WaveData"/> is not null
        /// </summary>
        public bool WaveDataPresent => WaveData != null;
        /// <summary>
        /// True if all data types are present
        /// </summary>
        public bool IsComplete => HeaderPresent && WaveCuePresent && WaveDataPresent;

        internal WAVResource(ResourceTableEntry ParentEntry) : base(ParentEntry)
        {

        }

        public void Dispose()
        {
            WaveCueStream?.Dispose();
            WaveCueStream = null;

            WaveData?.Dispose();
            WaveData = null;
        }
    }
}
