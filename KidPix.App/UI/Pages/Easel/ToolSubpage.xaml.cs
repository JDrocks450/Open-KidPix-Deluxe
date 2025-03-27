using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for ToolSubpage.xaml
    /// </summary>
    public partial class ToolSubpage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public enum UIStates
        {
            //**UI TRAY TYPE 1
            Pencils,
            Paints,
            Fills,
            Erasers,
            Mixers,
            ClippingSelections,
            //**UI TRAY TYPE 2

        }

        public UIStates CurrentState
        {
            get => (UIStates)GetValue(CurrentStateProperty);
            set
            {
                PropertyChanged?.Invoke(this, new(nameof(CurrentTray)));
                SetValue(CurrentStateProperty, value);
            }
        } 

        public int CurrentTray => (int)CurrentState;
        public static DependencyProperty CurrentStateProperty = DependencyProperty.Register(nameof(CurrentState), typeof(UIStates), typeof(ToolSubpage));

        public ToolSubpage()
        {
            InitializeComponent();
        }

        private void FirstSection_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CurrentState++;
        }
    }
}
