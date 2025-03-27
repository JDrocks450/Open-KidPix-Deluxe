using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;

namespace KidPix.API.Importer.Graphics.Brushes
{
    /// <summary>
    /// A base class for brushes that has functionality for painting raw data to a <see cref="Bitmap"/>
    /// </summary>
    public abstract class BMPBrush
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
        public static Bitmap? Plaster(BMPHeader Header, byte[] RawData, ColorPalette? CustomPalette = null, bool ShouldFlipEndian = true)
        {
            if(Header.BitsPerPixel == 8)
            {
                Color[] arr = new Color[256];
                for (int i = 0; i < 256; i++)
                    arr[i] = Color.FromArgb(i, i, i);
                CustomPalette = new(arr);
            }

            PixelFormat Format = Header.BitDepthDescription switch
            {
                BitmapFormat.kBitsPerPixel16 => PixelFormat.Format16bppRgb555,
                BitmapFormat.kBitsPerPixel8 => PixelFormat.Format8bppIndexed,
                BitmapFormat.kBitsPerPixel24 => PixelFormat.Format24bppRgb,
            };

            if (Header.BitsPerPixel == 16)
                FlipEndian(ref RawData); // flip from Big to Little Endian

            var bmp = new Bitmap(Header.Width, Header.Height, Format);

            int expectedSize = Header.BytesPerRow * Header.Height;
            if (expectedSize != RawData.Length)
                Array.Resize(ref RawData, expectedSize);

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, Format);

            nint pNative = bmpData.Scan0;
            Marshal.Copy(RawData, 0, pNative, expectedSize);
            bmp.UnlockBits(bmpData);

            if (CustomPalette != null)
                bmp.Palette = CustomPalette;

            return bmp;
        }

        private static void FlipEndian(ref byte[] RawData)
        {
            byte[] destination = new byte[RawData.Length];
            for (int i = 0; i < RawData.Length - 1; i += 2)
            {
                destination[i] = RawData[i + 1];
                destination[i + 1] = RawData[i];
            }
            RawData = destination;
        }

        /// <summary>
        /// Renders the Image resource to the <paramref name="Output"/> parameter 
        /// <para/><i>Using ref parameter allows for the incomplete image to still be displayed for studying/research on error</i>
        /// </summary>
        /// <param name="Output"></param>
        /// <returns>true if completed without errors</returns>
        public abstract void GetImageDataBytes(ref byte[] Output);
    }
}
