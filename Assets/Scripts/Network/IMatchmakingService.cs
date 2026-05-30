using System;
using System.Threading.Tasks;

/// <summary>
/// Abstracts KCP (listen-server) and Steam P2P matchmaking behind a single interface.
/// </summary>
public interface IMatchmakingService
{
    /// <summary>Fired once a room/lobby code is available to share with the other player.</summary>
    event Action<string> OnRoomCodeReady;

    /// <summary>Fired when both players are connected and the game is ready to start.</summary>
    event Action OnConnected;

    /// <summary>Fired on any connection failure with a human-readable reason.</summary>
    event Action<string> OnConnectionFailed;

    /// <summary>Start hosting a game. Raises <see cref="OnRoomCodeReady"/> with the shareable code.</summary>
    void HostGame();

    /// <summary>Join an existing game using the code produced by <see cref="HostGame"/>.</summary>
    Task JoinGame(string code);

    /// <summary>Cleanly disconnect and stop hosting/connecting.</summary>
    void LeaveGame();
}
