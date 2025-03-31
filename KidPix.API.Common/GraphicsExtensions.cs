using System.Drawing;
using System.Drawing.Imaging;

namespace KidPix.API.Common
{
    public static class GraphicsExtensions
    {
        /// <summary>
        /// Sets <see cref="Palette"/> to be a Greyscale palette (256 entries TransparentBlack -> White)
        /// </summary>
        public static ColorPalette MakeGreyscalePaletteSemiOpaque() => MakePaletteFromPrimaryColorSemiOpaque(Color.White);
        /// <summary>
        /// Sets <see cref="Palette"/> to be a Greyscale palette (256 entries Black -> White)
        /// </summary>
        public static ColorPalette MakeGreyscalePalette()
        {
            Color[] palette = new Color[256];
            for (int i = 0; i < 256; i++)
                palette[i] = Color.FromArgb(i, i, i);
            return new ColorPalette(palette);
        }
        /// <summary>
        /// Makes a <see cref="ColorPalette"/> from TransparentBlack to the <paramref name="PrimaryColor"/>
        /// </summary>
        /// <param name="PrimaryColor"></param>
        /// <returns></returns>
        public static ColorPalette MakePaletteFromPrimaryColorSemiOpaque(Color PrimaryColor)
        {
            Color fromColor = Color.FromArgb(0, PrimaryColor);
            Color[] arr = new Color[256];            
            for (int i = 1; i < 256; i++)
            {
                Color newColor = LerpColor(fromColor, PrimaryColor, i / 256.0f);
                newColor = Color.FromArgb(i, newColor.R, newColor.G, newColor.B);
                arr[i] = newColor;
            }
            return new(arr);
        }
        /// <summary>
        /// Makes a <see cref="ColorPalette"/> from Black to the <paramref name="PrimaryColor"/>
        /// </summary>
        /// <param name="PrimaryColor"></param>
        /// <returns></returns>
        public static ColorPalette MakePaletteFromPrimaryColor(Color PrimaryColor)
        {
            Color[] arr = new Color[256];
            for (int i = 1; i < 256; i++)
            {
                arr[i] = LerpColor(Color.Black, PrimaryColor, i / 256.0f);
            }
            arr[0] = Color.FromArgb(0, 0, 0, 0);
            return new(arr);
        }
        public enum Opaqueness
        {
            SemiOpaque,
            Opaque
        }
        public static ColorPalette MakePaletteFromPrimaryColor(Opaqueness Opacity, Color PrimaryColor) => 
            Opacity switch 
            { 
                Opaqueness.Opaque => MakePaletteFromPrimaryColor(PrimaryColor), 
                Opaqueness.SemiOpaque => MakePaletteFromPrimaryColorSemiOpaque(PrimaryColor) 
            };

        /// <summary>
        /// Lerps one <see cref="Color"/> to another <see cref="Color"/> by a percentage (0 -> 1)
        /// </summary>
        /// <param name="color1"></param>
        /// <param name="color2"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Color LerpColor(Color color1, Color color2, double t)
        {
            // Ensure t is clamped between 0 and 1
            t = Math.Clamp(t, 0f, 1f);

            double r = color1.R + (color2.R - color1.R) * t;
            double g = color1.G + (color2.G - color1.G) * t;
            double b = color1.B + (color2.B - color1.B) * t;

            return Color.FromArgb((int)r, (int)g, (int)b);
        }
    }
}
