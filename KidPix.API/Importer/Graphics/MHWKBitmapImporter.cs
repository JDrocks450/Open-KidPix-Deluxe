using KidPix.API.Importer.Graphics.Brushes;
using KidPix.API.Importer.Mohawk;
using KidPix.API.Importer.tBMP.Decompressor;
using KidPix.API.Util;
using System.Reflection.PortableExecutable;

namespace KidPix.API.Importer.Graphics
{

    /// <summary>
    /// Imports graphics resources from: <see cref="CHUNK_TYPE.tBMH"/>, <see cref="CHUNK_TYPE.tBMP"/> or other recognized graphics chunks
    /// </summary>
    [MHWKImporter(CHUNK_TYPE.tBMP)]
    public class MHWKBitmapImporter : MHWKResourceImporterBase
    {
        public static BMPHeader ReadBMPHeader(Stream Stream) => ReadBMPHeader(new EndianBinaryReader(Stream));
        public static BMPHeader ReadBMPHeader(EndianBinaryReader reader) => 
            new BMPHeader(
                Width: reader.ReadUInt16() & 0x3FFF,
                Height: reader.ReadUInt16() & 0x3FFF,
                BytesPerRow: reader.ReadUInt16() & 0x3FFE,
                Format: reader.ReadUInt16()
            );

        internal static bool AttemptReadMohawkBitmap(Stream ResourceDataStream, out BMPHeader? Header, out byte[] ResourceBytes)
        {
            ResourceBytes = null;
            Header = null;
            if (ResourceDataStream.Length < BMPHeader.BMPHEADER_SIZE) return false;

            EndianBinaryReader reader = new(ResourceDataStream);            
            var header = Header = ReadBMPHeader(reader);

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

            //paint
            BitmapDrawCompression drawCompression = header.DrawAlgorithm;
            switch (drawCompression)
            {
                case BitmapDrawCompression.kDrawRLE:
                    new BMPRLE16Brush(Header, decompressedImageStream, Endianness.BigEndian).GetImageDataBytes(ref ResourceBytes);
                    break;
                case BitmapDrawCompression.kDrawRLE8:
                    new BMPRLE8Brush(Header, decompressedImageStream, Endianness.BigEndian).GetImageDataBytes(ref ResourceBytes);
                    break;
                case BitmapDrawCompression.kDrawRaw:
                    if (Header.BitsPerPixel == 24) throw new NotImplementedException("24bpp crashes with a memory exception, I have no idea right now.");
                    ResourceBytes = new byte[header.BytesPerRow * header.Height];
                    decompressedImageStream.ReadExactly(ResourceBytes);
                    break;
                default:
                    throw new Exception("Unknown draw routine: " + drawCompression);
            }
            if (decompressed) // a new stream was created
                decompressedImageStream.Dispose();
            return ResourceBytes != null;
        }

        /// <summary>
        /// Imports the given resource stream encoded as a <see cref="CHUNK_TYPE.tBMP"/> into a <see cref="BMPResource"/>
        /// and closes the given <paramref name="Stream"/>
        /// </summary>
        /// <param name="Stream"></param>
        /// <param name="ParentEntry"></param>
        /// <returns></returns>
        public override KidPixResource? Import(Stream Stream, ResourceTableEntry ParentEntry)
        {           
            AttemptReadMohawkBitmap(Stream, out BMPHeader? header, out byte[] resource);
            Stream.Dispose(); // Save on resources, this stream is now closed as it is used up            
            return new BMPResource(ParentEntry, header, resource);
        }
    }
}

