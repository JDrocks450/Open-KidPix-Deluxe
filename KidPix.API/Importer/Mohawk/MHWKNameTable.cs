namespace KidPix.API.Importer.Mohawk
{
    public class MHWKNameTable
    {
        public class NameTableEntry
        {
            public ushort Offset { get; internal set; }
            public ushort Index { get; internal set; }
            public string Name { get; internal set; } = "";
        }

        public HashSet<NameTableEntry> Entries { get; } = new();
    }
}
