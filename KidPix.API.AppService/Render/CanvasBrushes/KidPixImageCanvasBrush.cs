using KidPix.API.Util;
using System.Drawing;
using System.Drawing.Imaging;

namespace KidPix.API.AppService.Render.CanvasBrushes
{
    /// <summary>
    /// Creates a new <see cref="KidPixImageCanvasBrush"/> with some custom features
    /// <para/>It makes use of the <see cref="Opaqueness.SemiOpaque"/> palette mode which makes all brushes somewhat transparent
    /// when palettized with the Primary Color
    /// </summary>
    public class KidPixImageCanvasBrush : KidPixCanvasBrush
    {
        public Bitmap BrushImage { get; private set; }

        public double Opacity { get; set; } = .20;

        public KidPixImageCanvasBrush(Bitmap brushImage, Color primaryColor, double radius) : base(primaryColor, radius)
        {
            BrushImage = brushImage;
            OnColorChanged(primaryColor);
        }

        internal override Brush GetMyBrushInternal()
        {
            float alphaNorm = (float)Math.Max(0,Math.Min(1,Opacity));
            ImageAttributes imgAtts = new ImageAttributes();            
            imgAtts.SetWrapMode(System.Drawing.Drawing2D.WrapMode.Clamp);
            ColorMatrix matrix = new ColorMatrix(new float[][]{
                new float[] {1F, 0, 0, 0, 0},
                new float[] {0, 1F, 0, 0, 0},
                new float[] {0, 0, 1F, 0, 0},
                new float[] {0, 0, 0, alphaNorm, 0},
                new float[] {0, 0, 0, 0, 1F}});
            imgAtts.SetColorMatrix(matrix);
            return new TextureBrush(BrushImage, new Rectangle(0,0,BrushImage.Width,BrushImage.Height), imgAtts);
        }

        internal override void PaintInternal(Graphics graphics, Point PaintPosition, PaintingCoordinateOrigin PaintPositionOrigin)
        {
            if (PaintPositionOrigin == PaintingCoordinateOrigin.TopLeft)
                PaintPosition = new Point(PaintPosition.X - (int)Radius, PaintPosition.Y - (int)Radius);
            using TextureBrush p = (TextureBrush)GetMyBrushInternal();
            p.TranslateTransform(PaintPosition.X, PaintPosition.Y);
            p.ScaleTransform((float)(Radius * 2) / p.Image.Width, (float)(Radius * 2) / p.Image.Height);
            graphics.FillEllipse(p, new Rectangle(PaintPosition, new Size((int)(Radius * 2), (int)(Radius * 2))));
        }

        public override void OnColorChanged(Color newColor)
        {
            if (BrushImage != null)
                BrushImage.Palette = KidPix.API.Common.GraphicsExtensions.MakePaletteFromPrimaryColor(Common.GraphicsExtensions.Opaqueness.SemiOpaque, newColor);
            base.OnColorChanged(newColor);
        }

        public override void Dispose()
        {
            BrushImage?.Dispose();
            BrushImage = null;

            base.Dispose();
        }
    }
}
