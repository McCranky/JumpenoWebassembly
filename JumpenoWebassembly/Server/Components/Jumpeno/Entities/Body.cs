using System.Numerics;

namespace JumpenoWebassembly.Server.Components.Jumpeno.Entities
{
    /// <summary>
    /// Reprezentuje telo objektu hry, ktore ma sovju pozíciu, veľkosť a stred
    /// </summary>
    public class Body
    {
        public Vector2 Size { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Origin { get; set; }

        public Body(float sizeX = 0, float sizeY = 0, float positionX = 0, float positionY = 0, float originX = 0, float originY = 0)
        {
            Size = new Vector2(sizeX, sizeY);
            Position = new Vector2(positionX, positionY);
            Origin = new Vector2(originX, originY);
        }
    }
}
