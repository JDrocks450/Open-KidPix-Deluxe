namespace KidPix.API.Importer.Mohawk
{
    /// <summary>
    /// Wrapper for <see cref="CHUNK_TYPE"/> with the ability to set a String version in the event the tag is not defined in <see cref="CHUNK_TYPE"/>    
    /// </summary>
    /// <param name="ChunkType"></param>
    /// <param name="AssetID"></param>
    public readonly record struct MHWKIdentifierToken(CHUNK_TYPE ChunkType, ushort AssetID)
    {
        public CHUNK_TYPE ChunkType { get; } = ChunkType;
        public ushort AssetID { get; } = AssetID;
    }

    /// <summary>
    /// See: <see cref="MHWKTypeDescription"/> to get names for each of these Chunk Types
    /// </summary>
    public enum CHUNK_TYPE : uint
    {
        ERRR = 0xFFFFFFFF,
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
