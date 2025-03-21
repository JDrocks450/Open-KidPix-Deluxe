﻿using KidPix.API.Importer.Mohawk;
using System.Drawing;

namespace KidPix.API.Importer.tBMP
{
    /// <summary>
    /// A <see cref="KidPixResource"/> that contains Raster Image data
    /// </summary>
    [KidPixResourceAttribute(CHUNK_TYPE.tBMP)] 
    public class BMPResource : KidPixResource, IDisposable
    {
        public BMPResource(ResourceTableEntry ParentEntry, BMPHeader Header) : base(ParentEntry) => this.Header = Header;
        public BMPResource(ResourceTableEntry ParentEntry, BMPHeader Header, Bitmap Image) : this(ParentEntry, Header) => BitmapImage = Image;

        public BMPHeader Header { get; }
        public Bitmap? BitmapImage { get; internal set; } = null;
        public Stream ImageStream { get; internal set; }

        public override void Dispose()
        {
            BitmapImage?.Dispose();
            BitmapImage = null;
        }
    }
}    

