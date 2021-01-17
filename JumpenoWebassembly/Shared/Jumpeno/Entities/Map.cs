using JumpenoWebassembly.Shared.Models;
using System;

namespace JumpenoWebassembly.Shared.Jumpeno.Entities
{
    public class Map
    {
        public const int _TileSize = 64;
        public MapInfo Info { get; set; }

        public string CssStyle(bool smallScreen) => smallScreen ? $@"
            width: {(int)Math.Round(Info.X / 2, 0)}px;
            height: {(int)Math.Round(Info.Y / 2, 0)}px; 
            background-color: {Info.Background};
            " : $@"
            width: {(int)Math.Round(Info.X, 0)}px;
            height: {(int)Math.Round(Info.Y, 0)}px; 
            background-color: {Info.Background};
            ";
    }
}
