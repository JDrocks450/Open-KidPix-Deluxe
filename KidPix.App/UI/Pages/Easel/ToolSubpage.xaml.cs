using KidPix.API.AppService.Model;
using KidPix.API.AppService.Render.CanvasBrushes;
using KidPix.API.AppService.Render.DrawFunctions;
using KidPix.API.AppService.Sessions;
using KidPix.API.AppService.Sessions.Contexts;
using KidPix.App.UI.Brushes;
using KidPix.App.UI.Controls;
using KidPix.App.UI.Model;
using KidPix.App.UI.Util;
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
        private KidPixUIContext? sessionUIState => mySession?.UIState;
        
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

            //**We must bind the pencil primary color property to the SelectedColor property in gameplay state
            var brushInstance = ((KPImageBrush)FindResource("UI_SelectedPrimaryColorBrush"));
            KPImageBrush.PalettePrimaryColorProperty.BindKPtoWPFOneWay(brushInstance,
                mySession.GameplayState.SelectedPrimaryColor, (System.Drawing.Color c) => c.ToMediaColor());

            //**debug bind selected tool label to selected tool
            TextBlock.TextProperty.BindKPtoWPFOneWay(DEBUG_SelectedToolLabel, mySession.GameplayState.SelectedCanvasBrush, (KidPixCanvasBrush? selectedBrush) => selectedBrush.BrushName);
        }

        private async void DrawToolButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not KPButton button) return;
            if (button.Tag is not string EnumName) return;
            if (string.IsNullOrWhiteSpace(EnumName)) return;
            if (!Enum.TryParse<KidPixUILibrary.KPUtilBrushes>(EnumName, true, out KidPixUILibrary.KPUtilBrushes ToolType)) return;
            //----            
            System.Drawing.Color primColor = mySession.GameplayState.SelectedPrimaryColor.Value;
            double radiusSize = mySession.GameplayState.SelectedBrushSizeRadius.Value;
            mySession.GameplayState.SelectedCanvasBrush.Value = await KidPixUILibrary.CreateBrush(ToolType, primColor, radiusSize);
        }

        private void FreePaintDrawModeShape_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => 
            mySession.GameplayState.SelectedCanvasBrush.Value.BrushDrawingFunction = KidPixCanvasDrawFunctions.FreePaintFunction;

        private void RectangleDrawModeShape_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => 
            mySession.GameplayState.SelectedCanvasBrush.Value.BrushDrawingFunction = KidPixCanvasDrawFunctions.RectangleFillFunction;

        private void CircleDrawModeShape_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => 
            mySession.GameplayState.SelectedCanvasBrush.Value.BrushDrawingFunction = KidPixCanvasDrawFunctions.CircularFillFunction;
    }
}
