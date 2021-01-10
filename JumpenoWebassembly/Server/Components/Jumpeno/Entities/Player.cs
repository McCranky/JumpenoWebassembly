using System;
using System.Numerics;
using static JumpenoWebassembly.Shared.Jumpeno.Enums;

namespace JumpenoWebassembly.Server.Components.Jumpeno.Entities
{
    /**
     * Reprezentuje telo hráča s ktorým sa pohybuje
     */
    public class Player : MoveableJumpenoComponent
    {
        public long Id { get; set; }
        public bool Spectator { get; set; } = false;
        public int Kills { get; set; }
        public bool Alive { get; set; }
        public bool InGame { get; set; }
        public string Skin { get; set; }
        public bool SmallScreen { get; set; } = false;
        public override string CssStyle(bool smallScreen) => smallScreen ? $@"
            top: {(int)Math.Round(Y / 2, 0)}px ;
            left: {(int)Math.Round(X / 2, 0)}px ;
            width: {Animation.Size.X / 2}px;
            height: {Animation.Size.Y / 2}px;
            background: url({Animation.CssTexturePathSmall}) {-Animation.Posiotion.X / 2}px {-Animation.Posiotion.Y / 2}px;
            " : $@"
            top: {(int)Math.Round(Y, 0)}px ;
            left: {(int)Math.Round(X, 0)}px ;
            width: {Animation.Size.X}px;
            height: {Animation.Size.Y}px;
            background: url({Animation.CssTexturePathBig}) {-Animation.Posiotion.X}px {-Animation.Posiotion.Y}px;
            ";

        public void SetBody()
        {
            //Animation = new Animation(Skin + ".png", new Vector2(4, 3), out Vector2 bodySize);
            Body.Size = new Vector2(64, 64);//= bodySize;
            Body.Origin = Body.Size / 2;
        }

        public void Die()
        {
            Alive = false;
            Animation.State = AnimationState.Dead;
            Velocity.Y = 0;
        }

        internal void Freeze()
        {
            for (int i = 0; i < Movement.Length; i++) {
                Movement[i] = false;
            }
        }
    }
}
