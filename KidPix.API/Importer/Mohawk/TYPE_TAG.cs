namespace KidPix.API.Importer.Mohawk
{
#if false
    public enum TYPE_TAG
    {
        // Main FourCCs
 ID_MHWK = "MHWK", // Main FourCC
 ID_RSRC = "RSRC", // Resource Directory Tag

        // Myst Resource FourCCs
 ID_CLRC = "CLRC" // Cursor Hotspots
 ID_EXIT ="EXIT" // Card Exit Scripts
 ID_HINT ="HINT" // Cursor Hints
 ID_INIT ="INIT" // Card Entrance Scripts
 ID_MSND ="MSND" // Standard Mohawk Sound
 ID_RLST =RLST // Resource List Specifies HotSpots
 ID_RSFL =RSFL // ??? (system.dat only
 ID_VIEW =VIEW // Card Details
 ID_WDIB =WDIB // LZ-Compressed Windows Bitmap

        // Myst Masterpiece Edition Resource FourCCs (In addition to Myst FourCCs
 ID_HELP =HELP // Help Chunk
 ID_MJMP =MJMP // MSND Jumps (To reduce MSND duplication
 ID_PICT =PICT // JPEG/PICT/WDIB Image

        // Riven Resource FourCCs
 ID_BLST =BLST // Card Hotspot Enabling Lists
 ID_CARD =CARD // Card Scripts
 ID_FLST =FLST // Card SFXE Lists
 ID_HSPT =HSPT // Card Hotspots
 ID_MLST =MLST // Card Movie Lists
 ID_NAME =NAME // Object Names
 ID_PLST =PLST // Card Picture Lists
 ID_RMAP =RMAP // Card Codes
 ID_SFXE =SFXE // Water Effect Animations
 ID_SLST =SLST // Card Ambient Sound Lists
 ID_TMOV =tMOV // QuickTime Movie

        // Riven Saved Game FourCCs
 ID_VARS =VARS // Variable Values
 ID_VERS =VERS // Version Info
 ID_ZIPS =ZIPS // Zip Mode Status
 ID_META =META // ScummVM save metadata
 ID_THMB =THMB // ScummVM save thumbnail

        // Zoombini Resource FourCCs
 ID_SND   =0SND // Standard Mohawk Sound
 ID_CURS =CURS // Cursor
 ID_SCRB =SCRB // Feature Script
 ID_SCRS =SCRS // Snoid Script
 ID_NODE =NODE // Walk Node
 ID_PATH =PATH // Walk Path
 ID_SHPL =SHPL // Shape List

        // Living Books Resource FourCCs
 ID_TCUR =tCUR // Cursor
 ID_BITL =BITL // Book Item List
 ID_CTBL =CTBL // Color Table
 ID_SCRP =SCRP // Script
 ID_SPR  =SPR# // Sprite?
 ID_VRSN =VRSN // Version
 ID_ANI  =ANI  // Animation
 ID_SHP  =SHP# // Shape
 ID_WAV  =WAV  // Old Sound Resource
 ID_BMAP =BMAP // Old Mohawk Bitmap
 ID_BCOD =BCOD // Book Code

        // JamesMath Resource FourCCs
 ID_TANM =tANM // Animation?
 ID_TMFO =tMFO // ???

        // CSTime Resource FourCCs
 ID_CINF =CINF // Case Info
 ID_CONV =CONV // Conversation
 ID_HOTS =HOTS // Hotspot
 ID_INVO =INVO // Inventory Object
 ID_QARS =QARS // Question and Responses
 ID_SCEN =SCEN // Scene
 ID_STRI =STRI // String Entry?

        // Mohawk Wave Tags
 ID_WAVE =WAVE // Game Sound (Third Tag
 ID_ADPC =ADPC // Game Sound Chunk
 ID_DATA =Data // Game Sound Chunk
 ID_CUE  =Cue# // Game Sound Chunk

        // Mohawk MIDI Tags
 ID_MIDI =MIDI // Game Sound (Third Tag instead of WAVE
 ID_PRG  =Prg# // MIDI Patch

        // Common Resource FourCCs
 ID_TBMP =tBMP // Standard Mohawk Bitmap
 ID_TWAV =tWAV // Standard Mohawk Sound
 ID_TPAL =tPAL // Standard Mohawk Palette
 ID_TCNT =tCNT // Shape Count (CSWorld CSAmtrak JamesMath
 ID_TSCR =tSCR // Script (CSWorld CSAmtrak Treehouse
 ID_STRL =STRL // String List (Zoombini CSWorld CSAmtrak
 ID_TBMH =tBMH // Standard Mohawk Bitmap
 ID_TMID =tMID // Standard Mohawk MIDI
 ID_REGS =REGS // Registration Data - Shape Offsets (Zoombini Treehouse
 ID_BYTS =BYTS // Byte Array? (Used as Database Entry in CSWorld CSAmtrak
 ID_INTS =INTS // uint16 Array? (CSWorld CSAmtrak
 ID_BBOX =BBOX // Boxes? (CSWorld CSAmtrak
 ID_SYSX =SYSX // MIDI Sysex
    }
    #endif
}

