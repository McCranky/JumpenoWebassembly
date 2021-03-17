using System;

namespace JumpenoWebassembly.Shared.Models
{
    public class MeasurePoint
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int GamesCount { get; set; }
        public int PlayersCount { get; set; }
        public float CPU { get; set; }
        public long Memory { get; set; }
    }
}
