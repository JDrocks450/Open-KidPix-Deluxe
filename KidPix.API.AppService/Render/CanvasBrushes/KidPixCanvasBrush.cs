using KidPix.API.AppService.Render.DrawFunctions;
using System.Drawing;

namespace KidPix.API.AppService.Render.CanvasBrushes
{
    /// <summary>
    /// Represents the current tool selected for use in painting to a <see cref="KidPixArtCanvas"/>
    /// </summary>
    public abstract class KidPixCanvasBrush : IDisposable
    {
        /// <summary>
        /// The name of this <see cref="KidPixCanvasBrush"/>. By default is the <see cref="System.Reflection.MemberInfo.Name"/> property
        /// </summary>
        public string BrushName { get => _name ?? GetType().Name; set => _name = value; }

        public KidPixCanvasBrush(Color primaryColor, double radius)
        {
            PrimaryColor = primaryColor;
            Radius = radius;
        }        

        /// <summary>
        /// The selected color by the user
        /// </summary>
        public Color PrimaryColor
        {
            get => _color;
            set
            {
                _color = value;
                OnColorChanged(value);
            }
        }
        protected Color _color;
        private string? _name;

        public double Radius { get; set; } = 5;

        public KidPixCanvasBrushDrawingFunction BrushDrawingFunction { get; set; } = new RectanglePaintDrawFunction();

        /// <summary>
        /// Returns a new instance of whatever <see cref="Brush"/> corresponds with this <see cref="KidPixCanvasBrush"/> 
        /// </summary>
        /// <returns></returns>
        internal abstract Brush GetMyBrushInternal();
        /// <summary>
        /// This function will draw one dot of the brush to the screen at the specified point using the specified <see cref="Graphics"/> instance
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="PaintPosition"></param>
        /// <param name="PaintPositionOrigin"></param>
        internal virtual void PaintInternal(Graphics graphics, Point PaintPosition, Stroke CurrentStroke)
        {            
            using Brush p = GetMyBrushInternal();
            if (p is TextureBrush texBrush)
            {
                texBrush.TranslateTransform(PaintPosition.X, PaintPosition.Y);
                texBrush.ScaleTransform((float)(Radius * 2) / texBrush.Image.Width, (float)(Radius * 2) / texBrush.Image.Height);
            }
            BrushDrawingFunction.DoPaintingFunction(new() { 
                Graphics = graphics, 
                GDIBrush = p, 
                Radius = Radius 
            }, [PaintPosition, CurrentStroke.StartPosition]);
        }
        /// <summary>
        /// Called when the <see cref="PrimaryColor"/> property changes value
        /// </summary>
        /// <param name="newColor"></param>
        public virtual void OnColorChanged(Color newColor) { }
        public virtual void Dispose() { }    
    }
    /// <summary>
    /// Assists in creating a new <see cref="KidPixCanvasBrush"/> by organizing them all into one <see cref="Type"/>
    /// </summary>
    public static class KidPixCanvasBrushes
    {
        public static KidPixPencilToolBrush Pencil(Color PrimaryColor, double Radius) => new KidPixPencilToolBrush(PrimaryColor, Radius);
        /// <summary>
        /// Creates a new <see cref="KidPixImageCanvasBrush"/> that uses an image you specify to paint an area of the canvas with that image
        /// <para/>Please note that the image you specify will be disposed of when this brush is disposed, so ensure you load from file for this.
        /// </summary>
        /// <param name="PrimaryColor"></param>
        /// <param name="Radius"></param>
        /// <returns></returns>
        public static KidPixImageCanvasBrush ImagePaintBrush(string Name, Bitmap BrushImage, Color PrimaryColor, double Radius) => new KidPixImageCanvasBrush(BrushImage, PrimaryColor, Radius)
        {
            BrushName = Name
        };
    }
}
