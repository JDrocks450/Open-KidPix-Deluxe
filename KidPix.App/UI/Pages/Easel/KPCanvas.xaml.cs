using KidPix.API.AppService.Render;
using KidPix.API.AppService.Sessions;
using KidPix.App.UI.Model;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KidPix.App.UI.Pages.Easel
{
    /// <summary>
    /// Interaction logic for KPCanvas.xaml
    /// </summary>
    public partial class KPCanvas : Canvas, IDisposable, ITypedVisualObjectChildComponent<EaselUI>
    {        
        public static double USER_POLL_FPS = 80.0;
        public static TimeSpan USER_POLL_FREQUENCY = TimeSpan.FromSeconds(1 / USER_POLL_FPS);

        private Timer _userPollThreadTimer;
        private bool isUpdating = false;
        private KidPixSession? _session;

        private KidPixSession? mySession => _session ?? (_session = ((ITypedVisualObjectChildComponent<EaselUI>)this)?.MyTypedParent?.MySession);
        private KidPixArtCanvas myCanvas;

        public KPCanvas()
        {
            InitializeComponent();

            Loaded += KPCanvas_Loaded;
        }

        ~KPCanvas()
        {
            Dispose();
        }

        private void KPCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            myCanvas = mySession.GameplayState.ArtCanvas;
            DisplayArtCanvas();
        }

        private void DisplayArtCanvas()
        {
            ArtCanvasDisplay.Source = myCanvas.CanvasImage.Convert(false);
            ArtCanvasDisplay.Width = myCanvas.CanvasDefinition.Width;
            ArtCanvasDisplay.Height = myCanvas.CanvasDefinition.Height;
        }

        private void CreateWorkerThread()
        {
            if (_userPollThreadTimer != null) return;
            _userPollThreadTimer = new Timer(UserPollThreadCallback, null, USER_POLL_FREQUENCY, USER_POLL_FREQUENCY);
            isUpdating = true;
        }

        private void Canvas_MouseEnter(object sender, MouseEventArgs e)
        {
            if (_userPollThreadTimer == null) 
                CreateWorkerThread();
        }

        private void Canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            Dispose();
        }
        
        public void Dispose()
        {
            isUpdating = false;
            if (_userPollThreadTimer != null)
                lock (_userPollThreadTimer)
                {
                    _userPollThreadTimer?.Dispose();
                    _userPollThreadTimer = null;
                }
        }

        private void UpdateFrame()
        {
            var mousePos = Mouse.GetPosition(this);
            var pos = mousePos - new Point(20/2,20/2);
            SetLeft(UserSelectionCursor, pos.X);
            SetTop(UserSelectionCursor, pos.Y);
            myCanvas.SetBrushCursorPosition((int)pos.X, (int)pos.Y);
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                myCanvas.DrawEllipse(System.Drawing.Color.Green, 10);
                DisplayArtCanvas();
            }
        }

        //***THREAD CALLBACK

        private void UserPollThreadCallback(object? state) => Dispatcher.InvokeAsync(UpdateFrame);
    }
}
