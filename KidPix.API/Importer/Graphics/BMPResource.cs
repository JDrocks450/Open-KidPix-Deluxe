using KidPix.API.Importer.Graphics.Brushes;
using KidPix.API.Importer.Mohawk;
using System.Drawing;

namespace KidPix.API.Importer.Graphics
{
    /// <summary>
    /// A <see cref="KidPixResource"/> that contains a <see cref="Bitmap"/>
    /// </summary>
    [KidPixResource(CHUNK_TYPE.tBMP)]
    public class BMPResource : KidPixResource, IDisposable, IStreamable, IPaintable
    {
        public BMPResource(ResourceTableEntry ParentEntry, BMPHeader Header, byte[] ImageData) : base(ParentEntry)
        {
            this.Header = Header;
            this.ImageData = ImageData;
            DataStream = new MemoryStream(ImageData);
        }

        public BMPHeader Header { get; }
        public byte[] ImageData { get; }
        public Stream DataStream { get; }

        /// <summary>
        /// Paints this <see cref="BMHResource"/>
        /// <para/>Please note a new <see cref="Bitmap"/> is created every time this is called, please destroy it once finished.
        /// </summary>
        /// <returns></returns>
        public Bitmap Paint() => BMPBrush.Plaster(Header, ImageData);        

        public override void Dispose()
        {
            DataStream.Dispose();
        }
    }
}

