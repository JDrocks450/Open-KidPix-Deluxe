#define DEBUG_ENABLED
#undef DEBUG_ENABLED

using System.Drawing;
using KidPix.API.Importer.Graphics;
using KidPix.API.Importer.Graphics.Brushes;
using KidPix.API.Util;
using static KidPix.API.Importer.tBMP.Decompressor.BMPRLE16Brush;

namespace KidPix.API.Importer.tBMP.Decompressor
{
    public static class BMPRLE16BrushDebug
    {
        public static bool DEBUGGING_ENABLED =
#if DEBUG_ENABLED
            true;
#else 
            false;
#endif
        private static int SET_MAX_COMMANDS = /*DEBUGGING_ENABLED ? 1 :*/ int.MaxValue;


        private static int DEBUG_DrawCalls = 0;
        public static int DEBUG_MAX_COMMANDS = SET_MAX_COMMANDS;
        public static bool DEBUG_RUNNING_UNTIL_NEXT_SCAN = false;
        public static Dictionary<int, List<RLEDrawCall>> DEBUG_DrawCallsByRow = new();
        public static RLEDrawCall? DEBUG_LAST_CALL;
        public static Exception? DEBUG_LAST_ERROR;

        public static bool AssertCanContinue() => DEBUG_DrawCalls > DEBUG_MAX_COMMANDS && !DEBUG_RUNNING_UNTIL_NEXT_SCAN;
        public static void IncNewCall() => DEBUG_DrawCalls++;
        public static void ResetCalls() => DEBUG_DrawCalls=0;

        public static void AssertRowCompleted()
        {
            if (DEBUG_RUNNING_UNTIL_NEXT_SCAN && DEBUG_DrawCalls > DEBUG_MAX_COMMANDS)
            {
                //scan line completed, reset flag and set command amount to the amount of commands thus far + 1
                DEBUG_RUNNING_UNTIL_NEXT_SCAN = false;
                DEBUG_MAX_COMMANDS = DEBUG_DrawCalls + 1;
            }
        }

        public static void ClearSession()
        {
            DEBUG_DrawCalls = 0;
            DEBUG_LAST_ERROR = null;
            DEBUG_MAX_COMMANDS = SET_MAX_COMMANDS;
            DEBUG_DrawCallsByRow = new();
            DEBUG_LAST_CALL = null;
        }
    }

    /// <summary>
    /// A Broderbund Mohawk Engine RLE16 Brush
    /// <para/>RLE16 is an improvement on their <see cref="BMPRLE8Brush"/> which was a very simple 8bpp Indexed Run Length Encoding strategy.
    /// <para/>It is more advanced making use of commands that condense repeated colors into one call, then have a stream of color bytes up until the next draw call.
    /// </summary>
    public partial class BMPRLE16Brush : BMPBrush
    {       
        public Stream? CompressedImageDataStream { get; }
        public Endianness Endian { get; } = Endianness.BigEndian;

        public BMPRLE16Brush(BMPHeader Header) : base(Header) { }
        public BMPRLE16Brush(BMPHeader Header, Stream CompressedImageDataStream, Endianness Endian = Endianness.BigEndian) : this(Header)
        {
            this.CompressedImageDataStream = CompressedImageDataStream;
            this.Endian = Endian;
        }
        public override void GetImageDataBytes(ref byte[] Output) => Brush(Header, CompressedImageDataStream, Endian, ref Output);

        public override Bitmap? Paint() => Paint(Header, CompressedImageDataStream, Endian);

        public static Bitmap? Paint(BMPHeader Header, Stream ImageData, Endianness Endian)
        {
            BMPRLE16BrushDebug.ClearSession();
            if (ImageData == null) throw new NullReferenceException(nameof(ImageData));
            byte[] rawData = new byte[0];
            try
            {
                Brush(Header, ImageData, Endian, ref rawData);
            }
            catch(Exception e)
            {
                BMPRLE16BrushDebug.DEBUG_LAST_ERROR = e;
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
            Dictionary<int, List<RLEDrawCall>> drawCallsByRow = BMPRLE16BrushDebug.DEBUG_DrawCallsByRow = new();
            BMPRLE16BrushDebug.ResetCalls();

            var _data = ImageData;
            var _header = Header;
            EndianBinaryReader reader = new(_data);

            //set the output image dimensions and initialize new byte array that will be the final output image before Plaster();
            int size = Header.BytesPerRow * Header.Height;
            byte[] rawData = Output = new byte[size];
            int scanline = 0; // -- the scanline we're currently on (bmp.Height is max)

            while (ImageData.Position < ImageData.Length)
            {
                if (scanline > Header.Height) 
                    break;

                //DEBUG - halt to show progress and learn format of this compression algorithm
                if (BMPRLE16BrushDebug.AssertCanContinue()) break;

                List<RLEDrawCall> drawCalls = new();
                drawCallsByRow.Add(scanline, drawCalls);

                //The index in the Output BMP we're currently at (set this initially to be start of new Scan Line in Output BMP)
                int rawDataIndex = (Header.BytesPerRow * scanline);
                //the amount of bytes we've read in this scanline
                int readBytes = 0;                

                //BEGIN READING SCAN LINE

                //indicates the size of the scanline
                ushort SCAN_SIZE = reader.ReadUInt16();      // the amount of "blocks" -- you multiply this number by 4 for byte amt of scanline           
                ushort ADVANCE_BYTES = reader.ReadUInt16();
                rawDataIndex += ADVANCE_BYTES << 1;
                long scanPosition = reader.BaseStream.Position;
                //The first number is the size in bytes (after PARAM0), interpreted as 4 * number
                int scanLineSizeBytes = SCAN_SIZE * 4;

                while (readBytes < scanLineSizeBytes) // read until the end of the scanline
                {
                    //the amount of bytes we've read for this draw call
                    int localReadBytes = readBytes;

                    //DEBUG - halt to show progress and learn format of this compression algorithm
                    if (BMPRLE16BrushDebug.AssertCanContinue()) break;

                    //Check if we're inside Output Scan Line
                    if (rawDataIndex > (Header.BytesPerRow * (scanline + 1)))
                    {
                        //rawDataIndex = (Header.BytesPerRow * (scanline));
                        throw new InvalidOperationException("Our position in the finished image is past where it is allowed to be." +
                            $" {rawDataIndex} / {(Header.BytesPerRow * (scanline + 1))}");
                    }
                    if (rawDataIndex > rawData.Length)
                        break;

                    RLEDrawCall drawCall = BMPRLE16BrushDebug.DEBUG_LAST_CALL = new(ImageData.Position); // make a new draw call for debug purposes

                    byte OPCODE = drawCall.OPCODE1 = reader.ReadByte(); // OPCODE 1 0x80 (10000000) is where is may split between one function or another
                    readBytes += 1 * sizeof(byte);
                    //If the OPCODE is below 0x80 -- this brush is unsupported.

                    //The amount of times the proceeding pixel data is copied to the Output image
                    byte REPEAT_LENGTH = drawCall.OPCODE2 = reader.ReadByte(); 
                    readBytes += 1 * sizeof(byte);

                    if (OPCODE == 0x0 && REPEAT_LENGTH == 0x0) continue;

                    //DEBUG--
                    BMPRLE16BrushDebug.IncNewCall();
                    drawCalls.Add(drawCall);
                    //**

                    ushort OPCODEPARAM1 = drawCall.OPCODEPARAM1 = reader.ReadUInt16(); // unknown
                    readBytes += (1 * sizeof(ushort));

                    if (OPCODE == 0x0) // COPY DIRECT
                    {
                        int copyByteLength = REPEAT_LENGTH << 1;
                        if (copyByteLength > (scanLineSizeBytes - readBytes))
                            throw new InvalidOperationException($"The amount of bytes requested to copy: {copyByteLength} was longer than the scanline's available data: {scanLineSizeBytes}");
                        drawCall.COPIED_BYTES = reader.BaseStream.Read(rawData, rawDataIndex, copyByteLength);
                        rawDataIndex += copyByteLength;
                        drawCall.CREATED_PIXELS = copyByteLength / 2;
                        readBytes += copyByteLength;
                        goto SAFEEXIT;
                    }

                    byte PIXELVAL1 = drawCall.PIXELVAL1 = reader.ReadByte(); // This is the first output Pixel read
                    byte PIXELVAL2 = drawCall.PIXELVAL2 = reader.ReadByte(); // -- Endian should be flipped in the BMPBrush.Plaster();
                    byte PIXELVAL3 = drawCall.PIXELVAL3 = reader.ReadByte(); // This is the second output Pixel read (maybe supposed to be used for dithering?)
                    byte PIXELVAL4 = drawCall.PIXELVAL4 = reader.ReadByte(); // -- Endian should be flipped in the BMPBrush.Plaster();                   

                    readBytes += (4 * sizeof(byte));

                    if (OPCODE == 0x80)
                    {
                        for (int loops = 0; loops < REPEAT_LENGTH; loops++)
                        {
                            //RLE Pixel 1
                            rawData[rawDataIndex++] = PIXELVAL1; 
                            rawData[rawDataIndex++] = PIXELVAL2;
                            drawCall.CREATED_PIXELS += 1;
                        }                        
                    }
                    else if (OPCODE == 0x81)
                    {
                        int repeatLength = REPEAT_LENGTH;
                        repeatLength += 256;
                        for (int loops = 0; loops < repeatLength; loops++)
                        {
                            //Dithering pixel 1
                            rawData[rawDataIndex++] = PIXELVAL1; 
                            rawData[rawDataIndex++] = PIXELVAL2;
                            drawCall.CREATED_PIXELS += 1;
                        }
                    }
                    else
                    {
                        //throw new Exception("Break point");
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
                BMPRLE16BrushDebug.AssertRowCompleted();
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
