using KidPix.API.AppService.Sessions.Contexts;
using System.Drawing;

namespace KidPix.API.AppService.Render.CanvasBrushes
{
    /// <summary>
    /// A basic tool that draws a circular area of a given size and color.
    /// <para/>This tool uses no external resources and is self-contained
    /// </summary>
    public class KidPixPencilToolBrush : KidPixCanvasBrush
    {
        public static readonly Color Default_Color = KidPixGameplayContext.Default_Color;

        public KidPixPencilToolBrush(double radius) : this(Default_Color, radius) { }
        public KidPixPencilToolBrush(Color PrimaryColor, double radius) : base(PrimaryColor, radius) { }
        /// <summary>
        /// Creates a new <see cref="Brush"/> that is not reused. Please <see cref="Brush.Dispose()"/> when completed using
        /// </summary>
        /// <returns></returns>
        internal override Brush GetMyBrushInternal() => new SolidBrush(PrimaryColor);      
    }
}
