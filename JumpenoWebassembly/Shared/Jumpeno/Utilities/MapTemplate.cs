using System;
using System.Collections.Generic;

namespace JumpenoWebassembly.Shared.Jumpeno.Utilities
{
    /**
     * Reprezentuje informácie o mape, ktoré su zapisovane alebo čitane z binarneho suboru, alebo použite pri inicializacii mapy.
     */
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
