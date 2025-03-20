using KidPix.API.Importer.Mohawk;
using KidPix.API.Importer.tBMP.Decompressor;
using KidPix.API.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KidPix.API.Importer.tBMP
{
    public enum BitmapDrawCompression : ushort
    {
        kDrawMASK = 0x00f0,
        kDrawRaw = 0x0000,
        kDrawRLE8 = 0x0010,
        kDrawMSRLE8 = 0x0020,
        kDrawRLE = 0x0030,
    }

    public enum BitmapPackCompression : ushort
    {
        kPackMASK = 0x0f00,
        kPackNone = 0x0000,
        kPackLZ = 0x0100,
        kPackLZ1 = 0x0200,
        kPackRiven = 0x0400,
        kPackXDec = 0x0f00,
    }

    public enum BitmapFormat : ushort
    {
        kBitsPerPixel1 = 0x0000,
        kBitsPerPixel4 = 0x0001,
        kBitsPerPixel8 = 0x0002,
        kBitsPerPixel16 = 0x0003,
        kBitsPerPixel24 = 0x0004,
        kBitsPerPixelMask = 0x0007,
        kBitmapHasCLUT = 0x0008,
        kDrawMASK = 0x00f0,
        kDrawRaw = 0x0000,
        kDrawRLE8 = 0x0010,
        kDrawMSRLE8 = 0x0020,
        kDrawRLE = 0x0030,
        kPackMASK = 0x0f00,
        kPackNone = 0x0000,
        kPackLZ = 0x0100,
        kPackLZ1 = 0x0200,
        kPackRiven = 0x0400,
        kPackXDec = 0x0f00,
        kFlagMASK = 0xf000,
        kFlag16_80X86 = 0x1000, // 16 bit pixel data has been converted to 80X86 format
        kFlag24_MAC = 0x1000 // 24 bit pixel data has been converted to MAC 32 bit format
    };

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

    /// <summary>
    /// A <see cref="KidPixResource"/> that contains Raster Image data
    /// </summary>
    public class BMPResource : KidPixResource, IDisposable
    {
        public BMPResource(ResourceTableEntry ParentEntry, BMPHeader Header) : base(ParentEntry) => this.Header = Header;
        public BMPResource(ResourceTableEntry ParentEntry, BMPHeader Header, Bitmap Image) : this(ParentEntry, Header) => BitmapImage = Image;

        public BMPHeader Header { get; }
        public Bitmap? BitmapImage { get; internal set; } = null;

        public void Dispose()
        {
            BitmapImage?.Dispose();
            BitmapImage = null;
        }
    }

    /// <summary>
    /// Imports graphics resources from: <see cref="CHUNK_TYPE.tBMH"/>, <see cref="CHUNK_TYPE.tBMP"/> or other recognized graphics chunks
    /// </summary>
    public static class MHWKRasterImporter
    {
        public static KidPixResource? Import(Stream Stream, ResourceTableEntry Entry)
        {
            EndianBinaryReader reader = new(Stream);

            AttemptGeneralMohawkBitmap(reader, out BMPHeader? header, out Bitmap? resource);
            return new BMPResource(Entry, header) { BitmapImage = resource };
        }

        private static bool AttemptGeneralMohawkBitmap(EndianBinaryReader reader, out BMPHeader? Header, out Bitmap? Resource)
        {
            Resource = null;
            BMPHeader header = Header = new BMPHeader(
                Width: reader.ReadUInt16() & 0x3FFF,
                Height: reader.ReadUInt16() & 0x3FFF,
                BytesPerRow: reader.ReadUInt16() & 0x3FFE,
                Format: reader.ReadUInt16()
            );

            //decompress
            Stream? decompressedImageStream = null;
            BitmapPackCompression compression = header.CompressionAlgorithm;            
            switch (compression)
            {
                case BitmapPackCompression.kPackNone:                    
                    decompressedImageStream = reader.BaseStream;
                    break;
                case BitmapPackCompression.kPackLZ:
                    decompressedImageStream = BMPLZDecompressor.Decompress(Header, reader);
                    break;
                default: return false;
            }
            if (decompressedImageStream == null) return false;

            long pos = decompressedImageStream.Position;

            //**DEBUG
            FileStream fs = File.Create("decompressDump.bin");
            decompressedImageStream.Seek(0, SeekOrigin.Begin);
            decompressedImageStream.CopyTo(fs);
            decompressedImageStream.Seek(pos, SeekOrigin.Begin);
            fs.Dispose();
            //**

            //paint
            BitmapDrawCompression drawCompression = header.DrawAlgorithm;
            switch (drawCompression)
            {
                case BitmapDrawCompression.kDrawRLE:
                    Resource = BMPRLEBrush.Paint(Header, decompressedImageStream, Endianness.BigEndian);
                    break;
                case BitmapDrawCompression.kDrawRLE8:
                    Resource = BMPRLE8Brush.Paint(Header, decompressedImageStream, Endianness.BigEndian);
                    break;
                case BitmapDrawCompression.kDrawRaw:                    
                    Resource = BMPBrush.Plaster(Header, decompressedImageStream);
                    break;
                default:
                    throw new Exception("Unknown draw routine: " + drawCompression);                    
            }
            decompressedImageStream?.Dispose();
            return Resource != null;
        }
    }
}    

