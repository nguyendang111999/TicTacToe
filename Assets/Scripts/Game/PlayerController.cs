using Mirror;
using UnityEngine;

/// <summary>
/// Represents a connected player. Syncs name and player index (0 = X, 1 = O).
/// Full implementation comes in Phase 3. This stub satisfies GameNetworkManager's compile dependency.
/// </summary>
public class PlayerController : NetworkBehaviour
{
    [SyncVar] public string PlayerName = "Player";
    [SyncVar] public int PlayerIndex = -1;

    /// <summary>Called server-side by GameNetworkManager.OnServerAddPlayer to assign X or O.</summary>
    [Server]
    public void ServerSetPlayerIndex(int index)
    {
        PlayerIndex = index;
    }
}
