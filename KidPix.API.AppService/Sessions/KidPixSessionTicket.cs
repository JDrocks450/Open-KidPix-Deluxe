namespace KidPix.API.AppService.Sessions
{
    /// <summary>
    /// Identifies a <see cref="KidPixSession"/> in the <see cref="KidPixSessionManager"/>
    /// </summary>
    public struct KidPixSessionTicket
    {
        public KidPixSessionTicket(int sessionID)
        {
            SessionID = sessionID;
        }

        public int SessionID { get; }
    }
}
