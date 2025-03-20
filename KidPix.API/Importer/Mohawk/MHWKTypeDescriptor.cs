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
            { CHUNK_TYPE.tBMH, "General Mohawk Bitmap" },
            { CHUNK_TYPE.tBMP, "General Mohawk Bitmap" },
            { CHUNK_TYPE.MBTN, "Kid Pix Button" },
            { CHUNK_TYPE.tMOV, "Quicktime Movie" },
            { CHUNK_TYPE.tWAV, "Mohawk Raw Wave File" },
            { CHUNK_TYPE.tPAL, "Bitmap Palette (for 8bpp Indexed)"}
        };
        public static string GetTypeName(CHUNK_TYPE ChunkType) => typeNames[ChunkType];
        public static bool TryGetTypeName(CHUNK_TYPE ChunkType, out string Description) => typeNames.TryGetValue(ChunkType, out Description);
    }
}
