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
    public static class KPGenericControlInterface
    {
        public static void AttachBrushBackground(in UIElement Element, KPBrush AttachBrush)
        {

        }
    }

    public interface IKPBackgroundBrushable
    {
        public KPBrush KPBackgroundBrush { get; set; }
    }

    /// <summary>
    /// Interaction logic for KPButton.xaml
    /// </summary>
    public partial class KPButton : Button, IKPBackgroundBrushable
    {
        private double _scaleFactor = 1.0;

        public KPBrush KPBackgroundBrush { get; set; }

        public bool ShouldAutoSize { get; set; } = true;
        public bool ShouldAnimationAutoSize { get; set; } = false;
        public double ScaleFactor
        {
            get => _scaleFactor; 
            set
            {
                _scaleFactor = value;
                DoAutoSize();
            }
        }        

        public KPButton()
        {
            InitializeComponent();

            Loaded += KPButton_Loaded;
        }

        private async void KPButton_Loaded(object sender, RoutedEventArgs e)
        {
            //**LOAD ANIMATION FRAMES

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
            if (Background is ImageBrush imgBrush && ShouldAutoSize)
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
    }
}
