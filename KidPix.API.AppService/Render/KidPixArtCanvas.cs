using KidPix.API.AppService.Model;
using KidPix.API.AppService.Render.CanvasBrushes;
using System.Drawing;
using System.Numerics;
using static KidPix.API.AppService.Render.CanvasBrushes.KidPixCanvasBrush;

namespace KidPix.API.AppService.Render
{    
    /// <summary>
    /// Exposes functionality to create and manipulate a Bitmap with special functionality for User Control
    /// <para/>You should use this to help you display a new canvas to draw stuff on
    /// </summary>
    public class KidPixArtCanvas : KidPixDependencyObject, IDisposable
    {
        private Graphics _graphics;
        private Point MousePosition = new(0,0);
        
        private Stroke? _currentStroke;

        /// <summary>
        /// Changes the currently selected tool (Brush) to be the new one.
        /// <para/>Changing tool mid <see cref="Stroke"/> is not allowed and the stroke will be cancelled upon setting 
        /// this value to a different tool (brush)
        /// </summary>
        public KidPixDependencyProperty<KidPixCanvasBrush?> SelectedTool { get; } = RegisterProperty<KidPixCanvasBrush?>(OnBrushChange);
        private static void OnBrushChange(KidPixDependencyObject Sender,
            KidPixCanvasBrush? OldBrush, KidPixCanvasBrush? NewBrush)
        {
            KidPixArtCanvas? instance = Sender as KidPixArtCanvas;
            if (instance.IsPainting)
                instance.StopStroke();
        }

        /// <summary>
        /// Determines whether the user is currently Painting
        /// </summary>
        public bool IsPainting => _currentStroke != null;
        /// <summary>
        /// The settings for the current Canvas
        /// <para/>Changing this can be done using <see cref="Clear(KidPixArtCanvasDefinition?)"/>
        /// </summary>
        public KidPixArtCanvasDefinition CanvasDefinition { get; private set; }
        /// <summary>
        /// A reference to the <see cref="Bitmap"/> this <see cref="KidPixArtCanvas"/> is altering
        /// </summary>
        public Bitmap CanvasImage { get; private set; }

        /// <summary>
        /// Creates a new Blank canvas with the default <see cref="KidPixArtCanvasDefinition"/>
        /// </summary>
        public KidPixArtCanvas()
        {
            CanvasDefinition = KidPixArtCanvasDefinition.Default;
            Clear();
        }

        public KidPixArtCanvas(KidPixArtCanvasDefinition Definition)
        {
            CanvasDefinition = Definition;
            Clear();
        }

        /// <summary>
        /// Starts (or continues) a new <see cref="Stroke"/> -- usually when the user clicks the mouse down to start (or continue) painting.
        /// <para/>This <see cref="Stroke"/> will begin at the last call to <see cref="CommitStroke(int, int)"/>
        /// <para/>If a <see cref="Stroke"/> is in progress, it will do processing to ensure the paint stroke is continuous from this new call since the last call
        /// in accordance to the guidelines/logic of the <see cref="SelectedTool"/>
        /// <para/>For instance, if using a standard Pen this will linearly interpolate between this new call and the last for instances where the user moves the pen very quickly
        /// </summary>
        public void CommitStroke(int X, int Y, KidPixCanvasBrush.PaintingCoordinateOrigin CoordinateOrigin = KidPixCanvasBrush.PaintingCoordinateOrigin.TopLeft)
        {
            if (SelectedTool.Value == default)
                throw new InvalidOperationException("You cannot commit a Stroke right now. You haven't selected a tool or brush yet.");

            Point currentPos = new(X, Y);
            if (CoordinateOrigin == PaintingCoordinateOrigin.TopLeft)
                currentPos = new Point(X - (int)SelectedTool.Value.Radius, Y - (int)SelectedTool.Value.Radius);

            if (_currentStroke == null)
                _currentStroke = new Stroke(currentPos);
            else if (_currentStroke.CurrentPosition == currentPos && !_currentStroke.ContinuousSprayMode)
                return; // The brush hasn't moved and continuous spray mode isn't on, cancel this stroke
            
            MousePosition = currentPos;
            SelectedTool.Value.BrushDrawingFunction.DoDrawFunction(_graphics, SelectedTool.Value, currentPos, _currentStroke);
            _currentStroke.CurrentPosition = MousePosition;
        }
        /// <summary>
        /// Stops the current <see cref="Stroke"/> and adds a new history item to the Undo stack to allow the user to 
        /// Undo/Redo to a point before/after this stroke
        /// </summary>
        public void StopStroke()
        {
            _currentStroke = null;
        }

        /// <summary>
        /// Clears the current <see cref="KidPixArtCanvas"/> and allows you to reuse this instance with a new <see cref="KidPixArtCanvasDefinition"/>
        /// <para/>This will clear all art on the canvas
        /// </summary>
        /// <param name="NewSettings"></param>
        public void Clear(KidPixArtCanvasDefinition? NewSettings = default)
        {
            if (NewSettings != null)
                CanvasDefinition = NewSettings;
            Dispose(); // dispose of old bitmap image

            CanvasImage = new Bitmap(CanvasDefinition.Width, CanvasDefinition.Height);
            _graphics = Graphics.FromImage(CanvasImage);            

            //Fill the canvas with white
            using Brush whiteBrush = new SolidBrush(Color.White);
            _graphics.FillRectangle(whiteBrush, new Rectangle(0,0,CanvasDefinition.Width,CanvasDefinition.Height));
            _graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
            _graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.GammaCorrected;
        }

        /// <summary>
        /// Disposes the <see cref="Graphics"/>, <see cref="Brush"/> and <see cref="CanvasImage"/> used by this object
        /// </summary>
        public void Dispose()
        {
            _graphics?.Dispose();
            _graphics = null;
            CanvasImage?.Dispose();
            CanvasImage = null;
        }
    }
}
