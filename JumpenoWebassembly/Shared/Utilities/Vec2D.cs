using System;
using System.Collections.Generic;
using System.Text;

namespace JumpenoWebassembly.Shared.Utilities
{
    public class Vec2D
    {
        public float X { get; set; }
        public float Y { get; set; }
        public Vec2D()
        {

        }

        public Vec2D(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
}
