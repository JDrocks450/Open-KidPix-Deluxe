using KidPix.API.AppService.Model;
using KidPix.API.AppService.Sessions;
using KidPix.API.AppService.Sessions.Contexts;
using KidPix.App.UI.Model;
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
    public partial class ToolSubpage : Page, INotifyPropertyChanged, ITypedVisualObjectChildComponent<EaselUI>
    {
        private KidPixSession? mySession => _session ?? (_session = ((ITypedVisualObjectChildComponent<EaselUI>)this)?.MyTypedParent?.MySession);
        private KidPixUIContext? sessionUIState => mySession?.GameplayState?.UIState;
        
        public event PropertyChangedEventHandler? PropertyChanged;

        public KidPixUIEnum.EaselToolSubpageUIStates CurrentState
        {
            get => (KidPixUIEnum.EaselToolSubpageUIStates)GetValue(CurrentStateProperty);
            set => SetValue(CurrentStateProperty, value);
        }
        public static DependencyProperty CurrentStateProperty = DependencyProperty.Register(nameof(CurrentState), typeof(KidPixUIEnum.EaselToolSubpageUIStates), typeof(ToolSubpage));
        private KidPixSession? _session;

        public ToolSubpage()
        {
            InitializeComponent();

            Loaded += ToolSubpage_Loaded;            
        }

        private void ToolSubpage_Loaded(object sender, RoutedEventArgs e)
        {
            //**ACQUIRE HOOKS TO KidPixSession of the Easel parent
            if (sessionUIState == null) return; // not good, this component will not work at all            
            CurrentStateProperty.Bind(this,sessionUIState.ToolSubpageState);
        }

        private void FirstSection_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            sessionUIState.ToolSubpageState.Value++;
        }
    }
}
