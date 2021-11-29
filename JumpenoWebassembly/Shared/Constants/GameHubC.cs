namespace JumpenoWebassembly.Shared.Constants
{
    public static class GameHubC
    {
        public const string Url = "/gamehub";

        #region Lobby

        public const string ConnectToLobby = "ConnectToLobby";
        public const string ConnectedToLobby = "ConnectedToLobby";
        public const string PlayerJoined = "PlayerJoined";
        public const string PlayerLeft = "PlayerLeft";
        public const string LobbyInfoChanged = "LobbyInfoChanged";
        public const string SettingsChanged = "SettingsChanged";
        public const string ChangeLobbyInfo = "ChangeLobbyInfo";
        public const string LeaveLobby = "LeaveLobby";
        public const string LobbyFull = "LobbyFull";
        public const string SendMessage = "SendMessage";
        public const string ReceiveMessage = "ReceiveMessage";

        #endregion

        #region Game
        
        public const string StartGame = "StartGame";
        public const string DeleteGame = "DeleteGame";
        public const string GameDeleted = "GameDeleted";

        public const string PrepareGame = "PrepareGame";

        public const string MapShrinked = "MapShrinked";
        public const string GameplayInfoChanged = "GameplayInfoChanged";
        public const string ChangeGameplayInfo = "ChangeGameplayInfo";

        public const string PlayerKicked = "PlayerKicked";
        public const string PlayerMoved = "PlayerMoved";
        public const string PlayerDied = "PlayerDied";
        public const string PlayerCrushed = "PlayerCrushed";
        public const string PlayerVisibilityChanged = "PlayerVisibilityChanged";

        public const string ChangePlayerMovement = "ChangePlayerMovement";
        public const string PlayerMovementChanged = "PlayerMovementChanged";

        #endregion

    }
}
