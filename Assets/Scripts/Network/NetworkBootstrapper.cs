using Mirror;
using UnityEngine;

/// <summary>
/// Sits on the NetworkManager GameObject. Swaps the active Mirror transport and
/// wires the correct <see cref="IMatchmakingService"/> based on the selected mode.
/// Assign both transport references via the Inspector.
/// </summary>
public class NetworkBootstrapper : MonoBehaviour
{
    public enum MatchmakingMode { Kcp, Steam }

    [SerializeField] private Transport _kcpTransport;
    [SerializeField] private Transport _steamTransport;

    private IMatchmakingService _activeService;

    // The two concrete services are set by the UI before calling HostGame/JoinGame.
    // They are MonoBehaviours on this same GameObject so the Inspector can assign
    // the GameNetworkManager reference they need.
    [SerializeField] private KcpMatchmakingService _kcpService;
    [SerializeField] private SteamMatchmakingService _steamService;

    public IMatchmakingService ActiveService => _activeService;

    public void SetMode(MatchmakingMode mode)
    {
        switch (mode)
        {
            case MatchmakingMode.Kcp:
                Transport.active = _kcpTransport;
                _activeService = _kcpService;
                break;
            case MatchmakingMode.Steam:
                Transport.active = _steamTransport;
                _activeService = _steamService;
                break;
        }
    }
}
