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
    /// <summary>
    /// Imports graphics resources from: <see cref="CHUNK_TYPE.tBMH"/>, <see cref="CHUNK_TYPE.tBMP"/> or other recognized graphics chunks
    /// </summary>
    [MHWKImporterAttribute(CHUNK_TYPE.tBMP)]
    public class MHWKRasterImporter : MHWKResourceImporterBase
    {
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
            bool decompressed = false;
            switch (compression)
            {
                case BitmapPackCompression.kPackNone:                    
                    decompressedImageStream = reader.BaseStream;
                    break;
                case BitmapPackCompression.kPackLZ:
                    decompressedImageStream = BMPLZDecompressor.Decompress(Header, reader);
                    decompressed = true;
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
                    Resource = BMPRLE16Brush.Paint(Header, decompressedImageStream, Endianness.BigEndian);
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
            if (decompressed)
                decompressedImageStream?.Dispose(); // CHANGE LATER -- we should replace the compressed image stream passed into the import method with this one
            return Resource != null;
        }

        public override KidPixResource? Import(Stream Stream, ResourceTableEntry ParentEntry)
        {
            EndianBinaryReader reader = new(Stream);

            AttemptGeneralMohawkBitmap(reader, out BMPHeader? header, out Bitmap? resource);
            Stream.Seek(0, SeekOrigin.Begin);
            return new BMPResource(ParentEntry, header) { BitmapImage = resource, ImageStream = Stream };
        }
    }
}    

