using KidPix.API.AppService.Render.CanvasBrushes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidPix.API.AppService.Render
{
    internal class Stroke
    {
        public Stroke(Point mousePosition)
        {
            StartPosition = CurrentPosition = mousePosition;
        }

        /// <summary>
        /// The current position of the brush over the canvas
        /// </summary>
        public Point CurrentPosition { get; set; }
        /// <summary>
        /// The position on the canvas where this stroke began
        /// </summary>
        public Point StartPosition { get; set; }
        /// <summary>
        /// Brush strokes will intentionally not overdraw if the user is holding the brush down yet hasn't actually moved it
        /// <para/>This mode will continue to paint in the same position as long as the brush is down, thus overdrawing to the canvas
        /// <para/>Default is false
        /// </summary>
        public bool ContinuousSprayMode { get; set; } = false;
        public KidPixCanvasBrush.PaintingCoordinateOrigin CoordinateOrigin { get; internal set; }
    }
}
