namespace KidPix.API.AppService.Render
{
    /// <summary>
    /// Determines settings used to create a new <see cref="KidPixArtCanvas"/>
    /// </summary>
    public class KidPixArtCanvasDefinition
    {
        public const int KidPix_DefaultCanvasSizeX = 670;
        public const int KidPix_DefaultCanvasSizeY = 478;

        /// <summary>
        /// The width of the canvas, in pixels
        /// </summary>
        public int Width { get; } = KidPix_DefaultCanvasSizeX;
        /// <summary>
        /// The height of the canvas, in pixels
        /// </summary>
        public int Height { get; } = KidPix_DefaultCanvasSizeY;

        public KidPixArtCanvasDefinition()
        {

        }

        public KidPixArtCanvasDefinition(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public static KidPixArtCanvasDefinition Default => new();
    }
}
