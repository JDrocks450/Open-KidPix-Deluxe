using System.Drawing;
using KidPix.API.Util;

namespace KidPix.API.Importer.Graphics.Brushes
{
    //////////////////////////////////////////
    // LZ Unpacker -- https://github.com/scummvm/scummvm/blob/master/engines/mohawk/bitmap.cpp#L182
    // Translated to C# by me (bisquick) (JDrocks450)
    //////////////////////////////////////////
    /// <summary>
    /// A decompressor that will decompress a LZ-compressed data stream for use in Mohawk games published by Br0derbund
    /// <para/>See: <see href="https://insidethelink.ortiche.net/wiki/index.php/Mohawk_Bitmaps"/>
    /// <para/>Original C++ implementation can be found here: <see href="https://github.com/scummvm/scummvm/blob/master/engines/mohawk/bitmap.cpp#L182"/>
    /// </summary>
    internal static class BMPLZDecompressor
    {
        const int LEN_BITS = 6;
        const int MIN_STRING = 3;                                   // lower limit for string length
        const int POS_BITS = 16 - LEN_BITS;
        const int MAX_STRING = (1 << LEN_BITS) + MIN_STRING - 1;  // upper limit for string length
        const int CBUFFERSIZE = 1 << POS_BITS;                    // size of the circular buffer
        const int POS_MASK = CBUFFERSIZE - 1;

        public static Stream? Decompress(BMPHeader Header, EndianBinaryReader reader)
        {
            uint uncompressedSize = reader.ReadUInt32();
            /* uint compressedSize = */
            reader.ReadUInt32();
            ushort dictSize = reader.ReadUInt16();

            // We only support the buffer size of 0x400
            if (dictSize != CBUFFERSIZE)
                throw new NotImplementedException("Unsupported dictionary size of " + dictSize.ToString("X4"));

            // Now go and decompress the data
            return DecompressLZ(reader, (int)uncompressedSize);
        }

        private static unsafe Stream DecompressLZ(EndianBinaryReader Reader, int uncompressedSize)
        {
            Stream stream = Reader.BaseStream;

            ushort flags = 0;
            uint bytesOut = 0;
            ushort insertPos = 0;

            // Expand the output buffer to at least the ring buffer size
            uint outBufSize = (uint)Math.Max(uncompressedSize, CBUFFERSIZE);
            byte[] arrayOutput = new byte[outBufSize];
            fixed (byte* outputData = &arrayOutput[0])
            {
                byte* dst = outputData;
                byte* buf = dst;

                while (stream.Position < stream.Length)
                {
                    flags >>= 1;

                    if ((flags & 0x100) == 0)
                        flags = (ushort)(Reader.ReadByte() | 0xff00);

                    if ((flags & 1) != 0)
                    {
                        if (++bytesOut > uncompressedSize)
                            break;
                        *dst++ = Reader.ReadByte();
                        if (++insertPos > POS_MASK)
                        {
                            insertPos = 0;
                            buf += CBUFFERSIZE;
                        }
                    }
                    else
                    {
                        ushort offLen = Reader.ReadUInt16();
                        ushort stringLen = (ushort)((offLen >> POS_BITS) + MIN_STRING);
                        ushort stringPos = (ushort)(offLen + MAX_STRING & POS_MASK);

                        bytesOut += stringLen;
                        if (bytesOut > uncompressedSize)
                            stringLen -= (ushort)(bytesOut - uncompressedSize);

                        byte* strPtr = buf + stringPos;
                        if (stringPos > insertPos)
                        {
                            if (bytesOut >= CBUFFERSIZE)
                                strPtr -= CBUFFERSIZE;
                            else if (stringPos + stringLen > POS_MASK)
                            {
                                for (ushort k = 0; k < stringLen; k++)
                                {
                                    *dst++ = *strPtr++;
                                    if (++stringPos > POS_MASK)
                                    {
                                        stringPos = 0;
                                        strPtr = outputData;
                                    }
                                }
                                insertPos = (ushort)(insertPos + stringLen & POS_MASK);
                                if (bytesOut >= uncompressedSize)
                                    break;
                                continue;
                            }
                        }

                        insertPos += stringLen;

                        if (insertPos > POS_MASK)
                        {
                            insertPos &= POS_MASK;
                            buf += CBUFFERSIZE;
                        }

                        for (ushort k = 0; k < stringLen; k++)
                            *dst++ = *strPtr++;

                        if (bytesOut >= uncompressedSize)
                            break;
                    }
                }
            }

#if false
            int flags = 0;
            long bytesOut = 0;
            long insertPos = 0;

            // Expand the output buffer to at least the ring buffer size
            int outBufSize = Math.Max(uncompressedSize, CBUFFERSIZE);

            byte[] outputData = new byte[outBufSize];
            byte[] buf = outputData;
            int dstIndex = 0;  // Using an index to simulate pointer movement
            int bufIndex = 0;  // Buffer index to simulate pointer movement

            while (stream.Position < stream.Length)
            {
                flags >>= 1;

                if ((flags & 0x100) == 0)
                {
                    flags = Reader.ReadByte() | 0xff00;
                }

                if ((flags & 1) != 0)
                {
                    if (++bytesOut > uncompressedSize)
                        break;

                    outputData[dstIndex++] = Reader.ReadByte();

                    if (++insertPos > POS_MASK)
                    {
                        insertPos = 0;
                        buf = new byte[CBUFFERSIZE];
                        bufIndex = 0;
                    }
                }
                else
                {
                    ushort offLen = Reader.ReadUInt16();
                    long stringLen = (offLen >> POS_BITS) + MIN_STRING;
                    ushort stringPos = (ushort)((offLen + MAX_STRING) & POS_MASK);

                    bytesOut += stringLen;

                    if (bytesOut > uncompressedSize)
                    {
                        stringLen -= bytesOut - uncompressedSize;
                    }

                    byte[] strPtr = new byte[stringLen];
                    Array.Copy(buf, stringPos, strPtr, 0, stringLen);

                    if (stringPos > insertPos)
                    {
                        if (bytesOut >= CBUFFERSIZE)
                            strPtr = new byte[CBUFFERSIZE];

                        else if (stringPos + stringLen > POS_MASK)
                        {
                            for (ushort k = 0; k < stringLen; k++)
                            {
                                outputData[dstIndex++] = strPtr[k];
                            }
                            insertPos = (ushort)((insertPos + stringLen) & POS_MASK);

                            if (bytesOut >= uncompressedSize)
                                break;

                            continue;
                        }
                    }

                    insertPos += stringLen;

                    if (insertPos > POS_MASK)
                    {
                        insertPos &= POS_MASK;
                        buf = new byte[CBUFFERSIZE];
                        bufIndex = 0;
                    }

                    for (ushort k = 0; k < stringLen; k++)
                    {
                        outputData[dstIndex++] = strPtr[k];
                    }

                    if (bytesOut >= uncompressedSize)
                        break;
                }
            }
#endif
            return new MemoryStream(arrayOutput);
        }
    }
}
