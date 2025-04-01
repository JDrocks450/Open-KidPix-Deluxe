using KidPix.API.AppService.Sessions.Contexts;
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
using KidPix.App.UI.Controls;

namespace KidPix.App.UI.Pages.Easel
{
    /// <summary>
    /// Interaction logic for SizeSelector.xaml
    /// </summary>
    public partial class SizeSelector : UserControl, ITypedVisualObjectChildComponent<EaselUI>
    {
        private KidPixSession? _session;
        private KidPixSession? mySession => _session ?? (_session = ((ITypedVisualObjectChildComponent<EaselUI>)this)?.MyTypedParent?.MySession);
        private KidPixUIContext? sessionUIState => mySession?.UIState;

        public SizeSelector()
        {
            InitializeComponent();

            Loaded += SizeSelector_Loaded;
        }

        private void SizeSelector_Loaded(object sender, RoutedEventArgs e)
        {
            sessionUIState.BrushSizeIndexSelected.ValueChanged += BrushSizeIndexSelected_ValueChanged;
        }

        private void BrushSizeIndexSelected_ValueChanged(API.AppService.Model.KidPixDependencyObject Parent, API.AppService.Model.IKidPixDependencyProperty Property)
        {
            SmallButton.IsChecked = sessionUIState.BrushSizeIndexSelected.Value == KidPixUIEnum.UIBrushSizeSelectionStates.Small;
            MediumButton.IsChecked = sessionUIState.BrushSizeIndexSelected.Value == KidPixUIEnum.UIBrushSizeSelectionStates.Medium;
            LargeButton.IsChecked = sessionUIState.BrushSizeIndexSelected.Value == KidPixUIEnum.UIBrushSizeSelectionStates.Large;
            mySession.GameplayState.SelectedBrushSizeRadius.Value = ((int)sessionUIState.BrushSizeIndexSelected.Value) * 5;
        }

        private void LargeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not KPButton button) return;
            if (!Enum.TryParse<KidPixUIEnum.UIBrushSizeSelectionStates>((string)button.Tag, false, out var SizeEnum)) return;
            sessionUIState.BrushSizeIndexSelected.Value = SizeEnum;
        }
    }
}
