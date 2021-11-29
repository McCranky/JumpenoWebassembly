using JumpenoWebassembly.Shared.Utilities;
using System;
using static JumpenoWebassembly.Shared.Jumpeno.Enums;

namespace JumpenoWebassembly.Shared.Jumpeno.Entities
{
    /// <summary>
    /// Reprezentuje telo hráča s ktorým sa pohybuje
    /// </summary>
    public class Player
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Skin { get; set; }
        public bool Spectator { get; set; } = false;

        public int Kills { get; set; }
        public bool Alive { get; set; }
        public bool Visible { get; set; }
        public bool InGame { get; set; }

        public Vector Position { get; set; }

        public Animation Animation { get; set; }


        public bool SmallScreen { get; set; } = false;
        public bool FacingRight { get; set; }

        public string CssClass => GetType().Name.ToLower() + (FacingRight ? " flippedHorizontal" : "");
        public string CssStyle => SmallScreen ? $@"
            top: {(int)Math.Round(Position.Y / 2, 0)}px ;
            left: {(int)Math.Round(Position.X / 2, 0)}px ;
            width: {Animation.Size.X / 2}px;
            height: {Animation.Size.Y / 2}px;
            background: url({Animation.CssTexturePathSmall}) {-Animation.Posiotion.X / 2}px {-Animation.Posiotion.Y / 2}px;
            " : $@"
            top: {(int)Math.Round(Position.Y, 0)}px ;
            left: {(int)Math.Round(Position.X, 0)}px ;
            width: {Animation.Size.X}px;
            height: {Animation.Size.Y}px;
            background: url({Animation.CssTexturePathBig}) {-Animation.Posiotion.X}px {-Animation.Posiotion.Y}px;
            ";

        public void SetBody()
        {
            Animation = new Animation(Skin + ".png", new Vector(4, 3), out _);
        }

        public void Die()
        {
            Alive = false;
            Animation.State = AnimationState.Dead;
        }
    }
}
