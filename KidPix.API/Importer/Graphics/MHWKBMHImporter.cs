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
            int frameIndex = 0;
            uint Scan0Offset = 0;
            IEnumerable<BMHFrameInfo> info = table.Resources.Values;
            do
            {
                ushort resource_number = Reader.ReadUInt16();
                uint offset = (uint)((Reader.ReadUInt16() + resource_number) + (ushort.MaxValue * resource_number));
                table.AddResource(new(frameIndex++, offset, 0));
                if (Scan0Offset == 0)
                    Scan0Offset = offset;
            } while (Reader.BaseStream.Position < Scan0Offset);
            for(int i = 0; i < info.Count() - 1; i++)            
                info.ElementAt(i).Length = info.ElementAt(i + 1).Offset - info.ElementAt(i).Offset;
            info.Last().Length = (uint)(Reader.BaseStream.Length - info.Last().Offset);
            return table;
        }

#if false
        private static BMHTable ReadBMHTableOld(EndianBinaryReader Reader)
        {
            BMHTable table = new();
            BMHFrameInfo lastScan = null;
            uint Scan0Offset = 0;
            int frameIndex = 0;
            uint offset_accumulator = 0;
            do
            {
                ushort resource_number = Reader.ReadUInt16();
                uint offset = (uint)((Reader.ReadUInt16() + resource_number) + (ushort.MaxValue * resource_number));
                if (lastScan != null && lastScan.ResourceID != resource_number) // does the last frame match this resource?                
                {
                    offset_accumulator = lastScan.Offset;
                    lastScan = null; // resource changed! set last scan to 0
                    frameIndex = 0;                    
                }
                if (lastScan != null)
                    lastScan.Length = (uint)(offset - lastScan.Offset);
                lastScan = new(resource_number, frameIndex++, offset, (uint)(Reader.BaseStream.Length - offset));
                table.AddScan(resource_number, lastScan);
                if (Scan0Offset == 0)
                    Scan0Offset = offset;
            } while (Reader.BaseStream.Position < Scan0Offset);
            return table;
        }
#endif
    }
}
