using KidPix.API.AppService.Model;

namespace KidPix.API.AppService.Sessions.Contexts
{
    /// <summary>
    /// Contains runtime info relating to the current state of the user interface
    /// </summary>
    public class KidPixUIContext : KidPixDependencyObject
    {
        /// <summary>
        /// The currently opened ToolSubpage page
        /// </summary>
        public KidPixDependencyProperty<KidPixUIEnum.EaselToolSubpageUIStates> ToolSubpageState { get; } = RegisterProperty(KidPixUIEnum.EaselToolSubpageUIStates.Pencils);
        /// <summary>
        /// Determines whether the color picker is open or not
        /// </summary>
        public KidPixDependencyProperty<bool> BigPickerOpened { get; } = RegisterProperty(false);
        /// <summary>
        /// Maps to the size selections for a brush which can appear as a control in the Tool Subpage with three circles illustrating sizes from small to large
        /// </summary>
        public KidPixDependencyProperty<KidPixUIEnum.UIBrushSizeSelectionStates> BrushSizeIndexSelected { get; } = RegisterProperty(KidPixUIEnum.UIBrushSizeSelectionStates.None);
    }
}
