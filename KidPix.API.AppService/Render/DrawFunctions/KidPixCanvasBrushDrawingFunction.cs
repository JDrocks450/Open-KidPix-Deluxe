using KidPix.API.AppService.Render.CanvasBrushes;
using System.Drawing;

namespace KidPix.API.AppService.Render.DrawFunctions
{
    /// <summary>
    /// Context used by <see cref="KidPixCanvasBrushDrawingFunction"/> objects
    /// </summary>
    public class KidPixCanvasBrushDrawingFunctionContext
    {
        public Graphics Graphics { get; set; }
        public Brush GDIBrush { get; set; }
        public double Radius { get; set; }
    }
    /// <summary>
    /// When overridden in a derived class, will determine how a <see cref="KidPixCanvasBrush"/> is drawn to the <see cref="KidPixArtCanvas"/>
    /// </summary>
    public abstract class KidPixCanvasBrushDrawingFunction
    {
        /// <summary>
        /// Performs the drawing function of this <see cref="KidPixCanvasBrushDrawingFunction"/>
        /// </summary>
        /// <param name="Graphics"></param>
        /// <param name="SelectedTool"></param>
        /// <param name="PreviewPosition"></param>
        /// <param name="CurrentStroke"></param>
        internal abstract void DoDrawFunction(Graphics Graphics, KidPixCanvasBrush SelectedTool, Point PreviewPosition, Stroke CurrentStroke);
        /// <summary>
        /// Calls the GDI call to put pixels on the <see cref="KidPixCanvasBrushDrawingFunctionContext.Graphics"/> instance
        /// </summary>
        /// <param name="context"></param>
        /// <param name="points"></param>
        internal abstract void DoPaintingFunction(KidPixCanvasBrushDrawingFunctionContext context, params Point[] points);
    }
}
