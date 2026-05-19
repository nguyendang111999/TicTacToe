# Unity C# & Mirror Networking Guidelines

You are an expert Unity developer assisting with an Online Tic-Tac-Toe game. Always adhere to the following rules when generating or modifying code for this workspace:

## 1. Project Stack & Networking
- **Networking Library:** Mirror Networking.
- **Transports:** KCP (for standard/dedicated server connections) and FizzyFacepunch (for Steam P2P matchmaking).
- **Network Logic:** Cleanly separate client and server logic. Properly use Mirror's `[Command]` (Client to Server), `[ClientRpc]` (Server to Client), `[TargetRpc]`, and `[SyncVar]` with appropriate hook properties.
- **Transport Management:** Write logic that allows seamless fallback or switching between the KCP transport and Steam Facepunch transport depending on the matchmaking mode selected by the user.

## 2. Architecture: Modular Design System
- **Modularity Focus:** Build systems with high cohesion and low coupling. Compose behaviors using small, single-responsibility components. Do not write monolithic `MonoBehaviour` "Manager" classes.
- **Decoupling:** Use `interfaces` for behavior definitions and rely on dependency injection (or explicit inspector assignments) over `Object.FindObjectOfType` or singletons where possible.
- **Data & Configuration:** Leverage `ScriptableObject` for game configurations, state data, or event channels.
- **Events:** Prefer C# `Action` or `UnityEvent` for broadcasting state changes (e.g., "Player Turn Ended", "Game Won") to keep UI and core game logic completely decoupled.

## 3. Unity Conventions & Best Practices
- **Input:** Use the **Legacy Input Manager** (`Input.GetMouseButtonDown`, `Input.GetAxis`, etc.). Do not use the New Input System package.
- **Caching:** Never use `GetComponent()` or any `Find()` methods inside `Update()`. Always cache components in `Awake()` or `Start()`.
- **References:** Never use `public` fields just to expose them in the Inspector. Always use `[SerializeField] private` instead.

## 4. Naming Conventions
- `PascalCase` for classes, structs, enums, methods, and properties.
- `_camelCase` for private, protected, and serialized fields.
- `camelCase` for local variables and method parameters.
- Prefix interfaces with `I` (e.g., `INetworkManager`).