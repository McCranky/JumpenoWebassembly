using System;
using System.Collections.Generic;
using System.Text;

namespace JumpenoWebassembly.Shared.Jumpeno
{
    public class GameInfo
    {
        public int PlayersLimit { get; set; } = 2;
        public string GameName { get; set; }
        public string GameCode { get; set; }
        public string Error { get; set; }
        public Enums.GameMode GameMode { get; set; } = Enums.GameMode.Player;
    }
}
