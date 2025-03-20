using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using KidPix.API.Util;
using KidPix.API.Importer.tWAV;

namespace KidPix.API.Importer.tBMP.Decompressor
{

    public class BMPRLE8Brush : BMPBrush
    {
        public Stream? CompressedImageDataStream { get; }
        public Endianness Endian { get; } = Endianness.BigEndian;

        public BMPRLE8Brush(BMPHeader Header) : base(Header) { }
        public BMPRLE8Brush(BMPHeader Header, Stream CompressedImageDataStream, Endianness Endian = Endianness.BigEndian) : this(Header)
        {
            this.CompressedImageDataStream = CompressedImageDataStream;
            this.Endian = Endian;
        }

        public override Bitmap? Paint() => Paint(Header, CompressedImageDataStream, Endian);

        public static Bitmap? Paint(BMPHeader Header, Stream ImageData, Endianness Endian)
        {
            if (ImageData == null) throw new NullReferenceException(nameof(ImageData));
            byte[] rawData = Brush(Header, ImageData, Endian);
            return Plaster(Header, rawData);
        }        

        private static byte[] Brush(BMPHeader Header, Stream ImageData, Endianness Endian)
        {
            var _data = ImageData;
            var _header = Header;
            EndianBinaryReader _reader = new(_data);
            
            int size = _header.BytesPerRow * _header.Height;
            byte[] rawData = new byte[size];
            
            for (ushort i = 0; i < _header.Height; i++)
            {
                uint rowByteCount = _reader.ReadUInt16(Endian);
                long startPos = _data.Position;                                
                int rawDataIndex = (Header.BytesPerRow * i);
                ushort remaining = (ushort)_header.Width;

                while (remaining > 0)
                {
                    byte code = _reader.ReadByte();
                    ushort runLen = (ushort)((code & 0x7F) + 1);

                    if (runLen > remaining)
                        runLen = remaining;

                    if ((code & 0x80) != 0)
                    {
                        byte val = _reader.ReadByte();
                        Array.Fill(rawData, val, rawDataIndex, runLen);
                        
                    }
                    else
                    {
                        _data.Read(rawData, rawDataIndex, runLen);
                    }
                    rawDataIndex += runLen;
                    remaining -= runLen;
                }
                _data.Seek(startPos + rowByteCount,SeekOrigin.Begin);
            }
            return rawData;
        }
    }
}
