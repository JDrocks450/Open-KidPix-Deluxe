using KidPix.API.AppService.Model;
using KidPix.API.AppService.Render;
using KidPix.API.AppService.Render.CanvasBrushes;
using System.Drawing;

namespace KidPix.API.AppService.Sessions.Contexts
{
    /// <summary>
    /// Contains runtime info relating to the current state of play
    /// </summary>
    public class KidPixGameplayContext : KidPixDependencyObject
    {
        public static readonly Color Default_Color = Color.LightGreen;

        public KidPixDependencyProperty<KidPixGameplayStates> GameplayState { get; } = RegisterProperty<KidPixGameplayStates>();

        /// <summary>
        /// The current state of UI variables
        /// </summary>
        public KidPixUIContext UIState { get; } = new();
        /// <summary>
        /// The image that this session allows the User to play with
        /// </summary>
        public KidPixArtCanvas ArtCanvas { get; } = new();
        /// <summary>
        /// The currently selected Primary Color for use when drawing/painting
        /// <para/>Default color is <see cref="Color.LightGreen"/>
        /// </summary>
        public KidPixDependencyProperty<Color> SelectedPrimaryColor { get; } = RegisterProperty(Default_Color);
        /// <summary>
        /// The currently selected tool for use on the <see cref="ArtCanvas"/>.
        /// <para/>These are the settings used to determine what the effect of the tool should be on the canvas
        /// </summary>
        public KidPixDependencyProperty<KidPixCanvasBrush?> SelectedCanvasBrush => ArtCanvas.SelectedTool;
        /// <summary>
        /// The currently selected tool size for use on the <see cref="ArtCanvas"/>.
        /// </summary>
        public KidPixDependencyProperty<double> SelectedBrushSizeRadius { get; } = new KidPixDependencyProperty<double>(10);
    }
}
