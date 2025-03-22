namespace KidPix.API.Importer.tBMP
{
    /// <summary>
    /// Header information for this <see cref="BMPResource"/> object
    /// </summary>
    /// <param name="Width"></param>
    /// <param name="Height"></param>
    /// <param name="BytesPerRow"></param>
    /// <param name="Format"></param>
    public record class BMPHeader(int Width, int Height, int BytesPerRow, ushort Format)
    {
        public record class BMPColorTable(ushort TableSize, byte RgbBits, params byte[] Palette);        

        /// <summary>
        /// Gets the BitsPerPixel setting of this resource from the <see cref="Format"/> property
        /// </summary>
        public byte BitsPerPixel => BitDepthDescription switch
        {
            BitmapFormat.kBitsPerPixel1 =>  1,
            BitmapFormat.kBitsPerPixel4 =>  4,
            BitmapFormat.kBitsPerPixel8 =>  8,
            BitmapFormat.kBitsPerPixel16 => 16,
            BitmapFormat.kBitsPerPixel24 => 24,
            _ => throw new InvalidDataException("Unknown bits per pixel")                    
        };
        public BitmapFormat BitDepthDescription => (BitmapFormat)(Format & (ushort)BitmapFormat.kBitsPerPixelMask);
        /// <summary>
        /// Interprets the drawing algorithm from the <see cref="Format"/> property
        /// </summary>
        public BitmapDrawCompression DrawAlgorithm => (BitmapDrawCompression)(Format & (ushort)BitmapFormat.kDrawMASK);
        /// <summary>
        /// Interprets the compression algorithm from the <see cref="Format"/> property
        /// </summary>
        public BitmapPackCompression CompressionAlgorithm => (BitmapPackCompression)(Format & (ushort)BitmapPackCompression.kPackMASK);

        public BMPColorTable? ColorTable { get; internal set; } = null;
    }
}    

