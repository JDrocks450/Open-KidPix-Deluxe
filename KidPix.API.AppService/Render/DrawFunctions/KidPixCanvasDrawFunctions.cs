namespace KidPix.API.AppService.Render.DrawFunctions
{
    /// <summary>
    /// Implements a set of predefined <see cref="KidPixCanvasBrushDrawingFunction"/> objects
    /// </summary>
    public static class KidPixCanvasDrawFunctions
    {        
        public static FreePaintDrawFunction FreePaintFunction { get; } = new FreePaintDrawFunction();
        public static RectanglePaintDrawFunction RectangleFillFunction { get; } = new RectanglePaintDrawFunction();
        public static EllipsePaintDrawFunction CircularFillFunction { get; } = new EllipsePaintDrawFunction();
    }
}
