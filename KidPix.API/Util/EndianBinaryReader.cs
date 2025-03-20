using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidPix.API.Util
{
    public class EndianBinaryReader : IDisposable
    {
        private readonly Stream stream;
        public Stream BaseStream => stream;

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
        public uint EnsureReadUInt32(Endianness Endian = Endianness.BigEndian)
        {
            byte[] data = new byte[sizeof(uint)];
            stream.ReadExactly(data, 0, sizeof(uint));
            return Endian == Endianness.BigEndian ? EndianBitConverter.Big.ToUInt32(data, 0) : EndianBitConverter.Little.ToUInt32(data, 0);
        }
        /// <summary>
        /// Reads at most 4 bytes from the <see cref="Stream"/> and converts it to a <see cref="UInt32"/> in the specified <paramref name="Endian"/>
        /// </summary>
        /// <param name="Endian"></param>
        /// <returns></returns>
        public uint ReadUInt32(Endianness Endian = Endianness.BigEndian)
        {
            byte[] data = new byte[sizeof(uint)];
            stream.Read(data, 0, sizeof(uint));
            return Endian == Endianness.BigEndian ? EndianBitConverter.Big.ToUInt32(data, 0) : EndianBitConverter.Little.ToUInt32(data, 0);
        }
        public ushort ReadUInt16(Endianness Endian = Endianness.BigEndian)
        {
            byte[] data = new byte[sizeof(ushort)];
            stream.Read(data, 0, sizeof(ushort));
            return Endian == Endianness.BigEndian ? EndianBitConverter.Big.ToUInt16(data, 0) : EndianBitConverter.Little.ToUInt16(data, 0);
        }
        public byte ReadByte() => (byte)stream.ReadByte();

        public ulong EnsureReadUInt64(Endianness Endian = Endianness.BigEndian)
        {
            byte[] data = new byte[sizeof(ulong)];
            stream.ReadExactly(data, 0, sizeof(ulong));
            return Endian == Endianness.BigEndian ? EndianBitConverter.Big.ToUInt64(data, 0) : EndianBitConverter.Little.ToUInt64(data, 0);
        }

        public string ReadCStr()
        {
            string currentEntry = "";
            byte c = ReadByte();
            while (c != 0)
            {
                currentEntry += (char)c;
                c = ReadByte();
            }
            return currentEntry;
        }
    }
}
