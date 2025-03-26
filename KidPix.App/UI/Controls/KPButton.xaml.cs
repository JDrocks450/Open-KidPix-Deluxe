using KidPix.API.Importer.Mohawk;
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
    public class KPImageBrush
    {
        public CHUNK_TYPE AssetType { get; set; }
        public ushort AssetID { get; set; }
        public int BMHFrame { get; set; } = -1;
        public int BMHAnimationFrames { get; set; } = 1;
    }

    /// <summary>
    /// Interaction logic for KPButton.xaml
    /// </summary>
    public partial class KPButton : Button
    {
        private List<ImageSource> _animationFrames = new();

        private int _animationClock = 0;
        private Task? _animationTask;

        public KPImageBrush KPImageBrush
        {
            get; set;
        } 

        public KPButton()
        {
            InitializeComponent();

            Loaded += KPButton_Loaded;
        }

        private async void KPButton_Loaded(object sender, RoutedEventArgs e)
        {            
            //**LOAD ANIMATION FRAMES
            for(int i = 0; i < KPImageBrush.BMHAnimationFrames; i++)
            {
                var img = await KidPixUILibrary.ResourceToBrush(new API.MHWKIdentifierToken(KPImageBrush.AssetType, (ushort)(KPImageBrush.AssetID )), KPImageBrush.BMHFrame + i);
                if (img == null) throw new NullReferenceException(nameof(img));
                _animationFrames.Add(img.ImageSource);
            }
            Background = new ImageBrush(_animationFrames[0]);
            Width = _animationFrames[0].Width;
            Height = _animationFrames[0].Height;
        }

        /// <summary>
        /// START ANIMATION
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            if (_animationFrames.Count < 2) return;

            _animationClock = 1;
            if (_animationTask != null)
                _animationTask.Dispose();
            _animationTask = Task.Run(delegate
            {
                bool mouseOver = true;
                while (mouseOver)
                {
                    Task.Delay(1000/7).Wait(); // 5fps
                    Dispatcher.Invoke(() =>
                    {
                        Background = new ImageBrush(_animationFrames[_animationClock % _animationFrames.Count]);
                        mouseOver = IsMouseOver;
                    }, System.Windows.Threading.DispatcherPriority.Render);
                    _animationClock++;
                }
            });
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Background = new ImageBrush(_animationFrames[0]);
            }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }
    }
}
