﻿using static JumpenoWebassembly.Shared.Jumpeno.Enums;

namespace JumpenoWebassembly.Shared.Jumpeno
{
    public class GameInfo
    {
        public int PlayersLimit { get; set; } = 2;
        public string GameName { get; set; }
        public string GameCode { get; set; }
        public GameMode GameMode { get; set; } = GameMode.Player;
    }
}
