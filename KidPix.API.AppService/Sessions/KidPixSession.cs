using KidPix.API.AppService.Sessions.Contexts;

namespace KidPix.API.AppService.Sessions
{

    /// <summary>
    /// Contains runtime info like current brush selected, the paint canvas data, etc.
    /// </summary>
    public class KidPixSession : IDisposable
    {
        public KidPixSessionTicket SessionID { get; private set; }
        /// <summary>
        /// Determines the current state of gameplay variables
        /// </summary>
        public KidPixGameplayContext GameplayState { get; } = new();
        /// <summary>
        /// The current state of UI variables
        /// </summary>
        public KidPixUIContext UIState { get; } = new();

        internal KidPixSession(KidPixSessionTicket sessionID)
        {
            SessionID = sessionID;
        }

        public void Dispose()
        {
            ;
        }
    }
}
