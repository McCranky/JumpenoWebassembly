using JumpenoWebassembly.Server.Components.Jumpeno.Entities;
using JumpenoWebassembly.Server.Hubs;
using JumpenoWebassembly.Shared.Constants;
using JumpenoWebassembly.Shared.Jumpeno;
using JumpenoWebassembly.Shared.Jumpeno.Game;
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
        public MapTemplateCollection MapTemplates { get; protected set; }
        public GameState GameState { get; set; }
        public Map Map { get; private set; }

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
        public int FramesToShrink { get; set; }
        public int FramesToScoreboard { get; set; }
        public int FramesToLobby { get; set; }
        public bool CountdownTimerRunning { get; set; }
        public bool ShrinkingAllowed { get; set; } = true;
        public bool GameoverTimerRunning { get; set; }
        public bool ScoreboardTimerRunning { get; set; }

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

        public GameEngine(GameSettings settings, MapTemplateCollection templates, IHubContext<GameHub> hub)
        {
            LobbyInfo = new LobbyInfo();
            _hub = hub;
            Settings = settings;
            Name = settings.GameName;
            MapTemplates = templates;
            PlayersInGame = new List<Player>(settings.PlayersLimit);
            PlayersInLobby = new List<Player>(settings.PlayersLimit);
            GameState = GameState.Lobby;
            RestartTimers();
            StartGameEngine();
        }

        public async Task<bool> AddPlayer(Player player)
        {
            if (Settings.GameMode == GameMode.Guided) {
                if (GameState == GameState.Countdown) {
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
                    } else if (GameState == GameState.Lobby) {
                        LobbyInfo.FramesToStart /= 2;
                        await _hub.Clients.Group(Settings.GameCode).SendAsync(GameHubC.LobbyInfoChanged, LobbyInfo);
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
            if (GameState != GameState.Lobby) {
                lock (PlayersInGame) PlayersInGame.Remove(player);
                --PlayersAllive;
            }
            PlayersInLobby.Remove(player);
            if (PlayersInLobby.Count >= 1) {
                Creator = PlayersInLobby[rnd.Next(0, PlayersInLobby.Count - 1)];
                Settings.CreatorId = Creator.Id;
                await _hub.Clients.Group(Settings.GameCode).SendAsync(GameHubC.SettingsChanged, Settings);
                if (LobbyInfo.StartTimerRunning) {
                    LobbyInfo.FramesToStart *= 2;
                    await _hub.Clients.Group(Settings.GameCode).SendAsync(GameHubC.LobbyInfoChanged, LobbyInfo);
                }
            }
            player.InGame = false;
        }

        public void RespawnPlayer(Player player)
        {
            player.X = rnd.Next(0, (int)Map.X - (int)player.Body.Size.X);
            player.Y = rnd.Next(0, (int)Map.Y - (int)player.Body.Size.Y);
            player.Visible = true;
            player.Alive = true;
            ++PlayersAllive;
        }

        public async Task Start()
        {
            //RestartTimers();

            PlayersInGame = new List<Player>(PlayersInLobby);
            foreach (var pl in PlayersInGame) {
                pl.InGame = true;
            }
            if (Settings.GameMode == GameMode.Guided) {
                PlayersInGame.Remove(Creator);
            }
            PlayersAllive = PlayersInGame.Count;
            Map = new Map(this, MapTemplates.GetRandomMap());
            Map.SpawnPlayers();

            LobbyInfo.StartTimerRunning = false;
            await _hub.Clients.Group(Settings.GameCode).SendAsync(GameHubC.LobbyInfoChanged, LobbyInfo);

            GameState = GameState.Countdown; // musi byt posledné, inak by sa mohlo pristupovať ku null objektom
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
            FramesToLobby = 10 * _FPS;
            FramesToScoreboard = 5 * _FPS;
            FramesToShrink = 2 * 60 * _FPS;

            LobbyInfo.StoppedStartTimer = false;
            CountdownTimerRunning = true;
            ShrinkingAllowed = true;
            GameoverTimerRunning = true;
            ScoreboardTimerRunning = true;
        }

        /**
         * Metóda, ktorú volá "timer" každých (_MillisecondInSecond / _FPS) milisekund.
         * Zabezpečuje aktualizaciu hernej logiky.
         */
        private async Task TickAsync(object source, ElapsedEventArgs e)
        {
            OnTick(new GameTickEventArgs { FpsTickNum = currentFPS });
            if (GameState == GameState.Countdown) {
                await Map.Update(currentFPS);
                if (PlayersAllive <= 1) {
                    GameState = GameState.Gameover;
                    return;
                }
                if (FramesToShrink <= 0) {
                    GameState = GameState.Shrinking;
                } else {
                    if (CountdownTimerRunning) {
                        --FramesToShrink;
                    }
                }
            } else if (GameState == GameState.Shrinking) {
                await Map.Update(currentFPS);
                if (PlayersAllive == 1) {
                    GameState = GameState.Gameover;
                    return;
                }
                if (Map.X > 0) {
                    if (ShrinkingAllowed) {
                        Map.Shrink();
                    }
                } else {
                    GameState = GameState.Gameover;
                }
            } else if (GameState == GameState.Lobby) {
                if (Settings.GameMode == GameMode.Guided) {
                    // samospustenie
                    if (LobbyInfo.StartTimerRunning) {
                        --LobbyInfo.FramesToStart;
                        if (LobbyInfo.FramesToStart <= 0) {
                            LobbyInfo.StartTimerRunning = false;
                            await Start();
                        } else {
                            await _hub.Clients.Group(Settings.GameCode).SendAsync(GameHubC.LobbyInfoChanged, LobbyInfo);
                        }
                    }
                } else {
                    if (PlayersInLobby.Count > 1) {
                        if (!LobbyInfo.StartTimerRunning && !LobbyInfo.StoppedStartTimer) {
                            LobbyInfo.StartTimerRunning = true;
                            if (PlayersInLobby.Count > 2) {
                                for (int i = 0; i < PlayersInLobby.Count - 2; i++) {
                                    LobbyInfo.FramesToStart /= 2;
                                }
                            }
                            await _hub.Clients.Group(Settings.GameCode).SendAsync(GameHubC.LobbyInfoChanged, LobbyInfo);
                        } else {
                            // samospustenie
                            if (!LobbyInfo.StoppedStartTimer) {
                                --LobbyInfo.FramesToStart;
                                await _hub.Clients.Group(Settings.GameCode).SendAsync(GameHubC.LobbyInfoChanged, LobbyInfo);
                            }
                            if (LobbyInfo.FramesToStart <= 0) {
                                LobbyInfo.StartTimerRunning = false;
                                await Start();
                            }
                        }

                    } else if (PlayersInLobby.Count == 1) {
                        if (LobbyInfo.StartTimerRunning) {
                            LobbyInfo.StartTimerRunning = false;
                            await _hub.Clients.Group(Settings.GameCode).SendAsync(GameHubC.LobbyInfoChanged, LobbyInfo);
                        }
                        if (LobbyInfo.DeleteTimerRunning) {
                            LobbyInfo.DeleteTimerRunning = false;
                            await _hub.Clients.Group(Settings.GameCode).SendAsync(GameHubC.LobbyInfoChanged, LobbyInfo);
                        }
                    } else {
                        if (!LobbyInfo.DeleteTimerRunning) {
                            LobbyInfo.DeleteTimerRunning = true;
                            await _hub.Clients.Group(Settings.GameCode).SendAsync(GameHubC.LobbyInfoChanged, LobbyInfo);
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
            } else if (GameState == GameState.Gameover) {
                await Map.Update(currentFPS);
                if (GameoverTimerRunning) {
                    --FramesToScoreboard;
                }
                if (FramesToScoreboard <= 0) {
                    foreach (var player in PlayersInGame) {
                        if (player.Alive) {
                            Winner = player;
                            break;
                        }
                    }
                    GameState = GameState.Scoreboard;
                }
            } else if (GameState == GameState.Scoreboard) {
                if (ScoreboardTimerRunning) {
                    --FramesToLobby;
                }
                if (FramesToLobby <= 0) {
                    foreach (var pl in PlayersInGame) {
                        pl.InGame = false;
                        pl.Freeze();
                    }
                    Creator.InGame = false;
                    PlayersInGame.Clear();
                    RestartTimers();
                    GameState = GameState.Lobby;
                }
            }
            currentFPS = currentFPS % 60 + 1;
            ++FPSElapsed;
        }

        public Player GetPlayer(long id)
        {
            if (GameState == GameState.Lobby) {
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
            GameState = GameState.Deleted;
            OnTick(new GameTickEventArgs { FpsTickNum = -1 });
        }
    }
}
