using Mirror;
using UnityEngine;

/// <summary>
/// Central Mirror NetworkManager for the game.
/// Handles player spawning, assigns PlayerIndex (0 = X, 1 = O), and routes scenes.
/// Attach to the NetworkManager prefab alongside KcpTransport and FizzyFacepunchTransport.
/// </summary>
public class GameNetworkManager : NetworkManager
{
    // Expose networkPort for matchmaking services to read/write.
    public ushort networkPort
    {
        get => GetComponent<kcp2k.KcpTransport>().port;
        set => GetComponent<kcp2k.KcpTransport>().port = value;
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        // Assign PlayerIndex based on connection order (0 = first = X, 1 = second = O).
        int playerIndex = numPlayers - 1; // numPlayers already incremented by base call
        if (conn.identity.TryGetComponent(out PlayerController player))
            player.ServerSetPlayerIndex(playerIndex);
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        // Notify remaining player that opponent left (handled in Phase 3).
        base.OnServerDisconnect(conn);
    }
}
