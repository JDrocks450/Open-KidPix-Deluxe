﻿namespace KidPix.API.Importer.tBMP.Decompressor
{

    public partial class BMPRLE16Brush
    {
        public class RLEDrawCall
        {
            public long OFFSET { get; set; }
            public long LENGTH { get; set; }

            public RLEDrawCall(long OFFSET)
            {
                this.OFFSET = OFFSET;
            }

            public byte OPCODE1 { get; set; }
            public byte OPCODE2 { get; set; }
            public ushort OPCODEPARAM1 { get; set; }
            public byte PIXELVAL1 { get; set; }
            public byte PIXELVAL2 { get; set; }
            public byte PIXELVAL3 { get; set; }
            public byte PIXELVAL4 { get; set; }

            public int COPIED_BYTES { get; set; }

            public int CREATED_PIXELS { get; set; }

            public override string ToString()
            {
                return $"[{OFFSET:X8}] OPCODES: 0x{OPCODE1:X2},0x{OPCODE2:X2} (PARAM0:{OPCODEPARAM1:X4}, " +
                    $"VALS: {PIXELVAL1:X4} {PIXELVAL2:X4} {PIXELVAL3:X4} {PIXELVAL4:X4}) {{{ COPIED_BYTES} }}";
            }
        }
    }
}
