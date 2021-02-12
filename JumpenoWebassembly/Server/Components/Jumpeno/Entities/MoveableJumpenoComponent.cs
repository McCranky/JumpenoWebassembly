using System;
using System.Numerics;
using System.Threading.Tasks;
using static JumpenoWebassembly.Shared.Jumpeno.Enums;

namespace JumpenoWebassembly.Server.Components.Jumpeno.Entities
{
    /// <summary>
    /// Umožňuje pohyb základnému prvku hry
    /// </summary>
    public class MoveableJumpenoComponent : JumpenoComponent
    {
        protected bool[] Movement = { false, false, false, false, false }; // UP, LEFT, DOWN, RIGHT
        protected Vector2 Velocity = new Vector2(0, 0);
        public float JumpHeight { get; protected set; } = 200f;
        public float SpeedBase { get; protected set; } = 100;
        public bool CanJump { get; set; }
        public bool Falling { get { return Velocity.Y > 0; } }
        public bool RightColission { get; set; }
        public bool LeftColission { get; set; }
        public void OnCollision(Vector2 direction)
        {
            if (direction.X < 0f) { // left
                Velocity.X = 0;
                LeftColission = true;
            } else if (direction.X > 0f) { // right
                Velocity.X = 0;
                RightColission = true;
            }
            if (direction.Y > 0f) { // bottom
                Velocity.Y = 0;
                CanJump = true;
            } else if (direction.Y < 0f) { // top
                Velocity.Y = 0;
            }
        }
        public void SetMovement(MovementDirection action, bool active)
        {
            Movement[(int)action] = active;
        }

        public override async Task Update(int fpsTick)
        {
            if (Y > Map.Y) {
                Visible = false;
                return;
            }

            //plynule zastavenie
            Velocity.X *= 0.6f;
            if (MathF.Abs(Velocity.X) < 0.2) {
                Velocity.X = 0;
            }

            if (Movement[(int)MovementDirection.Right]) {
                Velocity.X += SpeedBase;
            }
            if (Movement[(int)MovementDirection.Left]) {
                Velocity.X -= SpeedBase;
            }
            if (Movement[(int)MovementDirection.Jump] && CanJump) {
                CanJump = false;
                Velocity.Y = -MathF.Sqrt(3f * 981f * JumpHeight);
            }

            Velocity.Y += 1581f * (1f / 60f); // gravitacia

            if (State != AnimationState.Dead) {
                if (Falling && !CanJump) {
                    State = AnimationState.Falling;
                    FacingRight = Velocity.X <= 0;
                } else {
                    if (Velocity.X == 0) {
                        State = AnimationState.Idle;
                    } else {
                        State = AnimationState.Walking;
                        FacingRight = Velocity.X <= 0;
                    }
                }
            }


            Body.Position = Body.Position + Velocity * (1 / 60f);

            //DEBUG player position
            //System.Console.WriteLine($"Player at: [{X}, {Y}]");
            await base.Update(fpsTick);
        }
    }
}
