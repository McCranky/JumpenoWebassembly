using System;

namespace JumpenoWebassembly.Shared.Jumpeno.Entities
{
    /// <summary>
    /// Reprezentuje platformu, po ktorej skáču hráči
    /// </summary>
    public class Platform
    {
        public float X { get; set; }
        public float Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public string CssStyle(bool smallScreen) => smallScreen ? $@"
            position: absolute;
            top: {(int)Math.Round(Y / 2, 0)}px ;
            left: {(int)Math.Round(X / 2, 0)}px ;
            width: {Width / 2}px ;
            height: {Height / 2}px ;
            background: url(images/small/tile.png) 0px 0px;
            " : $@"
            position: absolute;
            top: {(int)Math.Round(Y, 0)}px ;
            left: {(int)Math.Round(X, 0)}px ;
            width: {Width}px ;
            height: {Height}px ;
            background: url(images/big/tile.png) 0px 0px;
            ";
    }
}
