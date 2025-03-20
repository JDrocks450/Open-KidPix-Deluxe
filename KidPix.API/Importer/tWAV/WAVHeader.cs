namespace KidPix.API.Importer.tWAV
{
    /// <summary>
    /// Contains Header data preceding the WAVEData
    /// </summary>
    public class WAVHeader
    {
        public string? FileName { get; set; }
        public uint Unknown1 { get; set; }
        public ushort Unknown2 { get; set; }
        public uint Unknown3 { get; set; }
        public ushort Unknown4 { get; set; }
    }
}
