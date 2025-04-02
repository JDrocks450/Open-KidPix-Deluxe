using System.Drawing;

namespace KidPix.API.AppService.Render.DrawFunctions
{
    /// <summary>
    /// A <see cref="KidPixCanvasBrushDrawingFunction"/> that will draw a rectangle on the screen from where the <see cref="Stroke"/> started to where it currently is
    /// </summary>
    public sealed class RectanglePaintDrawFunction : OriginDestinationPaintDrawFunction
    {
        internal override bool IsPreviewable => true;

        protected override void DoOriginDestinationPaintFunction(KidPixCanvasBrushDrawingFunctionContext context, Point Origin, Point Destination, Size Size) =>
            context.Graphics.FillRectangle(context.GDIBrush, new Rectangle(Origin, Size));
    }
}
