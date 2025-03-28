using KidPix.API.AppService.Sessions;
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

        public BigColorPicker()
        {
            KidPixUILibrary.LinkResource("BigPicker.MHK");

            InitializeComponent();
        }

        private void BigColorPicker_Loaded(object sender, RoutedEventArgs e)
        {
            mySession.GameplayState.UIState.BigPickerOpened.ValueChanged += PropertyChanged;
        }

        private void PropertyChanged(API.AppService.Model.KidPixDependecyObject Parent, API.AppService.Model.IKidPixDependencyProperty Property)
        {
            if (mySession.GameplayState.UIState.BigPickerOpened.Value) Open();
            else Close();
        }

        void Open()
        {
            BeginAnimation(Canvas.BottomProperty, (DoubleAnimation)FindResource("BigPickerOpenAnimationKey"));
        }
        void Close()
        {
            BeginAnimation(Canvas.BottomProperty, (DoubleAnimation)FindResource("BigPickerCloseAnimationKey"));
        }
    }
}
