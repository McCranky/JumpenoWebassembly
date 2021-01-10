﻿namespace JumpenoWebassembly.Shared.Jumpeno
{
    public static class Enums
    {
        public enum GameMode
        {
            Player,
            Guided
        }

        public enum GameState
        {
            Lobby,
            Countdown,
            Shrinking,
            Gameover,
            Scoreboard,
            Deleted
        }

        public enum AnimationState
        {
            Idle, Walking, Falling, Dead
        }

        public enum MovementDirection
        {
            Left, Right, Jump
        }
    }
}
