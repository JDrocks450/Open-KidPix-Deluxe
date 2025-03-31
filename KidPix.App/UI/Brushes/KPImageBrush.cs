using KidPix.API;
using KidPix.API.Importer;
using KidPix.API.Importer.Mohawk;
using KidPix.App.UI.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using static KidPix.API.Common.GraphicsExtensions;

namespace KidPix.App.UI.Brushes
{
    /// <summary>
    /// A <see cref="Brush"/>-like class that implements the <see cref="KidPix.API.Importer.Graphics"/> libraries 
    /// to assist in making WPF interface design easier. This is an <see langword="abstract"/> class
    /// </summary>
    public abstract class KPBrush : FrameworkElement
    {
        public static implicit operator Brush(KPBrush Brush) => Brush.BrushReference;

        protected KPBrush() : base()
        {

        }                

        /// <summary>
        /// The Output of this object. This is written to and manipulated by derived classes to create graphics in your interface
        /// </summary>
        public Brush BrushReference
        {
            get
            {
                return _brush;
            }
            protected set
            {
                _brush = value;
                SetValue(BrushReferenceProperty, _brush);
            }
        }
        public static DependencyProperty BrushReferenceProperty = DependencyProperty.Register(nameof(BrushReference), typeof(Brush), typeof(KPBrush));
        private Brush _brush;
        
        /// <summary>
        /// Loads (or reloads) assets used by this object using the <see cref="KidPixUILibrary"/>
        /// </summary>
        /// <returns></returns>
        public abstract Task LoadResources();
        /// <summary>
        /// (Re)draws this <see cref="KPBrush"/>. This should be called at least once. 
        /// </summary>
        /// <returns></returns>
        public abstract Task InvalidateBrush();
        /// <summary>
        /// Casts <see cref="BrushReference"/> to <typeparamref name="T"/> for ease of use
        /// <para/> You can determine what type <typeparamref name="T"/> should be by checking the <see cref="BrushReference"/> property on the <see cref="KPBrush"/> you're using
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetMyBrushInstance<T>() where T : Brush => BrushReference as T;
    }

    /// <summary>
    /// A <see cref="KPBrush"/> that exposes an <see cref="ImageBrush"/> that draws Graphics resources to the screen from the <see cref="KidPixUILibrary"/>
    /// </summary>
    public class KPImageBrush : KPBrush
    {
        protected ImageBrush myBrush => base.BrushReference as ImageBrush;
        protected ImageSource? myResource;
        private CHUNK_TYPE _assetType;
        private ushort _assetID;
        private int _bmhID;
        private bool _lazyLoadQueued = false;

        /// <summary>
        /// The <see cref="ImageBrush"/> used by the <see cref="KPImageBrush"/> to attach to a control in the UI
        /// </summary>
        public new ImageBrush BrushReference => myBrush;

        public KPImageBrush() : base()
        {
            base.BrushReference = new ImageBrush();            
        }

        public KPImageBrush(CHUNK_TYPE assetType, ushort assetID) : this()
        {
            AssetType = assetType;
            AssetID = assetID;            
        }

        public CHUNK_TYPE AssetType
        {
            get => _assetType; 
            set
            {
                _assetType = value;
                DoLazyLoadInvalidate();
            }
        }
        public ushort AssetID
        {
            get => _assetID;
            set
            {
                _assetID = value;
                DoLazyLoadInvalidate();
            }
        }
        public int BMHFrame
        {
            get => _bmhID;
            set
            {
                _bmhID = value;
                DoLazyLoadInvalidate();
            }
        }
        public Color? TransparentColor { get; set; } = default;

        /// <summary>
        /// For palettized <see cref="KPImageBrush"/> images, this will set the color of the palette
        /// <para/>Many brushes and controls use this to change their color based on settings or gameplay mechanics
        /// </summary>
        public Color PalettePrimaryColor
        {
            get => (Color)GetValue(PalettePrimaryColorProperty);
            set => SetValue(PalettePrimaryColorProperty, value);
        }
        public static DependencyProperty PalettePrimaryColorProperty = DependencyProperty.Register(nameof(PalettePrimaryColor), typeof(Color), 
            typeof(KPBrush), new PropertyMetadata(OnPaletteColorChangedCallback));        

        private static void OnPaletteColorChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not KPImageBrush imageBrush) return;
            imageBrush.DoLazyLoadInvalidate();
        }

        protected void DoLazyLoadInvalidate()
        {
            if (_lazyLoadQueued) return;
            _lazyLoadQueued = true;
            _ = myBrush.Dispatcher.InvokeAsync(async delegate
            {
                _lazyLoadQueued = false;
                await LoadResources();
                await InvalidateBrush();
            }, DispatcherPriority.Render);
        }

        protected async Task<ImageBrush?> LibraryImport(int OverrideBMHFrame = -1) => await KidPixUILibrary.ResourceToBrush(new(AssetType, AssetID), OverrideBMHFrame != -1 ? OverrideBMHFrame : BMHFrame, new() 
        { 
            TransparentColor = TransparentColor, 
            PrimaryColor = PalettePrimaryColor,
            Opacity = Opaqueness.Opaque
        });

        public override async Task LoadResources() => myResource = (await LibraryImport())?.ImageSource;

        public override async Task InvalidateBrush()
        {
            if (myResource == null) await LoadResources(); // try to load resource, even if it's null, null is still an acceptable value for ImageSource
            await myBrush.Dispatcher.InvokeAsync(() => myBrush.ImageSource = myResource);            
        }
    }

    /// <summary>
    /// A <see cref="KPBrush"/> that exposes an <see cref="ImageBrush"/> that draws Graphics resources to the screen from the <see cref="KidPixUILibrary"/>
    /// that exclusively uses <see cref="BMHResource"/> frames to create animations
    /// </summary>
    public class KPAnimatedImageBrush : KPImageBrush
    {
        private ImageSource? myHomeResource;
        private List<ImageSource> _animationFrames = new();
        private int _animationClock = 0;
        private bool _isAnimating = false;
        private bool _boomerangParam = false;
        private int _playedCount = 0;

        public enum PlaybackMode
        {
            Loop,
            ReverseLoop,
            Boomerang,
            PlayOnce
        }

        public int CurrentAnimationFrame => _animationClock;
        public TimeSpan AnimationTimeSpan { get; set; } = TimeSpan.FromMilliseconds(1000/15.0); // ~7fps
        public PlaybackMode AnimationMode { get; set; } = PlaybackMode.Loop;
        /// <summary>
        /// The amount of times to repeat the animation timeline.
        /// <para/>-1 is infinite
        /// </summary>
        public int RepeatAmount { get; set; } = -1;

        public delegate void AnimationEventHandler(KPAnimatedImageBrush Brush);
        public event AnimationEventHandler OnFrameChanged, OnAnimationStopped;

        public KPAnimatedImageBrush() : base() { }

        public KPAnimatedImageBrush(CHUNK_TYPE assetType, ushort assetID, int bMHFrame, string AnimationFramesRange) : base(assetType, assetID)
        {
            BMHFrame = bMHFrame;
            this.Range = AnimationFramesRange;
        }

        /// <summary>
        /// This is a string that will determine how the <see cref="AnimationFrames"/> list is created
        /// <para/> Passing numbers separated by commas will load each BMHFrame from the resource determined by <see cref="AssetID"/>
        /// <para/> Passing <c>Number1..Number2</c> will take all frames between Number1 and Number2
        /// <para/> You can use any combination of these you like to create an animation timeline
        /// <code>"1,3..6,7,9,8" will yield: { 1,3,4,5,6,7,9,8 } </code>
        /// </summary>
        public string Range { get; set; }
        public int[] AnimationFrames {
            get
            {
                if (Range == null) return new int[0];
                //PARSE STRING NOW
                //check for commas
                string[] groups = Range.Split(',');
                var myVec = new List<int>();
                foreach (var group in groups)
                {
                    var sanitized = group.Trim();
                    if (group.Contains("..")) // Check for RANGEs
                    {
                        string[] rangeGrouping = sanitized.Split("..");
                        if (rangeGrouping.Length != 2) throw new InvalidOperationException("Range is not properly formatted: " + sanitized);
                        int number1 = 0;
                        bool overrideLogic = false;
                        if (rangeGrouping[0].Contains('_'))
                        {
                            number1 = BMHFrame;
                            overrideLogic = true;
                        }
                        else
                            number1 = int.Parse(rangeGrouping[0]);
                        int number2 = int.Parse(rangeGrouping[1]);
                        if (overrideLogic) 
                            number2 += number1-1;
                        for (int i = number1; i <= number2; i++)
                            myVec.Add(i);
                        continue;
                    }
                    //no range, try normal number parse
                    int number = int.Parse(sanitized);
                    myVec.Add(number);
                }
                return myVec.ToArray();
            }
        }

        public override async Task LoadResources()
        {
            //LOAD THE BASE FRAME FOR IDLE MODE
            await base.LoadResources();
            myHomeResource = myResource;

            //LOAD ANIMATION TIME LINE
            _animationFrames.Clear();
            foreach (int FrameID in AnimationFrames)
            {
                var img = (await LibraryImport(FrameID))?.ImageSource;
                if (img == null) throw new NullReferenceException(nameof(img));
                _animationFrames.Add(img);
            }
            if (!_animationFrames.Any()) return;            
        }

        /// <summary>
        /// Starts playing an animation
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void Play()
        {
            if (_isAnimating)
                throw new InvalidOperationException("This brush is already animating!");
            if (_animationFrames.Count < 2) return; // no frames!!

            InitPlaybackMode(AnimationMode);
            _isAnimating = true;
            Task _animationTask = Task.Run(delegate
            {
                TimeSpan elapsedTime = new();
                TimeSpan updateTimeSpan = TimeSpan.FromMilliseconds(33); // 33ms update time

                lock (_animationFrames)
                { // acquire lock
                    while (_isAnimating)
                    {
                        Task.Delay(updateTimeSpan.Milliseconds).Wait();
                        elapsedTime += updateTimeSpan;
                        if (elapsedTime >= AnimationTimeSpan)
                        {
                            elapsedTime = new();
                            BrushReference.Dispatcher.Invoke(() =>
                            {
                                myResource = _animationFrames[_animationClock % _animationFrames.Count];
                                myBrush.ImageSource = myResource;
                                OnFrameChanged?.Invoke(this);
                            }, System.Windows.Threading.DispatcherPriority.Render);
                            DoPlaybackMode(AnimationMode);
                        }
                    }
                }
            }).ContinueWith(delegate
            { // ensure the frame is reset back to HOME RESOURCE
                BrushReference.Dispatcher.Invoke(() =>
                {
                    myResource = myHomeResource;
                    myBrush.ImageSource = myResource;
                    OnAnimationStopped?.Invoke(this);
                }, System.Windows.Threading.DispatcherPriority.Render);
            });
        }

        public void Stop()
        {
            _isAnimating = false;            
        }

        private void InitPlaybackMode(PlaybackMode Mode)
        {
            _playedCount = 0;
            switch (Mode)
            {
                case PlaybackMode.Boomerang:
                    _boomerangParam = true;
                    goto case PlaybackMode.Loop;
                case PlaybackMode.PlayOnce:
                case PlaybackMode.Loop:
                    _animationClock = 1;
                    break;
                case PlaybackMode.ReverseLoop:
                    _animationClock = _animationFrames.Count;
                    break;                
            }
        }

        private void DoPlaybackMode(PlaybackMode Mode)
        {
            int FRAME_SKIP = 1;
            switch (AnimationMode)
            {
                case PlaybackMode.Loop:
                    _animationClock+= FRAME_SKIP;
                    if (_animationClock + FRAME_SKIP >= int.MaxValue)
                        InitPlaybackMode(Mode);
                    break;
                case PlaybackMode.ReverseLoop:
                    _animationClock-= FRAME_SKIP;
                    if (_animationClock < 0)
                        InitPlaybackMode(Mode);
                    break;
                case PlaybackMode.Boomerang:
                    _animationClock += _boomerangParam ? FRAME_SKIP : -FRAME_SKIP;
                    if (_animationClock >= _animationFrames.Count)
                    {
                        _animationClock = _animationFrames.Count - (FRAME_SKIP) - 1;
                        _boomerangParam = false;
                    }
                    if (_animationClock < 0)
                    {
                        _boomerangParam = true;
                        _animationClock = 1;
                        _playedCount++;
                    }
                    if(RepeatAmount > 0 && _playedCount >= RepeatAmount)                    
                        Stop();                    
                    break;
                case PlaybackMode.PlayOnce:
                    _animationClock++;
                    if (_animationClock > _animationFrames.Count) Stop();
                    break;
            }
        }
    }
}
