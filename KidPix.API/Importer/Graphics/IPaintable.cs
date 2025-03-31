using KidPix.API.Importer.Graphics.Brushes;
using KidPix.API.Util;
using System.Drawing;
using System.Drawing.Imaging;

namespace KidPix.API.Importer.Graphics
{
    /// <summary>
    /// An object that has exposed functions for rendering an image to the screen
    /// </summary>
    public interface IPaintable
    {
        /// <summary>
        /// The header for the currently painting image
        /// </summary>
        public BMPHeader Header { get; }

        /// <summary>
        /// The palette to use to display this <see cref="IPaintable"/>
        /// <para/>Only supported for <see cref="BMPHeader.BitDepthDescription"/> <see cref="BitmapFormat.kBitsPerPixel8"/> (8bpp Indexed) imagery
        /// <para/>Using this on a resource that isn't the above will not have any effect
        /// </summary>
        public ColorPalette? Palette { get; set; }

        /// <summary>
        /// Macro function for calling <see cref="BMPBrush.MakePaletteFromPrimaryColor(Color)"/>
        /// </summary>
        public void SetPaletteToPrimaryColorPalette(Color PrimaryColor, Common.GraphicsExtensions.Opaqueness Opacity) => 
            Palette = Common.GraphicsExtensions.MakePaletteFromPrimaryColor(Opacity, PrimaryColor);

        /// <summary>
        /// Gets a <see cref="Bitmap"/> representation of this object
        /// </summary>
        /// <returns></returns>
        public Bitmap Paint();
    }
}

