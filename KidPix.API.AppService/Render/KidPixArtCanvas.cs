﻿using KidPix.API.AppService.Model;
using System.Drawing;
using System.Numerics;
using static KidPix.API.AppService.Render.KidPixCanvasBrush;

namespace KidPix.API.AppService.Render
{    
    /// <summary>
    /// Represents the current tool selected for use in painting to a <see cref="KidPixArtCanvas"/>
    /// </summary>
    public abstract class KidPixCanvasBrush
    {
        public KidPixCanvasBrush(Color primaryColor, double radius)
        {
            PrimaryColor = primaryColor;
            Radius = radius;
        }

        public enum PaintingCoordinateOrigin
        {
            TopLeft,
            Center
        }

        /// <summary>
        /// The selected color by the user
        /// </summary>
        public Color PrimaryColor { get; set; }
        public double Radius { get; set; } = 5;

        internal abstract Brush GetMyBrushInternal();
        internal abstract void PaintInternal(Graphics graphics, Point PaintPosition, PaintingCoordinateOrigin PaintPositionOrigin);
    }
    /// <summary>
    /// A basic tool that draws a circular area of a given size and color.
    /// <para/>This tool uses no external resources and is self-contained
    /// </summary>
    public class KidPixPencilToolBrush : KidPixCanvasBrush
    {
        public KidPixPencilToolBrush(Color primaryColor, double radius) : base(primaryColor, radius) { }

        internal override Brush GetMyBrushInternal() => new SolidBrush(PrimaryColor);

        internal override void PaintInternal(Graphics graphics, Point PaintPosition, PaintingCoordinateOrigin PaintPositionOrigin)
        {
            if (PaintPositionOrigin == PaintingCoordinateOrigin.TopLeft)
                PaintPosition = new Point(PaintPosition.X - (int)Radius, PaintPosition.Y - (int)Radius);
            using Brush p = GetMyBrushInternal();
            graphics.FillEllipse(p, new Rectangle(PaintPosition, new Size((int)(Radius * 2), (int)(Radius * 2))));
        }
    }

    /// <summary>
    /// Exposes functionality to create and manipulate a Bitmap with special functionality for User Control
    /// <para/>You should use this to help you display a new canvas to draw stuff on
    /// </summary>
    public class KidPixArtCanvas : KidPixDependecyObject, IDisposable
    {
        private Graphics _graphics;
        private Point MousePosition = new(0,0);

        private class Stroke
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
        }
        private Stroke? _currentStroke;
        private KidPixDependencyProperty<KidPixCanvasBrush?> _selectedBrush = RegisterProperty<KidPixCanvasBrush?>();

        /// <summary>
        /// Changes the currently selected tool (Brush) to be the new one.
        /// <para/>Changing tool mid <see cref="Stroke"/> is not allowed and the stroke will be cancelled upon setting 
        /// this value to a different tool (brush)
        /// </summary>
        public KidPixDependencyProperty<KidPixCanvasBrush?> SelectedTool
        {
            get => _selectedBrush;
            set
            {
                _selectedBrush = value;
                OnBrushChange();
            }
        }
        private void OnBrushChange()
        {
            if (IsPainting)
                StopStroke();
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
            if (_currentStroke == null)
                _currentStroke = new Stroke(currentPos);
            else if (_currentStroke.CurrentPosition == currentPos && !_currentStroke.ContinuousSprayMode)
                return; // The brush hasn't moved and continuous spray mode isn't on, cancel this stroke

            MousePosition = currentPos;
            
            Point drawPoint = currentPos;
            double strokeLen = Vector2.Distance(new(X,Y), new(_currentStroke.CurrentPosition.X, _currentStroke.CurrentPosition.Y));
            int totalStrokeLen = (int)Math.Round(strokeLen);

            if (strokeLen > 0)
            {
                for (int i = 0; i < totalStrokeLen; i++)
                {
                    float percentage = i / (float)totalStrokeLen;
                    var lerpedPoint = Vector2.Lerp(new(X, Y), new(_currentStroke.CurrentPosition.X, _currentStroke.CurrentPosition.Y), percentage);
                    drawPoint = new((int)Math.Round(lerpedPoint.X), (int)Math.Round(lerpedPoint.Y));
                    SelectedTool.Value.PaintInternal(_graphics, drawPoint, CoordinateOrigin);
                }
            }
            else SelectedTool.Value.PaintInternal(_graphics, drawPoint, CoordinateOrigin);

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
