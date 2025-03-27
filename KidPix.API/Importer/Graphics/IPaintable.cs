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
        /// Sets <see cref="Palette"/> to be a Greyscale palette (256 entries Black -> White)
        /// <para/><paramref name="Image"/> is optional if you already have a Bitmap you'd like to set the palette on
        /// </summary>
        public void SetGreyscalePalette(Bitmap? Image)
        {
            Color[] palette = new Color[256];
            for (int i = 0; i < 256; i++)
                palette[i] = Color.FromArgb(i, i, i);
            Palette = new ColorPalette(palette);
            if (Image != null) Image.Palette = Palette;
        }

        /// <summary>
        /// Gets a <see cref="Bitmap"/> representation of this object
        /// </summary>
        /// <returns></returns>
        public Bitmap Paint();
    }
}

