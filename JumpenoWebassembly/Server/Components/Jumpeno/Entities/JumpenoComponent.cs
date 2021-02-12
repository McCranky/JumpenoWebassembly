using JumpenoWebassembly.Server.Components.Jumpeno.Game;
using System;
using System.Numerics;
using System.Threading.Tasks;
using static JumpenoWebassembly.Shared.Jumpeno.Enums;

namespace JumpenoWebassembly.Server.Components.Jumpeno.Entities
{
    /// <summary>
    /// Reprezentuje základnú časť hry a obsahuje všetky potrebné informácie pre vykreslovanie
    /// </summary>
    public class JumpenoComponent
    {
        public string Name { get; set; }
        public bool Visible { get; set; } = true;
        public Body Body { get; set; } = new Body(0, 0, 0);
        public float X { set { Body.Position = new Vector2(value, Body.Position.Y); } get { return Body.Position.X; } }
        public float Y { set { Body.Position = new Vector2(Body.Position.X, value); } get { return Body.Position.Y; } }
        public bool Solid { set; get; } = false; // able to walk thru
        public bool FacingRight { get; set; } = true;
        public Map Map { get; set; }
        public AnimationState State { get; set; } = AnimationState.Idle;

        public Collider GetCollider()
        {
            return new Collider(Body, Solid);
        }

        public virtual async Task Update(int fpsTickNum)
        {
            await Task.CompletedTask;
        }
    }
}
