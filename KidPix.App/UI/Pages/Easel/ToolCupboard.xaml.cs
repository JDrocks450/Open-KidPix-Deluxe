using KidPix.API.AppService.Sessions;
using KidPix.API.AppService.Sessions.Contexts;
using KidPix.App.UI.Brushes;
using KidPix.App.UI.Controls;
using KidPix.App.UI.Model;
using System.Windows.Controls;

namespace KidPix.App.UI.Pages.Easel
{    
    /// <summary>
    /// Interaction logic for ToolCupboard.xaml
    /// </summary>
    public partial class ToolCupboard : Page, ITypedVisualObjectChildComponent<EaselUI>
    {
        private KidPixSession? _session;
        private KidPixSession? mySession => _session ?? (_session = ((ITypedVisualObjectChildComponent<EaselUI>)this)?.MyTypedParent?.MySession);
        private KidPixUIContext? sessionUIState => mySession?.UIState;

        public ToolCupboard()
        {
            InitializeComponent();

            Loaded += ToolCupboard_Loaded;
        }

        private void ToolCupboard_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //**We must bind the paint sploosh image brush palette primary color property to the SelectedColor property in gameplay state
            var brushInstance = ((KPImageBrush)FindResource("PaletteSplashBrushKey"));
            KPImageBrush.PalettePrimaryColorProperty.BindKPtoWPFOneWay(brushInstance,
                mySession.GameplayState.SelectedPrimaryColor,(System.Drawing.Color c) => c.ToMediaColor());

            //**attach ui buttons to ensure they check and uncheck based on what mode of play we're in
            BindUIButtonsToSubpageUIState();
        }

        private void BindUIButtonsToSubpageUIState()
        {
            //uses a converter to test the current state of the ui subpage if it matches that of the button
            API.AppService.Model.KidPixDependencyProperty<KidPixUIEnum.EaselToolSubpageUIStates> state = mySession.UIState.ToolSubpageState;
            KPButton.IsCheckedProperty.BindKPtoWPFOneWay(DrawButton, state, (KidPixUIEnum.EaselToolSubpageUIStates e) => e == KidPixUIEnum.EaselToolSubpageUIStates.Pencils);
            KPButton.IsCheckedProperty.BindKPtoWPFOneWay(PaintButton, state, (KidPixUIEnum.EaselToolSubpageUIStates e) => e == KidPixUIEnum.EaselToolSubpageUIStates.Paints);
            KPButton.IsCheckedProperty.BindKPtoWPFOneWay(SelectionButton, state, (KidPixUIEnum.EaselToolSubpageUIStates e) => e == KidPixUIEnum.EaselToolSubpageUIStates.ClippingSelections);
            KPButton.IsCheckedProperty.BindKPtoWPFOneWay(FillButton, state, (KidPixUIEnum.EaselToolSubpageUIStates e) => e == KidPixUIEnum.EaselToolSubpageUIStates.Fills);
            KPButton.IsCheckedProperty.BindKPtoWPFOneWay(EraserButton, state, (KidPixUIEnum.EaselToolSubpageUIStates e) => e == KidPixUIEnum.EaselToolSubpageUIStates.Erasers);
            KPButton.IsCheckedProperty.BindKPtoWPFOneWay(ABCButton, state, (KidPixUIEnum.EaselToolSubpageUIStates e) => e == KidPixUIEnum.EaselToolSubpageUIStates.Text);
            KPButton.IsCheckedProperty.BindKPtoWPFOneWay(AnimationsButton, state, (KidPixUIEnum.EaselToolSubpageUIStates e) => e == KidPixUIEnum.EaselToolSubpageUIStates.Animations);
            KPButton.IsCheckedProperty.BindKPtoWPFOneWay(BackgroundsButton, state, (KidPixUIEnum.EaselToolSubpageUIStates e) => e == KidPixUIEnum.EaselToolSubpageUIStates.Backgrounds);
            KPButton.IsCheckedProperty.BindKPtoWPFOneWay(AudioButton, state, (KidPixUIEnum.EaselToolSubpageUIStates e) => e == KidPixUIEnum.EaselToolSubpageUIStates.Audios);
            KPButton.IsCheckedProperty.BindKPtoWPFOneWay(MixersButton, state, (KidPixUIEnum.EaselToolSubpageUIStates e) => e == KidPixUIEnum.EaselToolSubpageUIStates.Mixers);
            KPButton.IsCheckedProperty.BindKPtoWPFOneWay(StampsButton, state, (KidPixUIEnum.EaselToolSubpageUIStates e) => e == KidPixUIEnum.EaselToolSubpageUIStates.Stamps);
            KPButton.IsCheckedProperty.BindKPtoWPFOneWay(StickersButton, state, (KidPixUIEnum.EaselToolSubpageUIStates e) => e == KidPixUIEnum.EaselToolSubpageUIStates.Stickers);
        }

        private void PaletteSplooshIcon_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            sessionUIState.BigPickerOpened.Value = true;
        }

        private void PaletteSplooshIcon_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //sessionUIState.BigPickerOpened.Value = false;
        }

        private void ToolCupboardItemClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            API.AppService.Model.KidPixDependencyProperty<KidPixUIEnum.EaselToolSubpageUIStates> toolSubpageState = mySession.UIState.ToolSubpageState;
            if (sender == DrawButton) toolSubpageState.Value = KidPixUIEnum.EaselToolSubpageUIStates.Pencils; //PENS
            else if (sender == PaintButton) toolSubpageState.Value = KidPixUIEnum.EaselToolSubpageUIStates.Paints; // PAINTS
            else if (sender == AudioButton) toolSubpageState.Value = KidPixUIEnum.EaselToolSubpageUIStates.Audios; // AUDIOS
            else if (sender == SelectionButton) toolSubpageState.Value = KidPixUIEnum.EaselToolSubpageUIStates.ClippingSelections; // CLIPPING
            else if (sender == FillButton) toolSubpageState.Value = KidPixUIEnum.EaselToolSubpageUIStates.Fills; // FILL TOOL
            else if (sender == EraserButton) toolSubpageState.Value = KidPixUIEnum.EaselToolSubpageUIStates.Erasers; // ERASE
            else if (sender == AnimationsButton) toolSubpageState.Value = KidPixUIEnum.EaselToolSubpageUIStates.Animations; // ANIMATIONS
            else if (sender == BackgroundsButton) toolSubpageState.Value = KidPixUIEnum.EaselToolSubpageUIStates.Backgrounds; // BACKGROUNDS
            else if (sender == ABCButton) toolSubpageState.Value = KidPixUIEnum.EaselToolSubpageUIStates.Text; // TEXT
            else if (sender == MixersButton) toolSubpageState.Value = KidPixUIEnum.EaselToolSubpageUIStates.Mixers; // Mixers
            else if (sender == StampsButton) toolSubpageState.Value = KidPixUIEnum.EaselToolSubpageUIStates.Stamps; // STAMPS
            else if (sender == StickersButton) toolSubpageState.Value = KidPixUIEnum.EaselToolSubpageUIStates.Stickers; // STAMPS
        }
    }
}
