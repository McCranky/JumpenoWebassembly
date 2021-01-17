﻿using JumpenoWebassembly.Server.Components.Jumpeno.Entities;
using JumpenoWebassembly.Server.Hubs;
using JumpenoWebassembly.Shared.Constants;
using JumpenoWebassembly.Shared.Jumpeno;
using JumpenoWebassembly.Shared.Jumpeno.Game;
using JumpenoWebassembly.Shared.Jumpeno.Utilities;
using JumpenoWebassembly.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using static JumpenoWebassembly.Shared.Jumpeno.Enums;

namespace JumpenoWebassembly.Server.Components.Jumpeno.Game
{
    public class GameTickEventArgs : EventArgs
    {
        public int FpsTickNum { set; get; }
    }

    /**
     * Obsahuje všetky časti hry. Mapu, hračov a zabezpečuje chod hry.
     */
    public class GameEngine : JumpenoComponent, IDisposable
    {
        private MapTemplate _mapTemplate;
        public Map Map { get; private set; }

        public GameplayInfo Gameplay { get; set; }
        //public GameState GameState { get; set; }
        //public int FramesToShrink { get; set; }
        //public int FramesToScoreboard { get; set; }
        //public int FramesToLobby { get; set; }
        //public bool CountdownTimerRunning { get; set; }
        //public bool ShrinkingAllowed { get; set; } = true;
        //public bool GameoverTimerRunning { get; set; }
        //public bool ScoreboardTimerRunning { get; set; }


        public GameSettings Settings { get; set; }
        //public GameMode GameMode { get; set; }
        //public int PlayersLimit { get; set; }
        //public string Code { get; set; }


        public List<Player> PlayersInGame { get; set; }
        public List<Player> PlayersInLobby { get; set; }
        public Player Creator { get; set; }
        public Player Winner { get; set; }
        public int PlayersAllive { get; set; }
        public int FPSElapsed { get; set; }

        public LobbyInfo LobbyInfo { get; set; }
        //public int FramesToStart { get; set; } // meni sa v zavyslosťi od počtu hračov
        //public bool StoppedStartTimer { get; set; }
        //public bool StartTimerRunning { get; set; }
        //public bool DeleteTimerRunning { get; set; }


        public const int _FPS = 60;
        public const double _MillisecondInSecond = 1000.0;
        private int currentFPS = 1;
        private int deleteFrames;
        public const int _MillisecondsDelay = 1000 / _FPS;
        private Timer timer;
        public override string CssStyle(bool smallScreen) => $@"
            top: 0px ;
            left: 0px;
            width: 100%;
            height: 100%; 
            ";

        public event EventHandler<GameTickEventArgs> OnTickReached;
        private void OnTick(GameTickEventArgs e)
        {
            var handler = OnTickReached;
            handler?.Invoke(this, e);
        }

        private readonly IHubContext<GameHub> _hub;

        public GameEngine(GameSettings settings, MapTemplate map, IHubContext<GameHub> hub)
        {
            Gameplay = new GameplayInfo();
            LobbyInfo = new LobbyInfo();
            Settings = settings;

            _hub = hub;
            Name = settings.GameName;
            _mapTemplate = map;
            PlayersInGame = new List<Player>(settings.PlayersLimit);
            PlayersInLobby = new List<Player>(settings.PlayersLimit);
            Gameplay.State = GameState.Lobby;
            RestartTimers();
            StartGameEngine();
        }

        private async Task NotifyGameplayInfoChanged() => await _hub.Clients.Group(Settings.GameCode).SendAsync(GameHubC.GameplayInfoChanged, Gameplay);
        private async Task NotifyLobbyInfoChanged() => await _hub.Clients.Group(Settings.GameCode).SendAsync(GameHubC.LobbyInfoChanged, LobbyInfo);
        private async Task NotifySettingsChanged() => await _hub.Clients.Group(Settings.GameCode).SendAsync(GameHubC.SettingsChanged, Settings);

        public async Task<bool> AddPlayer(Player player)
        {
            if (Settings.GameMode == GameMode.Guided) {
                if (Gameplay.State == GameState.Countdown) {
                    if (PlayersInGame.Count < Settings.PlayersLimit) { // pripoji hrača do bežiacej hry
                        PlayersInGame.Add(player);
                        PlayersInLobby.Add(player);
                        player.InGame = true;
                        Map.SpawnPlayer(player);
                        ++PlayersAllive;
                        return true;
                    }
                    return false;
                } else { // Lobby
                    if (PlayersInLobby.Count < Settings.PlayersLimit) {
                        PlayersInLobby.Add(player);
                        return true;
                    }
                    return false;
                }
            } else { // Player Mode - hraju iba hrači ktory boli pri štarte, ostatny čakaju na skončenie hry
                if (PlayersInLobby.Count < Settings.PlayersLimit) {
                    PlayersInLobby.Add(player);
                    if (PlayersInLobby.Count == 1) {
                        Creator = player;
                    } else if (Gameplay.State == GameState.Lobby) {
                        LobbyInfo.FramesToStart /= 2;
                        await NotifyLobbyInfoChanged();
                    }
                    return true;
                }
                return false;
            }
        }

        public async Task RemovePlayer(Player player)
        {
            if (player.Spectator) {
                return;
            }
            if (Gameplay.State != GameState.Lobby) {
                lock (PlayersInGame) PlayersInGame.Remove(player);
                --PlayersAllive;
            }
            PlayersInLobby.Remove(player);
            if (PlayersInLobby.Count == 1 && Settings.GameMode != GameMode.Guided) {
                Creator = PlayersInLobby[rnd.Next(0, PlayersInLobby.Count - 1)];
                Settings.CreatorId = Creator.Id;
                await NotifySettingsChanged();

                if (LobbyInfo.StartTimerRunning) {
                    LobbyInfo.FramesToStart *= 2;
                    await NotifyLobbyInfoChanged();
                }
            }
            player.InGame = false;
        }

        public async Task Start()
        {
            RestartTimers();

            PlayersInGame = new List<Player>(PlayersInLobby);
            foreach (var pl in PlayersInGame) {
                pl.InGame = true;
            }
            if (Settings.GameMode == GameMode.Guided) {
                PlayersInGame.Remove(Creator);
            }
            PlayersAllive = PlayersInGame.Count;
            Map = new Map(this, _mapTemplate);
            Map.SpawnPlayers();

            var playerPositions = new List<PlayerPosition>();
            foreach (var player in PlayersInGame) {
                playerPositions.Add(new PlayerPosition { Id = player.Id, X = player.X, Y = player.Y });
            }

            var platforms = new List<Shared.Jumpeno.Entities.Platform>();
            foreach (var platform in Map.Platforms) {
                platforms.Add(new Shared.Jumpeno.Entities.Platform { X = platform.X, Y = platform.Y, Width = (int)platform.Body.Size.X, Height = (int)platform.Body.Size.Y });
            }

            var mapInfo = new MapInfo { Background = Map.BackgroundColor, X = Map.X, Y = Map.Y };
            await _hub.Clients.Group(Settings.GameCode).SendAsync(GameHubC.PrepareGame, mapInfo, platforms, playerPositions);

            LobbyInfo.StartTimerRunning = false;
            await NotifyLobbyInfoChanged();

            Gameplay.State = GameState.Countdown; // musi byt posledné, inak by sa mohlo pristupovať ku null objektom
            await NotifyGameplayInfoChanged();
        }

        public void StartGameEngine()
        {
            timer = new Timer(_MillisecondInSecond / _FPS);
            timer.Elapsed += async (sender, e) => await TickAsync(sender, e);
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        public void StopGameEngine()
        {
            timer.Elapsed -= async (sender, e) => await TickAsync(sender, e);
            timer.Enabled = false;
            OnTick(new GameTickEventArgs { FpsTickNum = currentFPS });
        }

        private void RestartTimers()
        {
            //[MINUTES] * [SECOND] * FRAMES
            LobbyInfo.FramesToStart = 2 * 60 * _FPS;
            Gameplay.FramesToLobby = 10 * _FPS;
            Gameplay.FramesToScoreboard = 5 * _FPS;
            Gameplay.FramesToShrink = 2 * 10 * _FPS;

            LobbyInfo.StoppedStartTimer = false;
            Gameplay.CountdownTimerRunning = true;
            Gameplay.ShrinkingAllowed = true;
            Gameplay.GameoverTimerRunning = true;
            Gameplay.ScoreboardTimerRunning = true;
        }

        /**
         * Metóda, ktorú volá "timer" každých (_MillisecondInSecond / _FPS) milisekund.
         * Zabezpečuje aktualizaciu hernej logiky.
         */
        private async Task TickAsync(object source, ElapsedEventArgs e)
        {
            OnTick(new GameTickEventArgs { FpsTickNum = currentFPS });
            if (Gameplay.State == GameState.Countdown) {
                await Map.Update(currentFPS, _hub);
                if (PlayersAllive <= 1) {
                    Gameplay.State = GameState.Gameover;
                    await NotifyGameplayInfoChanged();
                    return;
                }
                if (Gameplay.FramesToShrink <= 0) {
                    Gameplay.State = GameState.Shrinking;
                } else {
                    if (Gameplay.CountdownTimerRunning) {
                        --Gameplay.FramesToShrink;
                    }
                }
                await NotifyGameplayInfoChanged();


            } else if (Gameplay.State == GameState.Shrinking) {
                await Map.Update(currentFPS, _hub); // TODO send new player positions
                if (PlayersAllive == 1) {
                    Gameplay.State = GameState.Gameover;
                    await NotifyGameplayInfoChanged();
                    return;
                }
                if (Map.X > 0) {
                    if (Gameplay.ShrinkingAllowed) {
                        Map.Shrink(out var platforms, out var players);
                        await _hub.Clients.Group(Settings.GameCode).SendAsync(GameHubC.MapShrinked, new MapInfo { X = Map.X }, platforms, players);
                        //await NotifyMapShrinked();
                    }
                } else {
                    Gameplay.State = GameState.Gameover;
                    await NotifyGameplayInfoChanged();
                }


            } else if (Gameplay.State == GameState.Lobby) {
                if (Settings.GameMode == GameMode.Guided) {
                    // samospustenie
                    if (LobbyInfo.StartTimerRunning) {
                        --LobbyInfo.FramesToStart;
                        if (LobbyInfo.FramesToStart <= 0) {
                            LobbyInfo.StartTimerRunning = false;
                            await Start();
                        } else {
                            await NotifyLobbyInfoChanged();
                        }
                    }
                } else { // GameMode.Player
                    if (PlayersInLobby.Count > 1) {
                        if (!LobbyInfo.StartTimerRunning && !LobbyInfo.StoppedStartTimer) {
                            LobbyInfo.StartTimerRunning = true;
                            if (PlayersInLobby.Count > 2) {
                                for (int i = 0; i < PlayersInLobby.Count - 2; i++) {
                                    LobbyInfo.FramesToStart /= 2;
                                }
                            }
                            await NotifyLobbyInfoChanged();
                        } else {
                            // samospustenie
                            if (!LobbyInfo.StoppedStartTimer) {
                                --LobbyInfo.FramesToStart;
                                await NotifyLobbyInfoChanged();
                            }
                            if (LobbyInfo.FramesToStart <= 0) {
                                LobbyInfo.StartTimerRunning = false;
                                await Start();
                            }
                        }

                    } else if (PlayersInLobby.Count == 1) {
                        if (LobbyInfo.StartTimerRunning) {
                            LobbyInfo.StartTimerRunning = false;
                            await NotifyLobbyInfoChanged();
                        }
                        if (LobbyInfo.DeleteTimerRunning) {
                            LobbyInfo.DeleteTimerRunning = false;
                            await NotifyLobbyInfoChanged();
                        }
                    } else {
                        if (!LobbyInfo.DeleteTimerRunning) {
                            LobbyInfo.DeleteTimerRunning = true;
                            await NotifyLobbyInfoChanged();
                            deleteFrames = 10 * _FPS;
                        } else {
                            --deleteFrames;
                            if (deleteFrames <= 0) {
                                Map = null;
                                Dispose();
                            }
                        }
                    }
                }
            } else if (Gameplay.State == GameState.Gameover) {
                await Map.Update(currentFPS, _hub);
                if (Gameplay.GameoverTimerRunning) {
                    --Gameplay.FramesToScoreboard;
                    await NotifyGameplayInfoChanged();
                }
                if (Gameplay.FramesToScoreboard <= 0) {
                    foreach (var player in PlayersInGame) {
                        if (player.Alive) {
                            Gameplay.WinnerId = player.Id;
                            //Winner = player;
                            break;
                        }
                    }
                    Gameplay.State = GameState.Scoreboard;
                    await NotifyGameplayInfoChanged();
                }
            } else if (Gameplay.State == GameState.Scoreboard) {
                if (Gameplay.ScoreboardTimerRunning) {
                    --Gameplay.FramesToLobby;
                    await NotifyGameplayInfoChanged();
                }
                if (Gameplay.FramesToLobby <= 0) {
                    foreach (var pl in PlayersInGame) {
                        pl.InGame = false;
                        pl.Freeze();
                    }
                    //Creator.InGame = false;
                    PlayersInGame.Clear();
                    RestartTimers();
                    Gameplay.State = GameState.Lobby;
                    await NotifyGameplayInfoChanged();
                    //TODO need more actions
                }
            }
            currentFPS = currentFPS % 60 + 1;
            ++FPSElapsed;
        }

        public Player GetPlayer(long id)
        {
            if (Gameplay.State == GameState.Lobby) {
                foreach (var player in PlayersInLobby) {
                    if (player.Id == id) {
                        return player;
                    }
                }
            } else {
                foreach (var player in PlayersInGame) {
                    if (player.Id == id) {
                        return player;
                    }
                }
            }

            return null;
        }

        public void Dispose()
        {
            timer.Elapsed -= async (sender, e) => await TickAsync(sender, e);
            timer.Enabled = false;
            Gameplay.State = GameState.Deleted;
            OnTick(new GameTickEventArgs { FpsTickNum = -1 });
        }
    }
}
