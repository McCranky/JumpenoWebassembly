using System;

namespace JumpenoWebassembly.Shared.Models
{
    public class MeasurePoint
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int GamesCount { get; set; }
        public int PlayersCount { get; set; }
        public double CPU { get; set; }
        public double Memory { get; set; }
    }
}
