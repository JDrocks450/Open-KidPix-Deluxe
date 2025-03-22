using KidPix.API.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KidPix.API.Importer.Mohawk.MHWKFileTable;
using static KidPix.API.Importer.Mohawk.MHWKNameTable;

/*
 * THANK YOU to SCUMMVM for cracking the Mohawk Data File Format!
 * This is a C# implementation of resource.cpp found at:
 * https://github.com/scummvm/scummvm/blob/master/engines/mohawk/resource.cpp
 */

namespace KidPix.API.Importer.Mohawk
{

    /// <summary>
    /// Imports a MHWK file, common file types for this *.KLB and *.MHK files
    /// </summary>
    public static class MHWKImporter
    {
        public static MHWKFile Import(string FileName)
        {
            using FileStream fileStream = File.OpenRead(FileName);
            using EndianBinaryReader binaryReader = new EndianBinaryReader(fileStream);

            //**read file header**            
            ReadMohawkHeader(binaryReader, out uint cSize);
            //read file table
            MHWKFileTable fileTable = ReadFileTable(binaryReader);
            //read type entries
            ReadResourceTable(binaryReader, fileTable, out var resTable);

            return new MHWKFile(FileName, fileStream.Length)
            {
                Files = fileTable,
                MHWKChunkPayloadSize = cSize,
                Resources = resTable
            };
        }

        static void ReadMohawkHeader(in EndianBinaryReader binaryReader, out uint ChunkSize)
        {
            uint callsign = binaryReader.ReadUInt32(); // must be Mohawk Archive
            if (callsign != (uint)CHUNK_TYPE.MHWK)
                throw new InvalidDataException($"This file cannot be read by this application. (Chunk type is {callsign})");
            ChunkSize = binaryReader.ReadUInt32();
            if (ChunkSize == 0)
                throw new InvalidDataException("This file cannot be read by this application. (Size is 0)");
        }

        static MHWKFileTable ReadFileTable(in EndianBinaryReader binaryReader)
        {
            if (binaryReader.ReadUInt32() != (uint)CHUNK_TYPE.RSRC)
                throw new InvalidDataException("RSRC Chunk is not present.");

            ushort version = binaryReader.ReadUInt16(Endianness.LittleEndian);

            ushort compaction = binaryReader.ReadUInt16();
            uint rsrcSize = binaryReader.ReadUInt32();
            uint absOffset = binaryReader.ReadUInt32();
            ushort fileTableOffset = binaryReader.ReadUInt16();
            ushort fileTableSize = binaryReader.ReadUInt16();

            // First, read in the file table
            binaryReader.BaseStream.Seek(absOffset + fileTableOffset, SeekOrigin.Begin);
            uint entries = binaryReader.ReadUInt32();
            MHWKFileTable fileTable = new(entries, absOffset)
            {
                FileTableOffset = fileTableOffset
            };

            Debug.WriteLine($"Reading file table with {entries} entries");

            for (uint i = 0; i < entries; i++)
            {
                FileTableEntry entry = new();

                entry.Offset = binaryReader.ReadUInt32();
                entry.Size = binaryReader.ReadUInt16();
                entry.Size += binaryReader.ReadByte() << 16; // Get bits 15-24 of size too
                entry.Flags = binaryReader.ReadByte();
                entry.Unknown = binaryReader.ReadUInt16();

                // Add in another 3 bits for file size from the flags.
                // The flags are useless to us except for doing this ;)
                entry.Size += (entry.Flags & 7) << 24;

                Debug.WriteLine($"File[{i}]: {entry}");

                fileTable.Entries.Add(entry);
            }

            return fileTable;
        }

        static void ReadResourceTable(in EndianBinaryReader binaryReader, MHWKFileTable FileTable, out MHWKResourceTable? ResTable)
        {            
            MHWKResourceTable resMap = ResTable = new();

            // Now go in and read in each of the types
            binaryReader.BaseStream.Seek(FileTable.AbsoluteOffset, SeekOrigin.Begin);
            ushort typeTableOffset = binaryReader.ReadUInt16();
            ushort typeCount = binaryReader.ReadUInt16();

            Debug.WriteLine($"Reading Type List ({typeTableOffset:X8}), {typeCount} entries...");

            //map types to names and resources
            for (ushort i = 0; i < typeCount; i++)
            { // foreach type in Types
                CHUNK_TYPE tag = (CHUNK_TYPE)binaryReader.ReadUInt32();

                //**read as str
                binaryReader.BaseStream.Seek(-sizeof(uint), SeekOrigin.Current);
                byte[] strBytes = new byte[sizeof(uint)];
                binaryReader.BaseStream.ReadExactly(strBytes);
                string typeNameStr = Encoding.UTF8.GetString(strBytes);
                //**

                ushort resourceTableOffset = binaryReader.ReadUInt16();
                ushort nameTableOffset = binaryReader.ReadUInt16();
                    
                Debug.WriteLine($"Type[{i}]: Tag = {tag} ResTable Offset = {resourceTableOffset:X4}  NameTable Offset = {nameTableOffset}");

                // Name Table
                binaryReader.BaseStream.Seek(FileTable.AbsoluteOffset + nameTableOffset, SeekOrigin.Begin);
                ushort nameTableEntries = binaryReader.ReadUInt16();
                MHWKNameTable nameTable = new();

                Debug.WriteLine($"Names = {nameTableEntries} entries");

                for (ushort j = 0; j < nameTableEntries; j++)
                {
                    NameTableEntry entry = new()
                    {
                        Offset = binaryReader.ReadUInt16(),
                        Index = binaryReader.ReadUInt16()
                    };

                    nameTable.Entries.Add(entry);

                    Debug.WriteLine($"Entry[{j:X2}]: Name List Offset = {entry.Offset:X4}  Index = {entry.Index}");
                }

                for (ushort j = 0; j < nameTableEntries; j++)
                {
                    NameTableEntry currentEntry = nameTable.Entries.ElementAt(j);

                    // Name List
                    binaryReader.BaseStream.Seek(FileTable.AbsoluteOffset + typeTableOffset + currentEntry.Offset, SeekOrigin.Begin);
                    currentEntry.Name = binaryReader.ReadCStr();

                    Debug.WriteLine($"Name = \'{currentEntry.Name}\'");
                }

                // Resource Table
                binaryReader.BaseStream.Seek(FileTable.AbsoluteOffset + resourceTableOffset, SeekOrigin.Begin);
                ushort resourceCount = binaryReader.ReadUInt16();

                Debug.WriteLine($"Resource count = {resourceCount}");                

                for (ushort j = 0; j < resourceCount; j++)
                {
                    ushort id = binaryReader.ReadUInt16();
                    ushort index = binaryReader.ReadUInt16();

                    ResourceTableEntry resEntry = new ResourceTableEntry(tag, id, index);

                    resMap.Add(tag, resEntry);

                    // Pull out the name from the name table
                    for (int k = 0; k < nameTable.Entries.Count; k++)
                    {
                        if (nameTable.Entries.ElementAt(k).Index == index)
                        {
                            resEntry.Name = nameTable.Entries.ElementAt(k).Name;
                            break;
                        }
                    }

                    // Pull out our offset/size too
                    resEntry.Offset = FileTable.Entries.ElementAt(index - 1).Offset;

                    // WORKAROUND: tMOV resources pretty much ignore the size part of the file table,
                    // as the original just passed the full Mohawk file to QuickTime and the offset.
                    // We set the resource size to the number of bytes till the beginning of the next
                    // resource in the archive.
                    // We need to do this because of the way Mohawk is set up (this is much more "proper"
                    // than passing _stream at the right offset). We may want to do that in the future, though.
                    if (tag == CHUNK_TYPE.tMOV)
                    {
                        ushort nextFileIndex = index;
                        resEntry.Size = 0;
                        while (resEntry.Size == 0)
                        {
                            if (nextFileIndex == FileTable.Entries.Count)
                                resEntry.Size = binaryReader.BaseStream.Length - FileTable[index - 1].Offset;
                            else
                                resEntry.Size = FileTable[nextFileIndex].Offset - FileTable[index - 1].Offset;

                            // Loop because two entries in the file table may point to the same data
                            // in the archive.
                            nextFileIndex++;
                        }
                    }
                    else
                        resEntry.Size = FileTable[index - 1].Size;

                    Debug.WriteLine($"Entry[{j}]: ID = {id} Index = {index}");
                }

                // Return to next TypeTable entry
                binaryReader.BaseStream.Seek(4 + FileTable.AbsoluteOffset + ((i + 1) * 8), SeekOrigin.Begin);
            }
        }
    }
}
