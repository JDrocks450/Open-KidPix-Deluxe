using KidPix.API.AppService.Sessions;
using KidPix.API.Importer.Graphics;
using KidPix.App.UI.Model;
using KidPix.App.UI.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
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

namespace KidPix.App.UI.Pages.Easel
{
    /// <summary>
    /// Interaction logic for BigColorPicker.xaml
    /// </summary>
    public partial class BigColorPicker : UserControl, ITypedVisualObjectChildComponent<EaselUI>
    {
        private KidPixSession? _session;
        private KidPixSession? mySession => _session ?? (_session = ((ITypedVisualObjectChildComponent<EaselUI>)this)?.MyTypedParent?.MySession);
        private System.Drawing.Bitmap _paletteBitmap;

        public BigColorPicker()
        {
            KidPixUILibrary.LinkResource("BigPicker.MHK");                        

            InitializeComponent();
        }

        private async void BigColorPicker_Loaded(object sender, RoutedEventArgs e)
        {
            mySession.UIState.BigPickerOpened.ValueChanged += PropertyChanged;

            //*load palette bmp
            API.Importer.Mohawk.CHUNK_TYPE AssetType = API.Importer.Mohawk.CHUNK_TYPE.tBMH;
            ushort AssetID = 1500;
            int BMHFrame = 6;
            using BMHResource? bmh = await KidPixUILibrary.TryImportResourceLinked<BMHResource>(new(AssetType, AssetID));
            bmh.SetCurrentResource(BMHFrame);
            _paletteBitmap = bmh.Paint();
        }

        private void PropertyChanged(API.AppService.Model.KidPixDependencyObject Parent, API.AppService.Model.IKidPixDependencyProperty Property)
        {
            if (mySession.UIState.BigPickerOpened.Value) Open();
            else Close();
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mySession.UIState.BigPickerOpened.Value = false; // close as the user could be trying to dismiss the control
            if (_paletteBitmap == null) return;
            if (!PaletteImageControl.IsMouseOver) return;
            Point mousePos = Mouse.GetPosition(PaletteImageControl);
            System.Drawing.Color selectedColor = _paletteBitmap.GetPixel((int)mousePos.X % _paletteBitmap.Width, (int)mousePos.Y % _paletteBitmap.Height);
            mySession.GameplayState.SelectedPrimaryColor.Value = selectedColor;
        }

        void Open() => BeginAnimation(Canvas.BottomProperty, (DoubleAnimation)FindResource("BigPickerOpenAnimationKey"));
        void Close() => BeginAnimation(Canvas.BottomProperty, (DoubleAnimation)FindResource("BigPickerCloseAnimationKey"));
    }
}
