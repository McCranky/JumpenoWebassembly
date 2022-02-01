using JumpenoWebassembly.Server.Components.Jumpeno.Entities;
using JumpenoWebassembly.Server.Hubs;
using JumpenoWebassembly.Shared.Constants;
using JumpenoWebassembly.Shared.Jumpeno.Utilities;
using JumpenoWebassembly.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using static JumpenoWebassembly.Shared.Jumpeno.Enums;

namespace JumpenoWebassembly.Server.Components.Jumpeno.Game
{
    /// <summary>
    /// Reprezentuje hracie pole, kde sa vyskytujú platformi a hráči.
    /// </summary>
    public class Map
    {
        private readonly int _tileSize;
        private readonly int _horizontalTilesCount;
        private readonly int _verticalTilesCount;
        public List<Platform> Platforms { get; set; }
        public string BackgroundColor { get; set; }
        public float X { get; private set; }
        public float Y { get; private set; }

        private readonly GameEngine _game;

        public Map(GameEngine game, MapTemplate template)
        {
            _game = game;
            _tileSize = MapC.TileSize;
            _horizontalTilesCount = MapC.HorizontalTilesCount;
            _verticalTilesCount = MapC.VerticalTilesCount;
            Platforms = new List<Platform>();
            // pokiaľ z nejakého dôvodu nemáme pripravené žiadne mapy, tak sa aplikuje prednastavená varianta, aby bolo na čom hrať
            if (template != null)
            {
                X = template.Width * _tileSize;
                Y = template.Height * _tileSize;
                BackgroundColor = template.BackgroundColor;

                GeneratePlatforms(template.Tiles);
            }
            else
            {
                X = _horizontalTilesCount * _tileSize;
                Y = _verticalTilesCount * _tileSize;
                BackgroundColor = "rgb(36, 30, 59)";

                GeneratePlatforms(null);
            }
        }

        private void GeneratePlatforms(string template)
        {
            if (template == null)
            {
                template = "00000000000000000000000000000000" +
                           "00000000000000000000000000000000" +
                           "00000000000000000000000000000000" +
                           "00000000000000000000000000000000" +
                           "00000000000000000000000000000000" +
                           "00000000000001111110000000000000" +
                           "00000000000000000000000000000000" +
                           "00000000000000000000000000000000" +
                           "00001111000000000000000011111111" +
                           "00000000000000000000000000000000" +
                           "00000000000000000000000000000000" +
                           "00000000000000000000000000000000" +
                           "10000000000111111100000000000000" +
                           "11000000000000000000000000000000" +
                           "11100000000000000000000000000011" +
                           "11111110000000000000011100000111" +
                           "11111111000000000000111111111111" +
                           "11111111100000000001111111111111";
            }
            int width = (int)X / _tileSize;
            int height = (int)Y / _tileSize;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (template[Conversions.Map2DToIndex(i, j, width)] == '1')
                    {
                        Platforms.Add(new Platform(new Vector2(i * _tileSize, j * _tileSize)));
                    }
                }
            }

        }

        public void SpawnPlayers()
        {
            foreach (var player in _game.PlayersInGame)
            {
                player.Map = this;
                player.Alive = true;
                player.Visible = true;
                player.SetBody();
                PositionPlayer(player);
            }
        }

        /// <summary>
        /// Používa sa pri hernom režime Guided, kedy sa môže hráč napojiť počas odpočtu.
        /// </summary>
        /// <param name="player"></param>
        public void SpawnPlayer(Player player)
        {
            player.Map = this;
            player.Alive = true;
            player.Visible = true;
            player.SetBody();
            PositionPlayer(player);
        }

        /// <summary>
        /// Zmenšovanie mapy
        /// </summary>
        /// <param name="platforms"></param>
        /// <param name="playerPositions"></param>
        public float Shrink()
        {
            float move = 64 * (1f / GameEngine._FPS);
            X -= move;

            foreach (var platform in Platforms)
            {
                platform.X -= move / 2f;
            }

            foreach (var player in _game.PlayersInGame)
            {
                player.X -= move / 2f;
            }

            return move;
        }

        /// <summary>
        /// Metóda aktualizuje všetkých hráčov a skontroluje kolízie.
        /// </summary>
        /// <param name="fpsTickNum"></param>
        /// <param name="hub"></param>
        /// <returns></returns>
        public async Task Update(int fpsTickNum, IHubContext<GameHub> hub)
        {
            var positions = new Dictionary<float, Tuple<Vector2, AnimationState>>();
            foreach (var p in _game.PlayersInGame)
            {
                positions.Add(p.Id, new Tuple<Vector2, AnimationState>(p.Body.Position, p.State));
                await p.Update(fpsTickNum);
            }

            // player and walls collision
            foreach (var player in _game.PlayersInGame)
            {
                if (!player.Alive)
                {
                    continue;
                }
                // top border
                if (player.Y < 0)
                {
                    player.Y = 0;
                    player.OnCollision(new Vector2(0, -1));
                }
                // bottom border
                if (player.Y > Y - player.Body.Size.Y)
                {
                    player.Y = Y - player.Body.Size.Y;
                    player.OnCollision(new Vector2(0, 1));
                }
                // left border
                if (player.X <= 0)
                {
                    player.X = 0;
                    player.LeftColission = true;
                }
                //right border
                if (player.X >= X - player.Body.Size.X)
                {
                    player.X = X - player.Body.Size.X;
                    player.RightColission = true;
                }
            }

            // player and platform collision
            foreach (var player in _game.PlayersInGame)
            {
                if (!player.Alive)
                {
                    continue;
                }
                foreach (var platform in Platforms)
                {
                    if (!platform.Visible)
                    {
                        continue;
                    }
                    var collision = player.GetCollider().CheckCollision(platform.GetCollider(), 0);
                    if (collision != default)
                    {
                        player.OnCollision(collision);
                    }
                }
            }

            // players collision
            foreach (var pl1 in _game.PlayersInGame)
            {
                if (!pl1.Alive)
                {
                    continue;
                }
                foreach (var pl2 in _game.PlayersInGame)
                {
                    if (!pl2.Alive || pl1 == pl2)
                    {
                        continue;
                    }
                    var collision = pl1.GetCollider().CheckCollision(pl2.GetCollider(), 0);
                    if (collision == default) continue;
                    if (collision.Y > 55 && collision.Y < 70 && pl1.Falling)
                    { // skocil mu na hlavu
                        pl1.Kills += 1;
                        pl1.OnCollision(collision);
                        pl2.Die();
                        await hub.Clients.Group(_game.Settings.GameCode).SendAsync(GameHubC.PlayerDied, pl2.Id, pl1.Id);
                        --_game.PlayersAllive;
                        if (_game.PlayersAllive == 1)
                        {
                            return;
                        }
                    }
                }

            }

            // send new player positions to clients && check for player crush while map shrinking
            foreach (var player in _game.PlayersInGame)
            {
                if (positions[player.Id].Item1 != player.Body.Position || positions[player.Id].Item2 != player.State)
                {
                    Console.WriteLine($"Position difference: [{(positions[player.Id].Item1 - player.Body.Position)}]");
                    await hub.Clients.Group(_game.Settings.GameCode).SendAsync(GameHubC.PlayerMoved, new PlayerPosition { Id = player.Id, X = player.X, Y = player.Y, FacingRight = player.FacingRight, State = player.State });
                }


                if (player.LeftColission && player.RightColission)
                {
                    if (_game.PlayersAllive == 1)
                    {
                        return;
                    }

                    player.Die();
                    await hub.Clients.Group(_game.Settings.GameCode).SendAsync(GameHubC.PlayerCrushed, player.Id);
                    --_game.PlayersAllive;
                }

                player.LeftColission = false;
                player.RightColission = false;
            }
        }

        /// <summary>
        /// Nastaví hráčovi takú pozíciu, aby nekolidoval so žiadnou platformou a stenou
        /// </summary>
        /// <param name="player"></param>
        private void PositionPlayer(Player player)
        {
            Console.WriteLine($"Positioning {player.Name} between X:[0, {(int)X - (int)player.Body.Size.X}] Y:[0, {(int)Y - (int)player.Body.Size.Y}]");
            var rnd = new Random();
            bool hit;

            do
            {
                hit = false;
                player.X = rnd.Next(0, (int)X - (int)player.Body.Size.X);
                player.Y = rnd.Next(0, (int)Y - (int)player.Body.Size.Y);
                Console.WriteLine($"Positioned {player.Name} at [{player.X}, {player.Y}]");
                foreach (var platform in Platforms)
                {
                    if (player.GetCollider().CheckCollision(platform.GetCollider(), 0, false) != default)
                    {
                        Console.WriteLine("Collision!");
                        hit = true;
                        break;
                    }
                }
            } while (hit);

            Console.WriteLine($"{player.Name} positioned on [{player.X}, {player.Y}].");
        }
    }
}
