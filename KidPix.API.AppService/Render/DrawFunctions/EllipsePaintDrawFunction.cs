using System.Drawing;

namespace KidPix.API.AppService.Render.DrawFunctions
{
    /// <summary>
    /// A <see cref="KidPixCanvasBrushDrawingFunction"/> that will draw a circle on the streen from where the <see cref="Stroke"/> started to where it currently is
    /// </summary>
    public class EllipsePaintDrawFunction : OriginDestinationPaintDrawFunction
    {
        protected override void DoOriginDestinationPaintFunction(KidPixCanvasBrushDrawingFunctionContext context, Point Origin, Point Destination, Size Size) =>
            context.Graphics.FillEllipse(context.GDIBrush, new Rectangle(Origin, Size));
    }
}
