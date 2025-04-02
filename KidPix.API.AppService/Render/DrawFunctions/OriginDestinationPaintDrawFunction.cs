using KidPix.API.AppService.Render.CanvasBrushes;
using System.Drawing;

namespace KidPix.API.AppService.Render.DrawFunctions
{
    /// <summary>
    /// An abstract <see cref="KidPixCanvasBrushDrawingFunction"/> that will draw a shape on the screen from where the <see cref="Stroke"/> started to where it currently is
    /// </summary>
    public abstract class OriginDestinationPaintDrawFunction : KidPixCanvasBrushDrawingFunction
    {
        internal override Point TranslatePoint(Point Point, double Radius, PaintingCoordinateOrigin Origin) => Point;

        internal override void DoPaintingFunction(KidPixCanvasBrushDrawingFunctionContext context, params Point[] points)
        {
            var point1 = new Point(Math.Min(points[0].X, points[1].X), Math.Min(points[0].Y, points[1].Y));
            var point2 = new Point(Math.Max(points[0].X, points[1].X), Math.Max(points[0].Y, points[1].Y));
            var size = new Size(point2.X - point1.X, point2.Y - point1.Y);

            DoOriginDestinationPaintFunction(context, point1, point2, size);
        }

        protected abstract void DoOriginDestinationPaintFunction(KidPixCanvasBrushDrawingFunctionContext context, Point Origin, Point Destination, Size Size);

        internal override void DoDrawFunction(Graphics Graphics, KidPixCanvasBrush SelectedTool, Point PreviewPosition, Stroke CurrentStroke) => 
            SelectedTool.PaintInternal(Graphics, PreviewPosition, CurrentStroke);
    }
}
