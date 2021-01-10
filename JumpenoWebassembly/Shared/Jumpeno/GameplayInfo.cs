using static JumpenoWebassembly.Shared.Jumpeno.Enums;

namespace JumpenoWebassembly.Shared.Jumpeno
{
    public class GameplayInfo
    {
        public GameState State { get; set; }

        public int FramesToShrink { get; set; }
        public int FramesToScoreboard { get; set; }

        public bool CountdownTimerRunning { get; set; }
        public bool ShrinkingAllowed { get; set; } = true;
        public bool GameoverTimerRunning { get; set; }

    }
}
