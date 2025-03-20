namespace KidPix.API.Importer.Mohawk
{
    /// <summary>
    /// Wrapper for <see cref="CHUNK_TYPE"/> with the ability to set a String version in the event the tag is not defined in <see cref="CHUNK_TYPE"/>    
    /// </summary>
    public class MHWKTypeDescriptor : IEquatable<CHUNK_TYPE>, IEquatable<MHWKTypeDescriptor>
    {
        public CHUNK_TYPE ChunkType { get; set; }
        public string StringFormat { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is CHUNK_TYPE inter)
                return ChunkType == inter;
            return base.Equals(obj);
        }
        public bool Equals(CHUNK_TYPE other) => ChunkType == other;
        public bool Equals(MHWKTypeDescriptor? other) => other != null ? other?.ChunkType == ChunkType : false;
    }

    /// <summary>
    /// See: <see cref="MHWKTypeDescription"/> to get names for each of these Chunk Types
    /// </summary>
    public enum CHUNK_TYPE : uint
    {
        /// <summary>
        /// An MHWK Chunk Type
        /// </summary>
        MHWK = 0x4D48574B,
        RSRC = 0x52535243,
        tMOV = 0x744D4F56,
        SND = 0x30534E44,
        REGS = 0x52454753,
        tBMH = 0x74424D48,
        tCNT = 0x74434E54,
        tSCR = 0x74534352,
        tBMP = 0x74424D50,
        MBTN = 0x4D42544E,
        tWAV = 0x74574156,     
        tPAL = 0x7450414C,
    }
    /// <summary>
    /// 64bit Chunk Types used for Wave files
    /// </summary>
    public enum LONG_CHUNK_TYPE : ulong
    {
        WAVECUE = 0x5741564543756523,
        WAVEDATA = 0x5741564544617461
    }
}
