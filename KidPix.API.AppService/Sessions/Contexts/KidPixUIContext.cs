using KidPix.API.AppService.Model;

namespace KidPix.API.AppService.Sessions.Contexts
{
    /// <summary>
    /// Contains runtime info relating to the current state of the user interface
    /// </summary>
    public class KidPixUIContext : KidPixDependencyObject
    {
        public KidPixDependencyProperty<KidPixUIEnum.EaselToolSubpageUIStates> ToolSubpageState { get; } = RegisterProperty(KidPixUIEnum.EaselToolSubpageUIStates.Pencils);
        public KidPixDependencyProperty<bool> BigPickerOpened { get; } = RegisterProperty(false);
    }
}
