﻿using System.Numerics;
using static JumpenoWebassembly.Shared.Jumpeno.Enums;

namespace JumpenoWebassembly.Shared.Jumpeno.Entities
{
    /// <summary>
    /// Umožnuje animovať zvhľad tela
    /// </summary>
    public class Animation
    {
        public static readonly string[] _Skins = { "mageSprite_aer", "mageSprite_water", "mageSprite_earth", "mageSprite_fire", "mageSprite_magic" };
        public Vector2 Posiotion { get; set; }
        public string TextureName { get; }
        public string CssTexturePathBig => "images/big/" + TextureName;
        public string TexturePathBig => "images/big/" + TextureName;
        public string CssTexturePathSmall => "images/small/" + TextureName;
        public string TexturePathSmall => "images/small/" + TextureName;
        public Vector2 Size { get; set; }
        public AnimationState State { get; set; } = AnimationState.Idle;
        public int CurrentImage { get; set; } = 0;
        public int ImageCount { get; set; }
        public string CssStyle => $@"
            width: {Size.X}px;
            height: {Size.Y}px;
            background: url({CssTexturePathBig}) {-Posiotion.X}px {-Posiotion.Y}px;
            ";

        public Animation(string texture, Vector2 proportion, out Vector2 bodySize)
        {
            TextureName = texture;
            System.Console.WriteLine(TexturePathBig);

            Size = new Vector2(64, 76);
            bodySize = new Vector2(Size.X, Size.Y);
            Posiotion = new Vector2 { X = 0, Y = 0 };
            ImageCount = (int)proportion.X;
        }

        public string GetFrameStyle(AnimationState ofState, int frame = 0)
        {
            return $@"
            width: {Size.X}px;
            height: {Size.Y}px;
            background: url({CssTexturePathBig}) {-(Size.X * frame)}px {-(Size.Y * (int)ofState)}px;
            ";
        }

        public void Update(int fpsTick)
        {
            CurrentImage = (CurrentImage + 1) % ImageCount;

            if (State == AnimationState.Dead) {
                CurrentImage = 0;
            } else if (State == AnimationState.Falling) {
                CurrentImage = 1;
            }

            if (State == AnimationState.Dead) {
                Posiotion = new Vector2 {
                    Y = Size.Y * 2,
                    X = Size.X * CurrentImage
                };
            } else {
                Posiotion = new Vector2 {
                    Y = Size.Y * (int)State,
                    X = Size.X * CurrentImage
                };
            }
        }
    }
}

