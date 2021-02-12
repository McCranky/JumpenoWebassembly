namespace JumpenoWebassembly.Shared.Jumpeno.Game
{
    /// <summary>
    /// Informacie o stave lobby fázy hry.
    /// Stav odpočtu (dodnota a či je povoleny)
    /// </summary>
    public class LobbyInfo
    {
        public int FramesToStart { get; set; }
        public bool StartTimerRunning { get; set; }
        public bool StoppedStartTimer { get; set; }
        public bool DeleteTimerRunning { get; set; }

    }
}
