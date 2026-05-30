using System;
using System.Threading.Tasks;
using Mirror;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

/// <summary>
/// Steam P2P matchmaking via Facepunch.Steamworks lobbies + FizzyFacepunch transport.
/// Host calls <see cref="HostGame"/> → creates a Steam lobby, shares lobby ID as code.
/// Guest calls <see cref="JoinGame"/> with the lobby ID → joins lobby, connects via FizzyFacepunch.
/// </summary>
public class SteamMatchmakingService : MonoBehaviour, IMatchmakingService
{
    [SerializeField] private GameNetworkManager _networkManager;

    public event Action<string> OnRoomCodeReady;
    public event Action OnConnected;
    public event Action<string> OnConnectionFailed;

    private Lobby? _currentLobby;

    private void OnEnable()
    {
        SteamMatchmaking.OnLobbyCreated += HandleLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += HandleLobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested += HandleJoinRequested;
    }

    private void OnDisable()
    {
        SteamMatchmaking.OnLobbyCreated -= HandleLobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= HandleLobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested -= HandleJoinRequested;
    }

    public async void HostGame()
    {
        if (!SteamClient.IsValid)
        {
            OnConnectionFailed?.Invoke("Steam is not running.");
            return;
        }

        Lobby? lobby = await SteamMatchmaking.CreateLobbyAsync(2);
        if (lobby == null)
        {
            OnConnectionFailed?.Invoke("Failed to create Steam lobby.");
            return;
        }

        lobby.Value.SetPublic();
        lobby.Value.SetJoinable(true);
        lobby.Value.SetData("game", "tictactoe");
        _currentLobby = lobby;

        _networkManager.StartHost();
        string code = lobby.Value.Id.Value.ToString();
        OnRoomCodeReady?.Invoke(code);
    }

    public async Task JoinGame(string code)
    {
        if (!SteamClient.IsValid)
        {
            OnConnectionFailed?.Invoke("Steam is not running.");
            return;
        }

        if (!ulong.TryParse(code, out ulong lobbyIdRaw))
        {
            OnConnectionFailed?.Invoke($"Invalid lobby code: \"{code}\".");
            return;
        }

        Lobby? result = await SteamMatchmaking.JoinLobbyAsync(new SteamId { Value = lobbyIdRaw });
        if (result == null)
        {
            OnConnectionFailed?.Invoke("Could not join Steam lobby. The lobby may be full or no longer exists.");
        }
    }

    public void LeaveGame()
    {
        _currentLobby?.Leave();
        _currentLobby = null;

        if (NetworkServer.active && NetworkClient.isConnected)
            _networkManager.StopHost();
        else if (NetworkClient.isConnected)
            _networkManager.StopClient();
    }

    private void HandleLobbyCreated(Result result, Lobby lobby)
    {
        if (result != Result.OK)
            OnConnectionFailed?.Invoke($"Steam lobby creation failed: {result}.");
    }

    private void HandleLobbyEntered(Lobby lobby)
    {
        _currentLobby = lobby;
        // Guest: host's SteamId is stored so FizzyFacepunch can connect.
        if (!NetworkServer.active)
        {
            _networkManager.networkAddress = lobby.Owner.Id.Value.ToString();
            _networkManager.StartClient();
        }
    }

    private async void HandleJoinRequested(Lobby lobby, SteamId _)
    {
        await JoinGame(lobby.Id.Value.ToString());
    }
}
