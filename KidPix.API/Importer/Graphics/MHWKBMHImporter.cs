using KidPix.API.Importer.Mohawk;
using KidPix.API.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace KidPix.API.Importer.Graphics
{
    /// <summary>
    /// A frame in a <see cref="BMHResource"/>
    /// </summary>
    /// <param name="Offset"></param>
    /// <param name="Length"></param>
    public record BMHFrameInfo(ushort Offset, int Length)
    {        
        public int Length { get; set; } = Length;
    }

    /// <summary>
    /// Every <see cref="BMHResource"/> starts with a table section that maps out the offsets of all the <see cref="BMPResource"/>s it contains
    /// </summary>
    public class BMHTable
    {
        /// <summary>
        /// This maps <c>[RESOURCE ID,[FRAME NUMBER, FILE OFFSET]]</c>
        /// </summary>
        public Dictionary<int, Dictionary<int, BMHFrameInfo>> Offsets { get; } = new();

        public BMHFrameInfo this[(int ResourceIndex, int FrameIndex) Identifier] => this[Identifier.ResourceIndex][Identifier.FrameIndex];
        public Dictionary<int, BMHFrameInfo> this[int ResourceIndex] => Offsets[ResourceIndex];

        public void AddScan(int Frame, BMHFrameInfo ScanLineInfo)
        {
            if (!Offsets.TryGetValue(Frame, out var map))
            {
                map = new Dictionary<int, BMHFrameInfo>();
                Offsets.Add(Frame, map);
            }
            map.Add(map.Count, ScanLineInfo);
        }        
    }

    /// <summary>
    /// A <see cref="KidPixResource"/> that contains a set of <see cref="BMPResource"/> packed together 
    /// intended to be used with animated objects
    /// </summary>
    public class BMHResource : KidPixResource
    {
        private Stream _fileStream;

        public BMHResource(Stream FileStream, BMHTable table, ResourceTableEntry ParentEntry) : base(ParentEntry)
        {
            Table = table;
            _fileStream = FileStream;
        }

        public BMHTable Table { get; }

        public byte[] ReadFrameData(int ResourceID, int FrameIndex)
        {
            var frame = Table[ResourceID][FrameIndex];
            return ReadFrameData(frame);
        }
        public byte[] ReadFrameData(BMHFrameInfo Info)
        {
            var frame = Info;
            _fileStream.Seek(frame.Offset, SeekOrigin.Begin);
            byte[] rawBytes = new byte[frame.Length];      
            _fileStream.ReadExactly(rawBytes,0,rawBytes.Length);
            return rawBytes;
        }

        /// <summary>
        /// Uses the <see cref="MHWKBitmapImporter"/> to convert the given <paramref name="FrameInfo"/> data to a <see cref="BMPResource"/>
        /// </summary>
        /// <param name="FrameInfo"></param>
        /// <returns><see cref="BMPResource"/></returns>
        public BMPResource? ImportFrame(BMHFrameInfo FrameInfo) => 
            MHWKResourceImporterBase.GetDefaultImporter(CHUNK_TYPE.tBMP).
            Import(new MemoryStream(ReadFrameData(FrameInfo)), ParentEntry) as BMPResource;

        public override void Dispose()
        {
            _fileStream.Dispose();
        }
    }

    /// <summary>
    /// Imports a <see cref="BMHResource"/> that contains <see cref="BMPResource"/> objects compressed together
    /// </summary>
    [MHWKImporter(CHUNK_TYPE.tBMH)]
    public class MHWKBMHImporter : MHWKResourceImporterBase
    {        
        public override KidPixResource? Import(Stream Stream, ResourceTableEntry ParentEntry)
        {
            EndianBinaryReader reader = new(Stream);

            ushort W = reader.ReadUInt16(); // Unsure Width?
            ushort UNKNOWN = reader.ReadUInt16(); // Unknown
            ushort H = reader.ReadUInt16(); // Unsure Height?
            ushort BITDEPTH = reader.ReadUInt16(); // appears to be bit depth

            //**read table
            BMHTable table = ReadBMHTable(reader);
            //**return bmh wrapper
            return new BMHResource(Stream, table, ParentEntry);
        }                

        private static BMHTable ReadBMHTable(EndianBinaryReader Reader)
        {
            BMHTable table = new();
            BMHFrameInfo lastScan = null;
            ushort Scan0Offset = 0;
            do
            {
                ushort frame_number = Reader.ReadUInt16();
                ushort offset = Reader.ReadUInt16();
                if (lastScan != null)
                    lastScan.Length = offset - lastScan.Offset;
                lastScan = new(offset, -1);
                table.AddScan(frame_number, lastScan);
                if (Scan0Offset == 0)
                    Scan0Offset = offset;
            } while (Reader.BaseStream.Position < Scan0Offset);
            return table;
        }
    }
}
