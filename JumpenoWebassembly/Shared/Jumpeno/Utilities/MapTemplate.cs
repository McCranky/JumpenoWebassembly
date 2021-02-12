namespace JumpenoWebassembly.Shared.Jumpeno.Utilities
{
    /// <summary>
    /// Reprezentuje informácie o mape, ktoré su zapisovane alebo čitane z databazy, alebo použite pri inicializacii mapy.
    /// </summary>
    public class MapTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string BackgroundColor { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Tiles { get; set; } // znázorňuje, či sa na danom políčku nachádza platforma alebo nie
    }
}
