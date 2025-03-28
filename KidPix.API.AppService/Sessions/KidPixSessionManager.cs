namespace KidPix.API.AppService.Sessions
{
    /// <summary>
    /// Contains all <see cref="KidPixSession"/> instances managed in the current context
    /// <para/>Multiple sessions can be used for multiple tabs containing different art, etc.
    /// </summary>
    public static class KidPixSessionManager
    {
        private static int _nextTicket = 1;
        private static Dictionary<KidPixSessionTicket, KidPixSession> _sessions = new();

        private static KidPixSessionTicket? _activeSession;

        /// <summary>
        /// Gets the currently active session
        /// <para/>
        /// </summary>
        public static KidPixSession? ActiveSession => !_activeSession.HasValue ? null : GetSession(_activeSession.Value);

        /// <summary>
        /// Gets a <see cref="KidPixSession"/> by its <see cref="KidPixSessionTicket"/> (exception if doesn't exist/disposed. See: <see cref="SessionExists(KidPixSessionTicket)"/>)
        /// </summary>
        /// <param name="SessionID"></param>
        /// <returns></returns>
        public static KidPixSession GetSession(KidPixSessionTicket SessionID) => _sessions[SessionID];
        /// <summary>
        /// Adds a new <see cref="KidPixSession"/> and optionally sets it as the new <see cref="ActiveSession"/>
        /// </summary>
        /// <param name="SessionID"></param>
        /// <returns></returns>
        public static KidPixSession CreateSession(bool Activate = true)
        {
            KidPixSessionTicket ticket = new(_nextTicket++);
            while (SessionExists(ticket))
                ticket = new(ticket.SessionID + 1);
            _sessions[ticket] = new KidPixSession(ticket);
            if (_sessions.Count < 2 || Activate) 
                SetActive(ticket);
            return GetSession(ticket);
        }
        /// <summary>
        /// Deletes a <see cref="KidPixSession"/> by its <see cref="KidPixSessionTicket"/>
        /// </summary>
        /// <param name="SessionID"></param>
        /// <returns></returns>
        public static bool CloseSession(KidPixSessionTicket SessionID)
        {
            if (!SessionExists(SessionID)) return false;
            var session = GetSession(SessionID);
            session.Dispose();
            return _sessions.Remove(SessionID);
        }
        /// <summary>
        /// Determines whether the <see cref="KidPixSession"/> with the given <paramref name="SessionID"/> exists in this controller
        /// </summary>
        /// <param name="SessionID"></param>
        /// <returns></returns>
        public static bool SessionExists(KidPixSessionTicket SessionID) => _sessions.ContainsKey(SessionID);
        /// <summary>
        /// Sets the current <see cref="ActiveSession"/>
        /// </summary>
        /// <param name="SessionID"></param>
        /// <returns></returns>
        public static void SetActive(KidPixSessionTicket SessionID) => _activeSession = SessionExists(SessionID) ? SessionID : _activeSession;
    }
}
