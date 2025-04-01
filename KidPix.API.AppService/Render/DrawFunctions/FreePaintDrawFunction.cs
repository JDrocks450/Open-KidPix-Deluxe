using KidPix.API.AppService.Render.CanvasBrushes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace KidPix.API.AppService.Render.DrawFunctions
{
    /// <summary>
    /// A <see cref="KidPixCanvasBrushDrawingFunction"/> that will paint instantaneously at the point on the canvas specified and connect the stroke points for fast strokes
    /// </summary>
    public class FreePaintDrawFunction : KidPixCanvasBrushDrawingFunction
    {
        internal override void DoPaintingFunction(KidPixCanvasBrushDrawingFunctionContext context, params Point[] points) => 
            context.Graphics.FillEllipse(context.GDIBrush, new Rectangle(points[0], new Size((int)(context.Radius * 2), (int)(context.Radius * 2))));

        internal override void DoDrawFunction(Graphics Graphics, KidPixCanvasBrush SelectedTool, Point PreviewPosition, Stroke CurrentStroke)
        {
            var _currentStroke = CurrentStroke;
            var _graphics = Graphics;

            Point drawPoint = PreviewPosition;
            double strokeLen = Vector2.Distance(new(PreviewPosition.X, PreviewPosition.Y), new(_currentStroke.CurrentPosition.X, _currentStroke.CurrentPosition.Y));
            int totalStrokeLen = (int)Math.Round(strokeLen);

            if (strokeLen > 0)
            {
                for (int i = 0; i < totalStrokeLen; i++)
                {
                    float percentage = i / (float)totalStrokeLen;
                    var lerpedPoint = Vector2.Lerp(new(PreviewPosition.X, PreviewPosition.Y), new(_currentStroke.CurrentPosition.X, _currentStroke.CurrentPosition.Y), percentage);
                    drawPoint = new((int)Math.Round(lerpedPoint.X), (int)Math.Round(lerpedPoint.Y));
                    SelectedTool.PaintInternal(_graphics, drawPoint, CurrentStroke);
                }
            }
            else SelectedTool.PaintInternal(_graphics, drawPoint, CurrentStroke);
        }
    }
}
