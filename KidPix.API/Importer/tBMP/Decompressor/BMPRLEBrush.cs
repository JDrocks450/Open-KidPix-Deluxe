using System.Drawing;
using KidPix.API.Util;

namespace KidPix.API.Importer.tBMP.Decompressor
{
    public partial class BMPRLEBrush : BMPBrush
    {
        public static int DEBUG_MAX_COMMANDS = 1;
        public static int DEBUG_MAX_LINES = 1;
        public static Dictionary<int, List<RLEDrawCall>> DEBUG_DrawCallsByRow = new();
        public static RLEDrawCall? DEBUG_LAST_CALL;

        public Stream? CompressedImageDataStream { get; }
        public Endianness Endian { get; } = Endianness.BigEndian;

        public BMPRLEBrush(BMPHeader Header) : base(Header) { }
        public BMPRLEBrush(BMPHeader Header, Stream CompressedImageDataStream, Endianness Endian = Endianness.BigEndian) : this(Header)
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
            //DIAGNOSTIC
            Dictionary<int, List<RLEDrawCall>> drawCallsByRow = DEBUG_DrawCallsByRow = new();
            int DEBUG_DrawCalls = 0;

            var _data = ImageData;
            var _header = Header;
            EndianBinaryReader reader = new(_data);

            int size = Header.BytesPerRow * Header.Height;
            byte[] rawData = new byte[size];
            int scanline = 0;

            while (ImageData.Position < ImageData.Length)
            {
                if (DEBUG_DrawCalls > DEBUG_MAX_COMMANDS) break;

                List<RLEDrawCall> drawCalls = new();
                drawCallsByRow.Add(scanline, drawCalls);

                int rawDataIndex = (Header.BytesPerRow * scanline);
                int readBytes = 0;

                //BEGIN READING SCAN LINE
                ushort SCAN_SIZE = reader.ReadUInt16();                
                ushort PARAM0 = reader.ReadUInt16();
                long position = reader.BaseStream.Position;
                //The first number is the size in bytes, interpreted as 4 * number + 2
                int scanLineSizeBytes = SCAN_SIZE * 4;

                while (readBytes < scanLineSizeBytes)
                {                    
                    if (DEBUG_DrawCalls > DEBUG_MAX_COMMANDS) break;

                    if (rawDataIndex > (Header.BytesPerRow * (scanline + 1)))
                        ;

                    RLEDrawCall drawCall = DEBUG_LAST_CALL = new(ImageData.Position);

                    byte OPCODE1 = drawCall.OPCODE1 = reader.ReadByte();
                    byte OPCODE2 = drawCall.OPCODE2 = reader.ReadByte();
                    readBytes += 2 * sizeof(byte);
                    if (OPCODE1 == 0 && OPCODE2 == 0) continue;

                    DEBUG_DrawCalls++;
                    drawCalls.Add(drawCall);                     

                    ushort OPCODEPARAM1 = drawCall.OPCODEPARAM1 = reader.ReadUInt16();
                    byte PIXELVAL1 = drawCall.PIXELVAL1 = reader.ReadByte();
                    byte PIXELVAL2 = drawCall.PIXELVAL2 = reader.ReadByte();
                    byte PIXELVAL3 = drawCall.PIXELVAL3 = reader.ReadByte();
                    byte PIXELVAL4 = drawCall.PIXELVAL4 = reader.ReadByte();
                    readBytes += (1 * sizeof(ushort)) + (4 * sizeof(byte));

                    //write to output image
                    rawData[rawDataIndex++] = PIXELVAL1;
                    rawData[rawDataIndex++] = PIXELVAL2;
                    rawData[rawDataIndex++] = PIXELVAL3;
                    rawData[rawDataIndex++] = PIXELVAL4;
                    //**                    

                    if (OPCODE1 == 0x80)
                    {
                        if (scanLineSizeBytes > readBytes) // still more data to go
                        {
                            if (reader.ReadByte() != 0)
                            {
                                reader.BaseStream.Seek(-1, SeekOrigin.Current);
                                continue;
                            }
                            reader.BaseStream.Seek(-1, SeekOrigin.Current);
                            //the amount of bytes multiplied by 2 until the next draw call
                            ushort COPYBYTES = drawCall.COPYBYTES = reader.ReadUInt16();
                            ushort CPYPARAM0 = drawCall.CPYPARAM0 = reader.ReadUInt16();
                            readBytes += 4;
                            int copyBytesAmt = COPYBYTES * 2;
                            if (copyBytesAmt > (scanLineSizeBytes - readBytes))
                                ;
                            if (copyBytesAmt == 0)
                            {

                            }
                            drawCall.COPIED_BYTES = new byte[copyBytesAmt];
                            reader.BaseStream.Read(rawData, rawDataIndex, copyBytesAmt);
                            reader.BaseStream.Seek(-copyBytesAmt, SeekOrigin.Current);
                            reader.BaseStream.Read(drawCall.COPIED_BYTES, 0, copyBytesAmt);
                            rawDataIndex += copyBytesAmt;
                            readBytes += copyBytesAmt;
                        }
                        else
                        {
                            var hits = drawCalls.Where(x => x != drawCall && x.OPCODE1 == 0x80 && x.OPCODE2 == OPCODE2 && x.COPIED_BYTES != null);
                            if (hits.Any())
                            {
                                var src = hits.First().COPIED_BYTES;
                                Array.Copy(src, 0, rawData, rawDataIndex, src.Length);
                                rawDataIndex += src.Length;
                                drawCall.COPIED_BYTES = src;
                                continue;
                            }
                        }
                    }
                }
                scanline++;
                readBytes = 0;

                reader.BaseStream.Seek(position + scanLineSizeBytes, SeekOrigin.Begin);
            }
            IEnumerable<byte> opcodes = drawCallsByRow.SelectMany(x => x.Value).Select(y => y.OPCODE1).Distinct();
            return rawData;
        }        

        private static byte[] BrushOLD(BMPHeader Header, Stream ImageData, Endianness Endian)
        {
            var _data = ImageData;
            var _header = Header;
            EndianBinaryReader _reader = new(_data);

            int size = _header.BytesPerRow * _header.Height;
            byte[] rawData = new byte[size];

            for (ushort i = 0; i < _header.Height; i++)
            {
                uint rowByteCount = (uint)(_reader.ReadUInt16(Endian) * 4) + 2;
                long startPos = _data.Position;
                int rawDataIndex = (Header.BytesPerRow * i);
                ushort remaining = (ushort)_header.Width;

                while (remaining > 0)
                {
                    ushort runLen = _reader.ReadUInt16();
                    byte code = _reader.ReadByte();                    

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
                _data.Seek(startPos + rowByteCount, SeekOrigin.Begin);
            }
            return rawData;
        }
    }
}
