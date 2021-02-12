using JumpenoWebassembly.Shared.Constants;
using System.Numerics;

namespace JumpenoWebassembly.Server.Components.Jumpeno.Entities
{
    /// <summary>
    /// Reprezentuje platformu, po ktorej skáču hráči
    /// </summary>
    public class Platform : JumpenoComponent
    {
        public Platform(Vector2 position)
        {
            //Animation = new Animation(texture, new Vector2(1, 1), out Vector2 bodySize);
            Body.Size = new Vector2(MapC.TileSize, MapC.TileSize); // = bodySize;
            Body.Origin = Body.Size / 2;
            Body.Position = position;
            Solid = true;
        }
    }
}
