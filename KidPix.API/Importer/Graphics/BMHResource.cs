using KidPix.API.Importer.Graphics.Brushes;
using KidPix.API.Importer.Mohawk;
using System.Drawing;
using System.Drawing.Imaging;

namespace KidPix.API.Importer.Graphics
{
    /// <summary>
    /// A frame in a <see cref="BMHResource"/>
    /// </summary>
    /// <param name="Offset"></param>
    /// <param name="Length"></param>
    public record BMHFrameInfo(int ResourceID, uint Offset, uint Length)
    {
        public int ResourceID { get; internal set; } = ResourceID;
        public uint Length { get; internal set; } = Length;

        public override string ToString()
        {
            return $"Resource {ResourceID}";
        }
    }

    /// <summary>
    /// Every <see cref="BMHResource"/> starts with a table section that maps out the offsets of all the <see cref="BMPResource"/>s it contains
    /// </summary>
    public class BMHTable
    {
        /// <summary>
        /// This maps <c>[RESOURCE ID,[FRAME NUMBER, FILE OFFSET]]</c>
        /// </summary>
        public Dictionary<int, BMHFrameInfo> Resources { get; } = new();

        public BMHFrameInfo this[int ResourceIndex] => Resources[ResourceIndex];

        public void AddResource(BMHFrameInfo ScanLineInfo)
        {
            int myKey = Resources.Count;
            ScanLineInfo.ResourceID = myKey;
            Resources.Add(myKey, ScanLineInfo);
        }
    }

    /// <summary>
    /// A <see cref="KidPixResource"/> that contains a set of <see cref="BMPResource"/> packed together 
    /// intended to be used with animated objects
    /// </summary>
    public class BMHResource : KidPixResource, IPaintable
    {
        private Stream _fileStream;

        /// <summary>
        /// The image stream that this resource is streaming its data from
        /// </summary>
        public Stream ImageStream { get; }

        /// <summary>
        /// The currently selected <see cref="BMHFrameInfo"/> which will be used by subsequent calls to <see cref="Paint()"/>
        /// </summary>
        public BMHFrameInfo SelectedResource { get; private set; }

        public BMPHeader Header { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="BMHResource"/> class
        /// </summary>
        /// <param name="FileStream"></param>
        /// <param name="table"></param>
        /// <param name="ParentEntry"></param>
        public BMHResource(Stream FileStream, BMHTable table, ResourceTableEntry ParentEntry) : base(ParentEntry)
        {
            Table = table;
            _fileStream = FileStream;

            SetCurrentResource(0);
        }

        public BMHTable Table { get; }

        public ColorPalette? Palette { get; set; }

        public byte[] ReadFrameData(int ResourceID)
        {
            var frame = Table[ResourceID];
            return ReadFrameData(frame);
        }
        public byte[] ReadFrameData(BMHFrameInfo Info)
        {
            var frame = Info;
            _fileStream.Seek(frame.Offset, SeekOrigin.Begin);
            byte[] rawBytes = new byte[frame.Length];      
            _fileStream.ReadExactly(rawBytes,0,rawBytes.Length);
            return rawBytes;
        }

        /// <summary>
        /// Uses the <see cref="MHWKBitmapImporter"/> to convert the given <paramref name="FrameInfo"/> data to a <see cref="BMPResource"/>
        /// </summary>
        /// <param name="FrameInfo"></param>
        /// <returns><see cref="BMPResource"/></returns>
        public BMPResource? ImportFrame(BMHFrameInfo FrameInfo) => 
            MHWKResourceImporterBase.GetDefaultImporter(CHUNK_TYPE.tBMP).
            Import(new MemoryStream(ReadFrameData(FrameInfo)), ParentEntry) as BMPResource;

        public void SetCurrentResource(int ResourceID) => SelectedResource = Table[ResourceID];

        /// <summary>
        /// Paints this <see cref="BMHResource"/> using the <see cref="SelectedResource"/> property which you can change using the 
        /// <see cref="SetCurrentFrame(int, int)"/> method
        /// <para/>Note: Each time this function is called, a new <see cref="Bitmap"/> is created, please destroy it when done.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Bitmap Paint()
        {
            var frame = SelectedResource;
            if (!MHWKBitmapImporter.AttemptReadMohawkBitmap(new MemoryStream(ReadFrameData(frame)), out BMPHeader? header, out byte[] rawData) ||
                header == null || rawData == null)
                throw new InvalidDataException("Could not render this resource.");
            Header = header;
            return BMPBrush.Plaster(Header, rawData, Palette);
        }

        public override void Dispose()
        {
            _fileStream.Dispose();
        }
    }
}
