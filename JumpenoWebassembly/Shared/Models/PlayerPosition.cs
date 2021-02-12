using static JumpenoWebassembly.Shared.Jumpeno.Enums;

namespace JumpenoWebassembly.Shared.Models
{
    /// <summary>
    /// Reprezentuje polohu hraca spolu so stavom animacie
    /// </summary>
    public class PlayerPosition
    {
        public float Id { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public bool FacingRight { get; set; }
        public AnimationState State { get; set; }
    }
}
