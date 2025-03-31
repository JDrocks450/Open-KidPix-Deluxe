using KidPix.API.Importer.Mohawk;
using KidPix.App.UI.Brushes;
using KidPix.App.UI.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KidPix.App.UI.Controls
{    
    /// <summary>
    /// Interaction logic for KPButton.xaml
    /// </summary>
    public partial class KPButton : ToggleButton
    {
        /// <summary>
        /// The <see cref="Background"/> of this <see cref="KPButton"/> determined using a <see cref="KPBrush"/>
        /// </summary>
        public KPBrush KPBackgroundBrush { get => (KPBrush)GetValue(KPBackgroundBrushProperty); set => SetValue(KPBackgroundBrushProperty, value); }
        public static readonly DependencyProperty KPBackgroundBrushProperty = DependencyProperty.Register(nameof(KPBackgroundBrush), typeof(KPBrush), typeof(KPButton));
        /// <summary>
        /// The brush to display this <see cref="KPButton"/> as when the user has toggled it
        /// </summary>
        public KPBrush KPSelectedBrush { get => (KPBrush)GetValue(KPSelectedBrushProperty); set => SetValue(KPSelectedBrushProperty, value); }
        public static readonly DependencyProperty KPSelectedBrushProperty = DependencyProperty.Register(nameof(KPSelectedBrush), typeof(KPBrush), typeof(KPButton));

        public bool ShouldAutoSize { get; set; } = true;
        public bool ShouldAnimationAutoSize { get; set; } = false;
        public double ScaleFactor
        {
            get => (double)GetValue(ScaleFactorProperty);
            set => SetValue(ScaleFactorProperty, value);
        }
        public static readonly DependencyProperty ScaleFactorProperty = DependencyProperty.Register(nameof(ScaleFactor), typeof(double), typeof(KPButton), new(1.0, DoAutoSizeCallback));

        private static void DoAutoSizeCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is KPButton button) button.DoAutoSize();
        }
        private static void DoOnBrushSetCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is KPButton button) { button.DoAutoSize(); }
        }

        public KPButton()
        {
            InitializeComponent();

            Loaded += KPButton_Loaded;
        }

        private async void KPButton_Loaded(object sender, RoutedEventArgs e)
        {
            //**LOAD ANIMATION FRAMES
            if (KPBackgroundBrush == null) return;

            await KPBackgroundBrush.LoadResources();
            await KPBackgroundBrush.InvalidateBrush();
            Background = KPBackgroundBrush;
            DoAutoSize();
            if (KPBackgroundBrush is KPAnimatedImageBrush animBrush)
            {
                animBrush.OnFrameChanged += delegate { if(ShouldAnimationAutoSize) DoAutoSize(); };
                animBrush.OnAnimationStopped += delegate { if (ShouldAnimationAutoSize) DoAutoSize(); };
            }
        }

        private void DoAutoSize()
        {            
            if (Background is ImageBrush imgBrush && imgBrush.ImageSource != null && ShouldAutoSize)
            {
                Width = imgBrush.ImageSource.Width * ScaleFactor;
                Height = imgBrush.ImageSource.Height * ScaleFactor;                
            }
        }

        /// <summary>
        /// START ANIMATION
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            if (KPBackgroundBrush is KPAnimatedImageBrush animBrush)
                animBrush.Play();
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            if (KPBackgroundBrush is KPAnimatedImageBrush animBrush)
                animBrush.Stop();
        }

        private void HOST_Checked(object sender, RoutedEventArgs e)
        {
            if (IsChecked.Value)
                Background = KPSelectedBrush?.BrushReference ?? KPBackgroundBrush?.BrushReference;
            else Background = KPBackgroundBrush?.BrushReference;
            DoAutoSize();
        }
    }
}
