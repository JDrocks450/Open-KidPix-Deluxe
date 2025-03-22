using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidPix.API.Importer.tBMP
{
    public enum BitmapDrawCompression : ushort
    {
        kDrawMASK = 0x00f0,
        kDrawRaw = 0x0000,
        kDrawRLE8 = 0x0010,
        kDrawMSRLE8 = 0x0020,
        kDrawRLE = 0x0030,
    }

    public enum BitmapPackCompression : ushort
    {
        kPackMASK = 0x0f00,
        kPackNone = 0x0000,
        kPackLZ = 0x0100,
        kPackLZ1 = 0x0200,
        kPackRiven = 0x0400,
        kPackXDec = 0x0f00,
    }

    public enum BitmapFormat : ushort
    {
        kBitsPerPixel1 = 0x0000,
        kBitsPerPixel4 = 0x0001,
        kBitsPerPixel8 = 0x0002,
        kBitsPerPixel16 = 0x0003,
        kBitsPerPixel24 = 0x0004,
        kBitsPerPixelMask = 0x0007,
        kBitmapHasCLUT = 0x0008,
        kDrawMASK = 0x00f0,
        kDrawRaw = 0x0000,
        kDrawRLE8 = 0x0010,
        kDrawMSRLE8 = 0x0020,
        kDrawRLE = 0x0030,
        kPackMASK = 0x0f00,
        kPackNone = 0x0000,
        kPackLZ = 0x0100,
        kPackLZ1 = 0x0200,
        kPackRiven = 0x0400,
        kPackXDec = 0x0f00,
        kFlagMASK = 0xf000,
        kFlag16_80X86 = 0x1000, // 16 bit pixel data has been converted to 80X86 format
        kFlag24_MAC = 0x1000 // 24 bit pixel data has been converted to MAC 32 bit format
    };
}
