using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidPix.API.AppService.Render
{
    /// <summary>
    /// Determines how to create a new <see cref="KidPixArtCanvas"/>
    /// </summary>
    public class KidPixArtCanvasDefinition
    {
        public const int KidPix_DefaultCanvasSizeX = 670;
        public const int KidPix_DefaultCanvasSizeY = 478;

        public int Width { get; } = KidPix_DefaultCanvasSizeX;
        public int Height { get; } = KidPix_DefaultCanvasSizeY;

        public KidPixArtCanvasDefinition()
        {

        }

        public static KidPixArtCanvasDefinition Default => new();
    }

    /// <summary>
    /// Exposes functionality to create and manipulate a Bitmap with special functionality for User Control
    /// <para/>You should use this to help you display a new canvas to draw stuff on
    /// </summary>
    public class KidPixArtCanvas : IDisposable
    {
        private Graphics _graphics;
        private Point MousePosition = new(0,0);

        public KidPixArtCanvasDefinition CanvasDefinition { get; private set; }
        /// <summary>
        /// A reference to the <see cref="Bitmap"/> this <see cref="KidPixArtCanvas"/> is altering
        /// </summary>
        public Bitmap CanvasImage { get; private set; }

        /// <summary>
        /// Creates a new Blank canvas with the default <see cref="KidPixArtCanvasDefinition"/>
        /// </summary>
        public KidPixArtCanvas()
        {
            CanvasDefinition = KidPixArtCanvasDefinition.Default;
            Clear();
        }

        public KidPixArtCanvas(KidPixArtCanvasDefinition Definition)
        {
            CanvasDefinition = Definition;
            Clear();
        }

        /// <summary>
        /// Sets the cursor for the current 
        /// </summary>
        public void SetBrushCursorPosition(int X, int Y) => MousePosition = new(X, Y);

        /// <summary>
        /// Clears the current <see cref="KidPixArtCanvas"/> and allows you to reuse this instance with a new <see cref="KidPixArtCanvasDefinition"/>
        /// <para/>This will clear all art on the canvas
        /// </summary>
        /// <param name="NewSettings"></param>
        public void Clear(KidPixArtCanvasDefinition? NewSettings = default)
        {
            if (NewSettings != null)
                CanvasDefinition = NewSettings;
            Dispose(); // dispose of old bitmap image

            CanvasImage = new Bitmap(CanvasDefinition.Width, CanvasDefinition.Height);
            _graphics = Graphics.FromImage(CanvasImage);

            using Brush whiteBrush = new SolidBrush(Color.White);
            _graphics.FillRectangle(whiteBrush, new Rectangle(0,0,CanvasDefinition.Width,CanvasDefinition.Height));
        }

        public void DrawEllipse(Color Color, double EllipseRadius)
        {
            using Brush p = new SolidBrush(Color);
            _graphics.FillEllipse(p, new Rectangle(MousePosition, new Size((int)(EllipseRadius * 2), (int)(EllipseRadius * 2))));            
        }

        public void Dispose()
        {
            _graphics?.Dispose();
            _graphics = null;
            CanvasImage?.Dispose();
            CanvasImage = null;
        }
    }
}
