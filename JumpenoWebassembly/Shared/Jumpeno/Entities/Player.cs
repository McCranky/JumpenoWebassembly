namespace JumpenoWebassembly.Shared.Jumpeno.Entities
{
    /**
     * Reprezentuje telo hráča s ktorým sa pohybuje
     */
    public class Player
    {
        public int Id { get; set; }
        public bool Spectator { get; set; } = false;
        public int Kills { get; set; }
        public bool Alive { get; set; }
        public bool InGame { get; set; }
        public string Skin { get; set; }
        public bool SmallScreen { get; set; } = false;
        public string Name { get; set; }
        public string CssStyle { get; set; }
    }
}
