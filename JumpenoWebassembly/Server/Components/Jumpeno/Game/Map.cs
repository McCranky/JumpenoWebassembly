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

namespace JumpenoWebassembly.Server.Components.Jumpeno.Game
{
    /**
     * Reprezentuje hracie pole, sa vyskytujú platformi a hráči.
     */
    public class Map
    {
        public const int _TileSize = 64;
        public List<Platform> Platforms { get; set; }
        public string BackgroundColor { get; set; }
        public float X { get; private set; }
        public float Y { get; private set; }

        private readonly GameEngine _game;

        public Map(GameEngine game, MapTemplate template)
        {
            _game = game;
            Platforms = new List<Platform>();
            // pokiaľ z nejakého dôvodu nemáme pripravené žiadne mapy, tak sa aplikuje prednastavená varianta, aby bolo na čom hrať
            if (template != null) {
                X = template.Width * _TileSize;
                Y = template.Height * _TileSize;
                BackgroundColor = template.BackgroundColor;

                GeneratePlatforms(template.Tiles);
            } else {
                X = 16 * _TileSize;
                Y = 9 * _TileSize;
                BackgroundColor = "rgb(36, 30, 59)";

                GeneratePlatforms(null);
            }
        }

        private void GeneratePlatforms(bool[] template)
        {
            if (template != null) {
                int width = (int)X / _TileSize;
                int height = (int)Y / _TileSize;
                for (int i = 0; i < width; i++) {
                    for (int j = 0; j < height; j++) {
                        if (template[Conversions.Map2DToIndex(i, j, width)]) {
                            Platforms.Add(new Platform("tile.png", new Vector2(i * _TileSize, j * _TileSize)));
                        }
                    }
                }
            } else { // ak nemáme template, tak vygenerujeme aspoň pár platforiem aby boli na mape nejake prekažky
                Platforms.Add(new Platform("tile.png", new Vector2(0 * _TileSize, 8 * _TileSize)));
                Platforms.Add(new Platform("tile.png", new Vector2(1 * _TileSize, 8 * _TileSize)));
                Platforms.Add(new Platform("tile.png", new Vector2(6 * _TileSize, 6 * _TileSize)));
                Platforms.Add(new Platform("tile.png", new Vector2(5 * _TileSize, 6 * _TileSize)));
            }

        }

        public void SpawnPlayers()
        {
            foreach (var player in _game.PlayersInGame) {
                player.Game = _game;
                player.Alive = true;
                player.Visible = true;
                player.SetBody();
                PositionPlayer(player);
            }
        }

        /**
         * Používa sa pri hernom režime Guided, kedy sa môže hráč napojiť počas odpočtu.
         */
        public void SpawnPlayer(Player player)
        {
            player.Game = _game;
            player.Alive = true;
            player.Visible = true;
            player.SetBody();
            PositionPlayer(player);
        }

        /**
         * Zmenšovanie mapy
         */
        public void Shrink(out List<Shared.Jumpeno.Entities.Platform> platforms, out List<PlayerPosition> playerPositions)
        {
            float move = 64 * (1f / GameEngine._FPS);
            X -= move;

            platforms = new List<Shared.Jumpeno.Entities.Platform>();
            foreach (var platform in Platforms) {
                platform.X -= move / 2f;
                platforms.Add(new Shared.Jumpeno.Entities.Platform { X = platform.X, Y = platform.Y, Width = (int)platform.Body.Size.X, Height = (int)platform.Body.Size.Y });
            }
            playerPositions = new List<PlayerPosition>();
            foreach (var player in _game.PlayersInGame) {
                player.X -= move / 2f;
                playerPositions.Add(new PlayerPosition { Id = player.Id, X = player.X, Y = player.Y });
            }
        }

        /**
         * Metóda aktualizuje všetkých hráčov a skontroluje kolízie.
         */
        public async Task Update(int fpsTickNum, IHubContext<GameHub> hub)
        {
            //DEBUG updates
            //System.Console.WriteLine("_FPS:" + fpsTickNum + "  Updated " + Count++ + " times.");

            var positionsToCompare = new List<PlayerPosition>();
            foreach (var p in _game.PlayersInGame) {
                positionsToCompare.Add(new PlayerPosition { Id = p.Id, X = p.X, Y = p.Y });
                //var pos = p.Body.Position;
                await p.Update(fpsTickNum);
                //if (pos != p.Body.Position) {
                //    await hub.Clients.Group(_game.Settings.GameCode).SendAsync(GameHubC.PlayerMoved, new PlayerPosition { Id = p.Id, X = p.X, Y = p.Y, FacingRight = p.FacingRight, State = p.State });
                //}
            }

            Vector2 colissionDirection = new Vector2(0, 0);
            // player and walls collision
            foreach (var player in _game.PlayersInGame) {
                if (!player.Alive) {
                    continue;
                }
                // top border
                if (player.Y < 0) {
                    player.Y = 0;
                    player.OnCollision(new Vector2(0, -1));
                }
                // bottom border
                if (player.Y > Y - player.Body.Size.Y) {
                    player.Y = Y - player.Body.Size.Y;
                    player.OnCollision(new Vector2(0, 1));
                }
                // left border
                if (player.X <= 0) {
                    player.X = 0;
                    player.LeftColission = true;
                }
                //right border
                if (player.X >= X - player.Body.Size.X) {
                    player.X = X - player.Body.Size.X;
                    player.RightColission = true;
                }
            }

            // player and platform collision
            foreach (var player in _game.PlayersInGame) {
                if (!player.Alive) {
                    continue;
                }
                foreach (var platform in Platforms) {
                    if (!platform.Visible) {
                        continue;
                    }
                    if (player.GetCollider().CheckCollision(platform.GetCollider(), ref colissionDirection, 0)) {
                        player.OnCollision(colissionDirection);
                    }
                }
            }

            // players collision
            foreach (var pl1 in _game.PlayersInGame) {
                if (!pl1.Alive) {
                    continue;
                }
                foreach (var pl2 in _game.PlayersInGame) {
                    if (!pl2.Alive || pl1 == pl2) {
                        continue;
                    }
                    if (pl1.GetCollider().CheckCollision(pl2.GetCollider(), ref colissionDirection, 0)) {
                        if (colissionDirection.Y > 55 && colissionDirection.Y < 70 && pl1.Falling) { // skocil mu na hlavu
                            pl1.Kills += 1;
                            pl1.OnCollision(colissionDirection);
                            pl2.Die();
                            await hub.Clients.Group(_game.Settings.GameCode).SendAsync(GameHubC.PlayerDied, pl2.Id, pl1.Id);
                            --_game.PlayersAllive;
                            if (_game.PlayersAllive == 1) {
                                return;
                            }
                        }
                    }
                }

            }

            // check for player crush while map shrinking
            foreach (var player in _game.PlayersInGame) {
                if (player.LeftColission && player.RightColission) {
                    if (_game.PlayersAllive == 1) {
                        return;
                    }

                    player.Die();
                    await hub.Clients.Group(_game.Settings.GameCode).SendAsync(GameHubC.PlayerCrushed, player.Id);
                    --_game.PlayersAllive;
                }

                player.LeftColission = false;
                player.RightColission = false;
            }

            foreach (var pl in _game.PlayersInGame) {
                foreach (var ptc in positionsToCompare) {
                    if (pl.Id == ptc.Id) {
                        if (pl.X != ptc.X || pl.Y != ptc.Y) {
                            await hub.Clients.Group(_game.Settings.GameCode).SendAsync(GameHubC.PlayerMoved, new PlayerPosition { Id = pl.Id, X = pl.X, Y = pl.Y, FacingRight = pl.FacingRight, State = pl.State });
                        }
                    }
                }
            }
        }

        /**
         * Nastaví hráčovi takú pozíciu, aby nekolidoval so žiadnou platformou a stenou
         */
        private void PositionPlayer(Player player)
        {
            var colissionDirection = new Vector2(0, 0);
            var rnd = new Random();
            var hit = false;

            do {
                player.X = rnd.Next(0, (int)X - (int)player.Body.Size.X);
                player.Y = rnd.Next(0, (int)Y - (int)player.Body.Size.Y);

                foreach (var platform in Platforms) {
                    if (player.GetCollider().CheckCollision(platform.GetCollider(), ref colissionDirection, 0, false)) {
                        hit = true;
                        break;
                    }
                }
            } while (hit);
        }
    }
}
