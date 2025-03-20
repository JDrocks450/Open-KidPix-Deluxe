using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;

namespace KidPix.API.Importer.tBMP.Decompressor
{
    public class BMPBrush
    {
        public BMPBrush(BMPHeader Header) => this.Header = Header;
        public BMPBrush(BMPHeader Header, byte[] RawData) : this(Header) => this.RawData = RawData;

        public BMPHeader Header { get; }
        public byte[] RawData { get; }

        public Bitmap? Plaster() => Plaster(Header, RawData);

        public virtual Bitmap? Paint() => Plaster();

        public static Bitmap? Plaster(BMPHeader Header, Stream Bytes)
        {
            byte[] array = new byte[Bytes.Length - Bytes.Position];
            Bytes.ReadExactly(array);
            return Plaster(Header, array);
        }
        public static Bitmap? Plaster(BMPHeader Header, byte[] RawData)
        {
            PixelFormat Format = Header.BitDepthDescription switch
            {
                BitmapFormat.kBitsPerPixel16 => PixelFormat.Format16bppRgb565,
                BitmapFormat.kBitsPerPixel8 => PixelFormat.Format8bppIndexed,
                BitmapFormat.kBitsPerPixel24 => PixelFormat.Format24bppRgb,
            };

            var bmp = new Bitmap(Header.Width, Header.Height, Format);

            int expectedSize = Header.BytesPerRow * Header.Height;
            if (expectedSize > RawData.Length)
                Array.Resize(ref RawData, expectedSize);

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0,bmp.Width,bmp.Height),ImageLockMode.WriteOnly,Format);

            IntPtr pNative = bmpData.Scan0;
            Marshal.Copy(RawData, 0, pNative, expectedSize);
            bmp.UnlockBits(bmpData);

            return bmp;
        }
    }
}
