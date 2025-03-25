using KidPix.API.Importer.Mohawk;

namespace KidPix.API
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
}
