using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidPix.API.Util
{
    /// <summary>
    /// Allows functions to read a <see cref="Stream"/> in either Byte Order
    /// <para/> Does NOT need to be Disposed if you intend on continuing to use the <see cref="Stream"/>
    /// </summary>
    public class EndianBinaryReader : IDisposable
    {
        private readonly Stream stream;
        public Stream BaseStream => stream;
        public bool DisallowAdvance { get; set; } = false;

        public EndianBinaryReader(Stream Stream)
        {
            stream = Stream;
        }

        public void Dispose()
        {
            stream.Dispose();
        }

        /// <summary>
        /// Reads exactly 4 bytes from the <see cref="Stream"/> and converts it to a <see cref="UInt32"/> in the specified <paramref name="Endian"/>
        /// </summary>
        /// <param name="Endian"></param>
        /// <returns></returns>
        public uint ReadUInt32(Endianness Endian = Endianness.BigEndian)
        {
            byte[] data = new byte[sizeof(uint)];
            stream.ReadExactly(data, 0, sizeof(uint));
            if (DisallowAdvance) stream.Seek(-sizeof(uint), SeekOrigin.Current);
            return Endian == Endianness.BigEndian ? EndianBitConverter.Big.ToUInt32(data, 0) : EndianBitConverter.Little.ToUInt32(data, 0);
        }

        public ushort ReadUInt16(Endianness Endian = Endianness.BigEndian)
        {
            byte[] data = new byte[sizeof(ushort)];
            stream.ReadExactly(data, 0, sizeof(ushort));
            if (DisallowAdvance) stream.Seek(-sizeof(ushort), SeekOrigin.Current);
            return Endian == Endianness.BigEndian ? EndianBitConverter.Big.ToUInt16(data, 0) : EndianBitConverter.Little.ToUInt16(data, 0);
        }
        public byte ReadByte()
        {
            var b = (byte)stream.ReadByte();
            if (DisallowAdvance) stream.Seek(-sizeof(byte), SeekOrigin.Current);
            return b;
        }
        public sbyte ReadInt8()
        {
            var b = (sbyte)stream.ReadByte();
            if (DisallowAdvance) stream.Seek(-sizeof(sbyte), SeekOrigin.Current);
            return b;
        }

        public ulong ReadUInt64(Endianness Endian = Endianness.BigEndian)
        {
            byte[] data = new byte[sizeof(ulong)];
            stream.ReadExactly(data, 0, sizeof(ulong));
            if (DisallowAdvance) stream.Seek(-sizeof(ulong), SeekOrigin.Current);
            return Endian == Endianness.BigEndian ? EndianBitConverter.Big.ToUInt64(data, 0) : EndianBitConverter.Little.ToUInt64(data, 0);
        }
        public long ReadInt64(Endianness Endian = Endianness.BigEndian)
        {
            byte[] data = new byte[sizeof(long)];
            stream.ReadExactly(data, 0, sizeof(long));
            if (DisallowAdvance) stream.Seek(-sizeof(long), SeekOrigin.Current);
            return Endian == Endianness.BigEndian ? EndianBitConverter.Big.ToInt64(data, 0) : EndianBitConverter.Little.ToInt64(data, 0);
        }

        public string ReadCStr()
        {
            long pos = stream.Position;
            string currentEntry = "";
            byte c = ReadByte();
            while (c != 0)
            {
                currentEntry += (char)c;
                c = ReadByte();
            }
            stream.Seek(pos, SeekOrigin.Begin);
            return currentEntry;
        }

        public short ReadInt16(Endianness Endian = Endianness.BigEndian)
        {
            byte[] data = new byte[sizeof(short)];
            stream.ReadExactly(data, 0, sizeof(short));
            if (DisallowAdvance) stream.Seek(-sizeof(short), SeekOrigin.Current);
            return Endian == Endianness.BigEndian ? EndianBitConverter.Big.ToInt16(data, 0) : EndianBitConverter.Little.ToInt16(data, 0);
        }
        public int ReadInt32(Endianness Endian = Endianness.BigEndian)
        {
            byte[] data = new byte[sizeof(int)];
            stream.ReadExactly(data, 0, sizeof(int));
            if (DisallowAdvance) stream.Seek(-sizeof(int), SeekOrigin.Current);
            return Endian == Endianness.BigEndian ? EndianBitConverter.Big.ToInt32(data, 0) : EndianBitConverter.Little.ToInt32(data, 0);
        }
    }
}
