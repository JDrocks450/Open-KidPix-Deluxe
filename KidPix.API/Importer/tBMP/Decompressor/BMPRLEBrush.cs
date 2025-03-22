using System.Drawing;
using KidPix.API.Util;

namespace KidPix.API.Importer.tBMP.Decompressor
{
    /// <summary>
    /// A Broderbund Mohawk Engine RLE16 Brush
    /// <para/>RLE16 is an improvement on their <see cref="BMPRLE8Brush"/> which was a very simple 8bpp Indexed Run Length Encoding strategy.
    /// <para/>It is more advanced making use of commands that condense repeated colors into one call, then have a stream of color bytes up until the next draw call.
    /// </summary>
    public partial class BMPRLE16Brush : BMPBrush
    {
        public static int DEBUG_MAX_COMMANDS = 1;
        public static bool DEBUG_RUNNING_UNTIL_NEXT_SCAN = false;
        public static Dictionary<int, List<RLEDrawCall>> DEBUG_DrawCallsByRow = new();
        public static RLEDrawCall? DEBUG_LAST_CALL;
        public static Exception? DEBUG_LAST_ERROR;

        public Stream? CompressedImageDataStream { get; }
        public Endianness Endian { get; } = Endianness.BigEndian;

        public BMPRLE16Brush(BMPHeader Header) : base(Header) { }
        public BMPRLE16Brush(BMPHeader Header, Stream CompressedImageDataStream, Endianness Endian = Endianness.BigEndian) : this(Header)
        {
            this.CompressedImageDataStream = CompressedImageDataStream;
            this.Endian = Endian;
        }

        public override Bitmap? Paint() => Paint(Header, CompressedImageDataStream, Endian);

        public static Bitmap? Paint(BMPHeader Header, Stream ImageData, Endianness Endian)
        {
            DEBUG_LAST_ERROR = null;
            if (ImageData == null) throw new NullReferenceException(nameof(ImageData));
            byte[] rawData = new byte[0];
            try
            {
                Brush(Header, ImageData, Endian, ref rawData);
            }
            catch(Exception e)
            {
                DEBUG_LAST_ERROR = e;
            }
            return Plaster(Header, rawData);
        }

        /// <summary>
        /// Perform RLE16 routine (decompression)
        /// <para/>This function will render the given RLE16 image data stream into the provided <paramref name="Output"/>
        /// <para/>Since <paramref name="Output"/> is passed by reference, it will be overwritten with new data.
        /// </summary>
        /// <param name="Header"></param>
        /// <param name="ImageData"></param>
        /// <param name="Endian"></param>
        /// <param name="Output"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private static void Brush(BMPHeader Header, Stream ImageData, Endianness Endian, ref byte[] Output)
        {
            //DIAGNOSTIC
            Dictionary<int, List<RLEDrawCall>> drawCallsByRow = DEBUG_DrawCallsByRow = new();
            int DEBUG_DrawCalls = 0;

            var _data = ImageData;
            var _header = Header;
            EndianBinaryReader reader = new(_data);

            //set the output image dimensions and initialize new byte array that will be the final output image before Plaster();
            int size = Header.BytesPerRow * Header.Height;
            byte[] rawData = Output = new byte[size];
            int scanline = 0; // -- the scanline we're currently on (bmp.Height is max)

            while (ImageData.Position < ImageData.Length)
            {
                //DEBUG - halt to show progress and learn format of this compression algorithm
                if (DEBUG_DrawCalls > DEBUG_MAX_COMMANDS && !DEBUG_RUNNING_UNTIL_NEXT_SCAN) break;

                List<RLEDrawCall> drawCalls = new();
                drawCallsByRow.Add(scanline, drawCalls);

                //The index in the Output BMP we're currently at (set this initially to be start of new Scan Line in Output BMP)
                int rawDataIndex = (Header.BytesPerRow * scanline);
                //the amount of bytes we've read in this scanline
                int readBytes = 0;

                //BEGIN READING SCAN LINE

                //indicates the size of the scanline
                ushort SCAN_SIZE = reader.ReadUInt16();      // the amount of "blocks" -- you multiply this number by 4 for byte amt of scanline           
                ushort PARAM0 = reader.ReadUInt16(); // unknown
                long scanPosition = reader.BaseStream.Position;
                //The first number is the size in bytes (after PARAM0), interpreted as 4 * number
                int scanLineSizeBytes = SCAN_SIZE * 4;

                while (readBytes < scanLineSizeBytes) // read until the end of the scanline
                {
                    //the amount of bytes we've read for this draw call
                    int localReadBytes = readBytes;

                    //DEBUG - halt to show progress and learn format of this compression algorithm
                    if (DEBUG_DrawCalls > DEBUG_MAX_COMMANDS && !DEBUG_RUNNING_UNTIL_NEXT_SCAN) break;

                    //Check if we're inside Output Scan Line
                    if (rawDataIndex > (Header.BytesPerRow * (scanline + 1)))
                        throw new InvalidOperationException("Our position in the finished image is past where it is allowed to be." +
                            $" {rawDataIndex} / {(Header.BytesPerRow * (scanline + 1))}");

                    RLEDrawCall drawCall = DEBUG_LAST_CALL = new(ImageData.Position); // make a new draw call for debug purposes

                    byte OPCODE = drawCall.OPCODE1 = reader.ReadByte(); // OPCODE 1 0x80 (10000000) is where is may split between one function or another
                    readBytes += 1 * sizeof(byte);
                    //If the OPCODE is below 0x80 -- this brush is unsupported.
                    if (OPCODE < 0x80) 
                        continue;

                    //The amount of times the proceeding pixel data is copied to the Output image
                    byte REPEAT_LENGTH = drawCall.OPCODE2 = reader.ReadByte(); 
                    readBytes += 1 * sizeof(byte);                    

                    //DEBUG--
                    DEBUG_DrawCalls++;
                    drawCalls.Add(drawCall);        
                    //**

                    ushort OPCODEPARAM1 = drawCall.OPCODEPARAM1 = reader.ReadUInt16(); // unknown
                    byte PIXELVAL2 = drawCall.PIXELVAL2 = reader.ReadByte(); // -- FLIP ENDIAN
                    byte PIXELVAL1 = drawCall.PIXELVAL1 = reader.ReadByte(); // This is the first output Pixel read
                    byte PIXELVAL4 = drawCall.PIXELVAL4 = reader.ReadByte(); // -- FLIP ENDIAN
                    byte PIXELVAL3 = drawCall.PIXELVAL3 = reader.ReadByte(); // This is the second output Pixel read (maybe supposed to be used for dithering?)
                    
                    readBytes += (1 * sizeof(ushort)) + (4 * sizeof(byte));

                    if (OPCODE == 0x80)
                    {
                        for (int loops = 0; loops < REPEAT_LENGTH; loops++)
                        {
                            //RLE Pixel 1
                            rawData[rawDataIndex++] = PIXELVAL1; 
                            rawData[rawDataIndex++] = PIXELVAL2;
                        }
                        //**
                    }
                    else if (OPCODE == 0x81)
                    {
                        for (int loops = 0; loops < REPEAT_LENGTH * 2; loops++)
                        {
                            //Dithering pixel 1
                            rawData[rawDataIndex++] = PIXELVAL1; 
                            rawData[rawDataIndex++] = PIXELVAL2;
                            //Dithering pixel 2
                            rawData[rawDataIndex++] = PIXELVAL3;
                            rawData[rawDataIndex++] = PIXELVAL4;
                        }
                    }
                    else
                    {
                        throw new Exception("Break point");
                    }

                    //Copy proceeding data bytes command
                    if (OPCODE == 0x80)
                    {
                        if (scanLineSizeBytes > readBytes) // still more data to go in scan line, safe to proceed
                        {
                            if (reader.ReadByte() != 0)
                            {
                                reader.BaseStream.Seek(-1, SeekOrigin.Current);
                                goto SAFEEXIT;
                            }
                            reader.BaseStream.Seek(-1, SeekOrigin.Current);
                            //the amount of bytes multiplied by 2 until the next draw call
                            ushort COPYBYTES = drawCall.COPYBYTES = reader.ReadUInt16(); // could be a command stream... [byte] OPCODE [byte] byte out, no idea
                            ushort CPYPARAM0 = drawCall.CPYPARAM0 = reader.ReadUInt16();
                            readBytes += 4;
                            int copyBytesAmt = COPYBYTES * 2;
                            if (copyBytesAmt > (scanLineSizeBytes - readBytes))
                                throw new InvalidOperationException($"The amount of bytes requested to copy: {copyBytesAmt} was longer than the scanline's available data: {scanLineSizeBytes}");
                            if (copyBytesAmt == 0)
                                throw new InvalidOperationException($"The amount of bytes requested to copy: {copyBytesAmt} was 0.");

                            drawCall.COPIED_BYTES = new byte[copyBytesAmt];
                            reader.BaseStream.Read(drawCall.COPIED_BYTES, 0, copyBytesAmt);
                            for (int i = 0; i < drawCall.COPIED_BYTES.Length - 1; i += 2)
                            {
                                rawData[rawDataIndex++] = drawCall.COPIED_BYTES[i + 1];
                                rawData[rawDataIndex++] = drawCall.COPIED_BYTES[i];
                            }
                            readBytes += copyBytesAmt;
                        }
                    }
                //safely continue onto the next loop while storing the drawcall length field
                SAFEEXIT:
                    drawCall.LENGTH = readBytes - localReadBytes;
                    continue;                
                }
                scanline++;
                readBytes = 0;

                reader.BaseStream.Seek(scanPosition + scanLineSizeBytes, SeekOrigin.Begin);

                //DEBUG
                if (DEBUG_RUNNING_UNTIL_NEXT_SCAN && DEBUG_DrawCalls > DEBUG_MAX_COMMANDS)
                {
                    //scan line completed, reset flag and set command amount to the amount of commands thus far + 1
                    DEBUG_RUNNING_UNTIL_NEXT_SCAN = false;
                    DEBUG_MAX_COMMANDS = DEBUG_DrawCalls + 1;
                }
            }
            
            IEnumerable<byte> opcodes = drawCallsByRow.SelectMany(x => x.Value).Select(y => y.OPCODE1).Distinct();            
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
