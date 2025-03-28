using KidPix.API.AppService.Sessions;
using KidPix.API.AppService.Sessions.Contexts;
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
        private KidPixUIContext? sessionUIState => mySession?.GameplayState?.UIState;

        public ToolCupboard()
        {
            InitializeComponent();
        }

        private void PaletteSplooshIcon_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            sessionUIState.BigPickerOpened.Value = true;
        }

        private void PaletteSplooshIcon_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            sessionUIState.BigPickerOpened.Value = false;
        }
    }
}
