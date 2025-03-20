namespace KidPix.API.Importer.Mohawk
{
    /// <summary>
    /// An entry in the <see cref="MHWKResourceTable"/> outlining where in the <see cref="MHWKFile"/> the resource data is
    /// </summary>
    public class ResourceTableEntry
    {
        public ResourceTableEntry(CHUNK_TYPE type, ushort id, ushort index)
        {
            Id = id;
            Index = index;
            EnclosingType = type;
        }

        public ushort Id { get; }
        public ushort Index { get; }
        public string Name { get; internal set; }
        public uint Offset { get; internal set; }
        public long Size { get; internal set; }

        public CHUNK_TYPE? EnclosingType { get; internal set; } = null;
    }
}
