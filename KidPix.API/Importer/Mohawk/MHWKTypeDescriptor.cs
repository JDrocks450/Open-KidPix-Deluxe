using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidPix.API.Importer.Mohawk
{
    public static class MHWKTypeDescription
    {
        static Dictionary<CHUNK_TYPE, string> typeNames = new()
        {
            { CHUNK_TYPE.MHWK, "Mohawk" },
            { CHUNK_TYPE.SND, "Mohawk Sound Effect" },
            { CHUNK_TYPE.REGS, "Mohawk Registry" },
            { CHUNK_TYPE.tSCR, "Kid Pix Screen" },
            { CHUNK_TYPE.tBMH, "Animated Bitmaps?" },
            { CHUNK_TYPE.tBMP, "Mohawk Bitmap Graphics" },
            { CHUNK_TYPE.MBTN, "Kid Pix Button" },
            { CHUNK_TYPE.tMOV, "Quicktime Movie" },
            { CHUNK_TYPE.tWAV, "Mohawk Raw Wave File" },
            { CHUNK_TYPE.tPAL, "Bitmap Palette (for 8bpp Indexed)"}
        };
        /// <summary>
        /// Guarantees to return a <see cref="String"/> representing the given <paramref name="ChunkType"/> by first seeing if a 
        /// description entry has been entered for this type, falling back to converting the passed <paramref name="ChunkType"/> to 
        /// a <see cref="String"/> using <see cref="UTF8Encoding.GetString(byte[], int, int)"/>
        /// </summary>
        /// <param name="ChunkType"></param>
        /// <returns></returns>
        public static string GetTypeName(CHUNK_TYPE ChunkType)
        {
            if (typeNames.TryGetValue(ChunkType, out string Name))
                return Name;
            return GetChunkTypeStringFromBytes(ChunkType);
        }
        /// <summary>
        /// Returns the description entry for this type, if one has been entered for this type
        /// </summary>
        /// <param name="ChunkType"></param>
        /// <param name="Description"></param>
        /// <returns></returns>
        public static bool TryGetTypeName(CHUNK_TYPE ChunkType, out string Description) => typeNames.TryGetValue(ChunkType, out Description);
        /// <summary>
        /// Uses a <see cref="UTF8Encoding"/> to convert the given <paramref name="ChunkTypeBytes"/> to a string
        /// </summary>
        /// <param name="ChunkTypeBytes"></param>
        /// <returns></returns>
        public static string GetChunkTypeStringFromBytes(uint ChunkTypeBytes) => new string(Encoding.UTF8.GetString(BitConverter.GetBytes(ChunkTypeBytes)).Reverse().ToArray());
        /// <summary>
        /// Uses a <see cref="UTF8Encoding"/> to convert the given <paramref name="ChunkType"/> to a string
        /// </summary>
        /// <param name="ChunkTypeBytes"></param>
        /// <returns></returns>
        public static string GetChunkTypeStringFromBytes(CHUNK_TYPE ChunkType) => GetChunkTypeStringFromBytes((uint)ChunkType);
    }
}
