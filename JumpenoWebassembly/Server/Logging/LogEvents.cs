namespace JumpenoWebassembly.Server.Logging
{
    public class LogEvents
    {
        public const int StartGame = 1000;
        public const int EndGame = 1001;

        public const int GeneratePlayerPosition = 2000;
        public const int GetPlayerPosition = 2001;

        public const int NotifyCollision = 3001;
        public const int NotifyEliminationByPlayer = 3002;
        public const int NotifyEliminationByMapShrink = 3003;
    }
}