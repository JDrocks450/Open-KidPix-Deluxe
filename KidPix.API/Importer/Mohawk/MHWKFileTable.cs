namespace KidPix.API.Importer.Mohawk
{
    public class MHWKFileTable
    {
        private uint _entries;

        public HashSet<FileTableEntry> Entries { get; }
        public uint AbsoluteOffset { get; }
        public uint FileTableOffset { get; internal set; }

        public MHWKFileTable(uint NumEntries, uint AbsoluteOffset)
        {
            _entries = NumEntries;
            Entries = new((int)NumEntries);

            this.AbsoluteOffset = AbsoluteOffset;
        }

        public class FileTableEntry
        {
            public uint Offset { get; internal set; }
            public int Size { get; internal set; }
            public byte Flags { get; internal set; }
            public ushort Unknown { get; internal set; }

            public override string ToString()
            {
                return $"Offset = {Offset:X8}  Size = {Size:X7}  Flags = {Flags:X2}  Unknown = {Unknown:X4}";
            }
        }

        public FileTableEntry this[int i] => Entries.ElementAt(i);
    }
}
