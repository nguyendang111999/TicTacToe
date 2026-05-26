# Plan: Online Tic-Tac-Toe with Mirror Networking

## Context
- Unity 2022.3.62f2, fresh project, 2D URP template
- No scripts, no networking packages yet
- UI: UI Toolkit
- Networking: Mirror + KCP (dedicated server / listen-server) + FizzyFacepunch (Steam P2P)
- Matchmaking: Simple room code (KCP) + Steam lobby (Facepunch)
- Extra features: Rematch, player name display, score tracker, SFX + basic animations

---

## TL;DR
Build in 7 phases — foundation (packages + architecture), offline game logic, networked game logic, matchmaking services, UI Toolkit interface, extra features, and polish/testing.

---

## Phase 1 — Foundation & Package Setup

1. Install Mirror Networking via OpenUPM (`com.mirror-networking.mirror`)
2. KCP Transport is bundled with Mirror — confirm it appears under Mirror/Transports/kcp2k
3. Install Facepunch.Steamworks.Net via Git URL into Packages/ manifest (not Asset Store)
4. Install FizzyFacepunch transport via Git URL (requires Facepunch.Steamworks)
5. Create Assets folder structure:
   - Scripts/Network/, Scripts/Game/, Scripts/UI/, Scripts/Config/
   - UI/UXML/, UI/USS/
   - Prefabs/Network/, Prefabs/Game/
   - Scenes/, ScriptableObjects/
6. Duplicate SampleScene → create MainMenu.unity and Game.unity under Assets/Scenes/
7. Add both scenes to EditorBuildSettings (File > Build Settings)
8. Create `GameNetworkManager` (extends Mirror `NetworkManager`) — configure default KCP transport, set offline/online scene references
9. Create `NetworkManager` prefab in Prefabs/Network/, assign GameNetworkManager + KcpTransport + FizzyFacepunchTransport components
10. Create `NetworkBootstrapper` MonoBehaviour — exposes `SetMode(MatchmakingMode)` to swap active transport at runtime (KCP vs Facepunch)

---

## Phase 2 — Core Game Logic (Offline First)

11. Create `GameConfig` ScriptableObject — holds board size (3), win length (3), player symbol strings ("X","O"), SFX clips
12. Create `CellState` enum with byte backing: `enum CellState : byte { Empty, X, O }` — 1-byte wire size, type-safe, Mirror serializes natively without a custom reader/writer
13. Create `BoardLogic` static class — pure logic: `CheckWinner(CellState[])`, `IsBoardFull(CellState[])`, `GetWinningLine()`
14. Create `CellView` MonoBehaviour — single cell visual (Image/Label), exposes `SetState(CellState)` method
15. Create `BoardView` MonoBehaviour — instantiates 9 CellViews in a grid, routes click events upward via `Action<int> OnCellClicked`
16. Test offline: wire up a simple test scene to verify win detection and board rendering without any networking

---

## Phase 3 — Networked Game Logic

17. Convert `TicTacToeBoard` to `NetworkBehaviour`:
    - `[SyncVar] private byte[] _cells` (9 cells, hook → refresh BoardView)
    - `[SyncVar] private int _currentPlayerIndex` (0 or 1, hook → update turn UI)
    - `[Command] CmdMakeMove(int cellIndex)` — server validates and updates _cells
    - `[ClientRpc] RpcOnGameOver(GameResult result)` — broadcast winner/draw
18. Create `PlayerController` NetworkBehaviour:
    - `[SyncVar] public string PlayerName`
    - `[SyncVar] public int PlayerIndex` (0 = X, 1 = O) — assigned by server on connect
    - Handles local input → calls `CmdMakeMove` only on `isLocalPlayer`
19. Create `GameStateController` NetworkBehaviour (server-side authority):
    - Tracks both players' connections via `NetworkServer.connections`
    - On each move: runs `BoardLogic.CheckWinner()`, advances turn, calls `RpcOnGameOver` when done
    - `[SyncVar] int ScoreX`, `[SyncVar] int ScoreO` — persisted across rematches in the same session
20. In `GameNetworkManager`: override `OnServerAddPlayer` to assign PlayerIndex, spawn Player prefab
21. Test: Play as host + client in the Unity editor (use "Host" + second editor instance or ParrelSync)

---

## Phase 4 — Matchmaking Services

22. Define `IMatchmakingService` interface: `HostGame()`, `Task<bool> JoinGame(string code)`, `LeaveGame()`, `event Action<string> OnRoomCodeReady`
23. Create `KcpMatchmakingService` (implements IMatchmakingService):
    - `HostGame()` → calls `NetworkManager.singleton.StartHost()`, raises `OnRoomCodeReady` with local IP:port string
    - `JoinGame(code)` → parses IP:port, sets `networkAddress` + `networkPort`, calls `StartClient()`
    - Note: For real cloud play, deploy a server build to a VPS and have the host run StartServer() instead
24. Create `SteamMatchmakingService` (implements IMatchmakingService):
    - `HostGame()` → create Steam lobby via `SteamMatchmaking.CreateLobbyAsync()`, set lobby metadata, raise `OnRoomCodeReady` with lobby ID string
    - `JoinGame(code)` → parse SteamId, join Steam lobby, connect via FizzyFacepunch
    - Handle `Lobby.OnGameLobbyJoinRequested` for Steam invite flow
25. `NetworkBootstrapper` wires the correct service based on selected mode and calls `SetTransport()`

---

## Phase 5 — UI Toolkit Interface

26. Create `MainMenu.uxml` + `MainMenu.uss`:
    - Mode selector: "Dedicated Server" / "Steam P2P" toggle buttons
    - Player name input field
    - "Host Game" button → shows generated room code in a label
    - "Join Game" button + room code input field
27. Create `MainMenuUI.cs` MonoBehaviour — binds UXML elements, injects correct `IMatchmakingService`, navigates to Game scene on connect
28. Create `Game.uxml` + `Game.uss`:
    - 3×3 grid of buttons (the board)
    - Turn indicator label ("X's Turn" / "O's Turn")
    - Score display (X: 0 — O: 0)
    - Player name labels above each side
29. Create `GameUI.cs` MonoBehaviour — binds to `TicTacToeBoard` SyncVar hooks and `GameStateController` events
30. Create `GameOverPanel.uxml` (overlay):
    - Winner/draw announcement label
    - "Rematch" button (only visible to both players; requires both to agree)
    - "Main Menu" button
31. Create `GameOverUI.cs` — handles rematch request flow (both players must press Rematch)

---

## Phase 6 — Extra Features

32. **Rematch system**: `[Command] CmdRequestRematch()` on `GameStateController` — when both players have requested, server resets board SyncVars and broadcasts `RpcOnRematch()`
33. **Player name sync**: Player enters name in MainMenu; `PlayerController` syncs it via `[Command] CmdSetName(string name)` → `[SyncVar] PlayerName`
34. **Score tracker**: `ScoreX`/`ScoreO` SyncVars on `GameStateController` — increment server-side on win, reset only on "Main Menu" (not on rematch)
35. **SFX**: `AudioSource` on a persistent GameObject, triggered by `BoardView` cell click events and `RpcOnGameOver` event — clip references stored in `GameConfig` ScriptableObject
36. **Animations**: Unity Animator on `CellView` — simple scale-pop animation on symbol placement; `GameOverPanel` slide-in via USS transitions or a short Animator clip

---

## Phase 7 — Polish & Testing

37. Handle disconnect mid-game: override `OnServerDisconnect` / `OnClientDisconnect` in `GameNetworkManager` → show "Opponent disconnected" via `RpcOnGameOver` with a Disconnected result
38. Prevent input during opponent's turn (validate `isLocalPlayer` and `_currentPlayerIndex` on client before sending Command)
39. Test KCP path: Host in Editor, join from a build (or two editors via ParrelSync)
40. Test Steam path: Two Steam accounts, create lobby + join
41. Test edge cases: simultaneous clicks, disconnect/reconnect, rematch 3+ times
42. Set Steam App ID (steamappid.txt in project root or via SteamManager init)
43. Configure build settings: target platform, IL2CPP vs Mono, strip unused packages

---

## Files to Create

| File | Purpose |
|---|---|
| `Assets/Scripts/Network/GameNetworkManager.cs` | Extends NetworkManager, scene management, player spawning |
| `Assets/Scripts/Network/NetworkBootstrapper.cs` | Transport switching |
| `Assets/Scripts/Network/IMatchmakingService.cs` | Interface |
| `Assets/Scripts/Network/KcpMatchmakingService.cs` | KCP listen-server matchmaking |
| `Assets/Scripts/Network/SteamMatchmakingService.cs` | Steam lobby matchmaking |
| `Assets/Scripts/Game/TicTacToeBoard.cs` | NetworkBehaviour, SyncVars + Commands |
| `Assets/Scripts/Game/PlayerController.cs` | NetworkBehaviour, input + name sync |
| `Assets/Scripts/Game/GameStateController.cs` | NetworkBehaviour, win/score/rematch authority |
| `Assets/Scripts/Game/BoardLogic.cs` | Static pure win-detection logic |
| `Assets/Scripts/Game/CellView.cs` | Visual cell MonoBehaviour |
| `Assets/Scripts/Game/BoardView.cs` | Grid of CellViews |
| `Assets/Scripts/UI/MainMenuUI.cs` | Main menu binding |
| `Assets/Scripts/UI/GameUI.cs` | Game screen binding |
| `Assets/Scripts/UI/GameOverUI.cs` | Game over panel + rematch flow |
| `Assets/Scripts/Config/GameConfig.cs` | ScriptableObject config |
| `Assets/UI/UXML/MainMenu.uxml` | Main menu layout |
| `Assets/UI/UXML/Game.uxml` | Game screen layout |
| `Assets/UI/UXML/GameOverPanel.uxml` | Game over overlay |
| `Assets/UI/USS/MainMenu.uss` | Main menu styles |
| `Assets/UI/USS/Game.uss` | Game screen styles |
| `Assets/Prefabs/Network/NetworkManager.prefab` | NetworkManager prefab |
| `Assets/Prefabs/Game/Board.prefab` | Board prefab |
| `Assets/Prefabs/Game/Cell.prefab` | Cell prefab |
| `Assets/Prefabs/Network/Player.prefab` | Player prefab |
| `Assets/ScriptableObjects/GameConfig.asset` | Config instance |

---

## Verification Checklist

- [ ] **Phase 2:** Two players can complete a game offline (all win/draw conditions correct)
- [ ] **Phase 3:** Host + client in ParrelSync can play a full game; board state is server-authoritative
- [ ] **Phase 4 KCP:** Two machines on same network can host/join by IP:Port
- [ ] **Phase 4 Steam:** Two Steam accounts can connect via Steam lobby ID
- [ ] **Phase 5:** UI responds correctly to all SyncVar hooks; no stale state after scene reload
- [ ] **Phase 6:** Rematch resets board but keeps score; disconnect triggers correct game-over message
- [ ] **Final:** Full play-through on both KCP and Steam paths from cold launch

---

## Decisions & Scope

- **Listen-server model** for KCP path (host = server + client). For true dedicated cloud server, deploy a headless server build to a VPS separately.
- **Steam P2P** requires a valid Steam App ID. Use test App ID 480 (SpaceWar) during development.
- **No authentication system** — player name is self-declared, no account system.
- **No spectator mode** — exactly 2 players per match.
- **UI Toolkit** for all UI (no uGUI Canvas).
- **Legacy Input Manager** (project guideline).
- **No singleton pattern** — services injected via Inspector or passed explicitly.
