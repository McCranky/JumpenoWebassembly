using JumpenoWebassembly.Server.Components.Jumpeno.Entities;
using JumpenoWebassembly.Server.Hubs;
using JumpenoWebassembly.Shared.Constants;
using JumpenoWebassembly.Shared.Jumpeno.Utilities;
using JumpenoWebassembly.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace JumpenoWebassembly.Server.Components.Jumpeno.Game
{
    /**
     * Reprezentuje hracie pole, sa vyskytujú platformi a hráči.
     */
    public class Map
    {
        private readonly int _tileSize;
        public List<Platform> Platforms { get; set; }
        public string BackgroundColor { get; set; }
        public float X { get; private set; }
        public float Y { get; private set; }

        private readonly GameEngine _game;

        public Map(GameEngine game, MapTemplate template)
        {
            _game = game;
            _tileSize = MapC.TileSize;
            Platforms = new List<Platform>();
            // pokiaľ z nejakého dôvodu nemáme pripravené žiadne mapy, tak sa aplikuje prednastavená varianta, aby bolo na čom hrať
            if (template != null) {
                X = template.Width * _tileSize;
                Y = template.Height * _tileSize;
                BackgroundColor = template.BackgroundColor;

                GeneratePlatforms(template.Tiles);
            } else {
                X = 16 * _tileSize;
                Y = 9 * _tileSize;
                BackgroundColor = "rgb(36, 30, 59)";

                GeneratePlatforms(null);
            }
        }

        private void GeneratePlatforms(string template)
        {
            if (template != null) {
                int width = (int)X / _tileSize;
                int height = (int)Y / _tileSize;
                for (int i = 0; i < width; i++) {
                    for (int j = 0; j < height; j++) {
                        if (template[Conversions.Map2DToIndex(i, j, width)] == '1') {
                            Platforms.Add(new Platform("tile.png", new Vector2(i * _tileSize, j * _tileSize)));
                        }
                    }
                }
            } else { // ak nemáme template, tak vygenerujeme aspoň pár platforiem aby boli na mape nejake prekažky
                Platforms.Add(new Platform("tile.png", new Vector2(0 * _tileSize, 8 * _tileSize)));
                Platforms.Add(new Platform("tile.png", new Vector2(1 * _tileSize, 8 * _tileSize)));
                Platforms.Add(new Platform("tile.png", new Vector2(2 * _tileSize, 8 * _tileSize)));
                Platforms.Add(new Platform("tile.png", new Vector2(3 * _tileSize, 8 * _tileSize)));
                Platforms.Add(new Platform("tile.png", new Vector2(13 * _tileSize, 6 * _tileSize)));
                Platforms.Add(new Platform("tile.png", new Vector2(12 * _tileSize, 6 * _tileSize)));
                Platforms.Add(new Platform("tile.png", new Vector2(11 * _tileSize, 6 * _tileSize)));
                Platforms.Add(new Platform("tile.png", new Vector2(10 * _tileSize, 6 * _tileSize)));
                Platforms.Add(new Platform("tile.png", new Vector2(9 * _tileSize, 6 * _tileSize)));
                Platforms.Add(new Platform("tile.png", new Vector2(8 * _tileSize, 6 * _tileSize)));
                Platforms.Add(new Platform("tile.png", new Vector2(7 * _tileSize, 6 * _tileSize)));
                Platforms.Add(new Platform("tile.png", new Vector2(6 * _tileSize, 6 * _tileSize)));
                Platforms.Add(new Platform("tile.png", new Vector2(5 * _tileSize, 6 * _tileSize)));
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
            foreach (var p in _game.PlayersInGame) {
                var pos = p.Body.Position;
                await p.Update(fpsTickNum);
                if (pos != p.Body.Position) {
                    await hub.Clients.Group(_game.Settings.GameCode).SendAsync(GameHubC.PlayerMoved, new PlayerPosition { Id = p.Id, X = p.X, Y = p.Y, FacingRight = p.FacingRight, State = p.State });
                }
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
