using System;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;

/// <summary>
/// Listen-server matchmaking over KCP.
/// Host calls <see cref="HostGame"/> → StartHost, receives IP:Port room code.
/// Guest calls <see cref="JoinGame"/> with that code → StartClient.
/// </summary>
public class KcpMatchmakingService : MonoBehaviour, IMatchmakingService
{
    [SerializeField] private GameNetworkManager _networkManager;

    public event Action<string> OnRoomCodeReady;
    public event Action OnConnected;
    public event Action<string> OnConnectionFailed;

    public void HostGame()
    {
        _networkManager.StartHost();
        string localIp = GetLocalIPAddress();
        string code = $"{localIp}:{_networkManager.networkPort}";
        OnRoomCodeReady?.Invoke(code);
    }

    public Task JoinGame(string code)
    {
        string[] parts = code.Split(':');
        if (parts.Length != 2 || !ushort.TryParse(parts[1], out ushort port))
        {
            OnConnectionFailed?.Invoke($"Invalid room code: \"{code}\". Expected format IP:Port.");
            return Task.CompletedTask;
        }

        _networkManager.networkAddress = parts[0];
        _networkManager.networkPort = port;
        _networkManager.StartClient();
        return Task.CompletedTask;
    }

    public void LeaveGame()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
            _networkManager.StopHost();
        else if (NetworkClient.isConnected)
            _networkManager.StopClient();
        else if (NetworkServer.active)
            _networkManager.StopServer();
    }

    private static string GetLocalIPAddress()
    {
        try
        {
            using System.Net.Sockets.UdpClient udp = new System.Net.Sockets.UdpClient();
            udp.Connect("8.8.8.8", 80);
            return ((System.Net.IPEndPoint)udp.Client.LocalEndPoint).Address.ToString();
        }
        catch
        {
            return "127.0.0.1";
        }
    }
}
