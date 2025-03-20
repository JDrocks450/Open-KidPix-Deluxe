using KidPix.API.Importer.Mohawk;
using KidPix.API.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace KidPix.API.Importer.tWAV
{
    /// <summary>
    /// Used to import Wave file resources from a <see cref="CHUNK_TYPE.tWAV"/> chunk
    /// </summary>
    public static class WAVImporter
    {
        const uint DATA_TAG = 0x44617461; // "Data"

        /// <summary>
        /// Imports the tWAV resource from a stream (at the current position)
        /// </summary>
        /// <param name="Stream">The caller is responsible for Disposing this stream once it is no longer in use.</param>
        /// <returns></returns>
        public static WAVResource Import(Stream Stream, ResourceTableEntry ParentEntry)
        {
            EndianBinaryReader reader = new EndianBinaryReader(Stream);            

            WAVResource resource = new(ParentEntry);

            while (reader.BaseStream.Position < reader.BaseStream.Length-4)
            {
                uint callsign = reader.EnsureReadUInt32();
                if (callsign != (uint)CHUNK_TYPE.MHWK)
                    continue;
                reader.BaseStream.Seek(-4, SeekOrigin.Current);
                try
                {
                    TryImport(reader, resource);
                }
                catch { } // shit hack solution
                if (resource.WaveDataPresent) return resource;
                if (resource.IsComplete) break;
            }

            return resource;
        }

        private static void TryImport(EndianBinaryReader Reader, WAVResource Resource)
        {
            uint callsign = Reader.EnsureReadUInt32();
            if (callsign != (uint)CHUNK_TYPE.MHWK)
                throw new InvalidDataException("Read into a chunk expecting a MHWK chunk type which was not present.");
            uint chunkLength = Reader.EnsureReadUInt32();
            long position = Reader.BaseStream.Position;

            while (Reader.BaseStream.Position < position + chunkLength)
            {
                LONG_CHUNK_TYPE tagType = (LONG_CHUNK_TYPE)Reader.EnsureReadUInt64();
                if (tagType != LONG_CHUNK_TYPE.WAVEDATA && tagType != LONG_CHUNK_TYPE.WAVECUE)
                    throw new InvalidDataException("Expected WAVECue# or WAVEData chunk, which was not present. ");

                switch (tagType)
                {
                    case LONG_CHUNK_TYPE.WAVECUE:
                        ReadWaveCue(Reader, Resource, position, chunkLength);
                        break;
                    case LONG_CHUNK_TYPE.WAVEDATA:
                        ReadWaveData(Reader, Resource, position, chunkLength);
                        break;

                }
                break;
            }
            Reader.BaseStream.Seek(position + chunkLength, SeekOrigin.Begin);
        }

        private static void ReadWaveData(EndianBinaryReader Reader, WAVResource Resource, long ChunkStart, uint ChunkLength)
        {
            long boundary = Math.Min(Reader.BaseStream.Length - Reader.BaseStream.Position, ChunkLength - ChunkStart);
            long DataSize = Math.Min(Reader.EnsureReadUInt32() - 28, boundary);
            Resource.WaveData = new WAVData(
                SampleRate: Reader.ReadUInt16(),                // sample rate   | ushort | 2
                SampleCount: Reader.ReadUInt32(),               // sample count  | uint   | 4         
                BitsPerSample: Reader.ReadByte(),               // BitsPerSample | byte   | 1
                Channels: Reader.ReadByte(),                    // Channels      | byte   | 1
                Encoding: (WAVEncodings)Reader.ReadUInt16(),    // Encoding      | ushort | 4
                LoopCount: Reader.ReadUInt16(),                 // LoopCount     | ushort | 4
                LoopStart: Reader.EnsureReadUInt32(),           // LoopStart     | uint   | 8
                LoopEnd: Reader.EnsureReadUInt32()              // LoopEnd       | uint   | 8
            );
            Reader.ReadByte(); // offset by 1 byte
            Resource.WaveData.AudioDataStream = new MemoryStream();
            Reader.BaseStream.CopyToExact(Resource.WaveData.AudioDataStream, (int)DataSize);
        }

        private static void ReadWaveCue(EndianBinaryReader Reader, WAVResource Resource, long ChunkStart, uint ChunkLength)
        {
            if (Resource.WaveCuePresent) return;
            WAVHeader header = Resource.Header = new()
            {
                Unknown1 = Reader.EnsureReadUInt32(),
                Unknown2 = Reader.ReadUInt16(),
                Unknown3 = Reader.EnsureReadUInt32(),
                Unknown4 = Reader.ReadUInt16(),

                FileName = Reader.ReadCStr()
            };

            if (Reader.EnsureReadUInt32() != DATA_TAG)
                throw new InvalidDataException("Expected a 'Data' tag but it was not present. ");

            long boundary = Math.Min(Reader.BaseStream.Length - Reader.BaseStream.Position, ChunkLength - ChunkStart);
            long DataSize = Math.Min(Reader.EnsureReadUInt32(), boundary);
            Resource.WaveCueStream = new MemoryStream((int)DataSize);
            Reader.BaseStream.CopyToExact(Resource.WaveCueStream, (int)DataSize);
        }
    }
}
