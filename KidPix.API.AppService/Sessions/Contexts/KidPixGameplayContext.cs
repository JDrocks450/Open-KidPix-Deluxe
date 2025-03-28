using KidPix.API.AppService.Model;
using KidPix.API.AppService.Render;

namespace KidPix.API.AppService.Sessions.Contexts
{
    /// <summary>
    /// Contains runtime info relating to the current state of play
    /// </summary>
    public class KidPixGameplayContext : KidPixDependecyObject
    {
        public KidPixDependencyProperty<KidPixGameplayStates> GameplayState { get; } = RegisterProperty<KidPixGameplayStates>();

        /// <summary>
        /// The current state of UI variables
        /// </summary>
        public KidPixUIContext UIState { get; } = new();
        /// <summary>
        /// The image that this session allows the User to play with
        /// </summary>
        public KidPixArtCanvas ArtCanvas { get; } = new();
    }
}
