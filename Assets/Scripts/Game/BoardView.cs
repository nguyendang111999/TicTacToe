using System;
using UnityEngine;

/// <summary>
/// Spawns and manages the grid of <see cref="CellView"/> instances.
/// Attach to the Board prefab. Assign <see cref="_cellPrefab"/> and <see cref="_config"/> in the Inspector.
/// </summary>
public class BoardView : MonoBehaviour
{
    [SerializeField] private CellView _cellPrefab;
    [SerializeField] private GameConfig _config;

    private CellView[] _cells;

    /// <summary>Raised when a player clicks an empty cell. Argument is the cell index (0-based, row-major).</summary>
    public event Action<int> OnCellClicked;

    private void Awake()
    {
        BuildGrid();
    }

    private void BuildGrid()
    {
        int total = _config.CellCount;
        _cells = new CellView[total];

        for (int i = 0; i < total; i++)
        {
            CellView cell = Instantiate(_cellPrefab, transform);
            cell.Initialize(i, HandleCellClicked);
            _cells[i] = cell;
        }
    }

    /// <summary>
    /// Refreshes the entire board to match the provided cell array.
    /// Call this from the <see cref="TicTacToeBoard"/> SyncVar hook.
    /// </summary>
    public void Refresh(byte[] cellData)
    {
        for (int i = 0; i < _cells.Length; i++)
            _cells[i].SetState((CellState)cellData[i]);
    }

    /// <summary>
    /// Highlights the winning line of cells and disables interaction on all others.
    /// </summary>
    public void ShowWinningLine(int startIndex, int dc, int dr)
    {
        // Disable all cells first.
        foreach (CellView cell in _cells)
            cell.SetHighlight(false);

        if (startIndex < 0) return;

        int boardSize = _config.BoardSize;
        int winLength = _config.WinLength;
        int row = startIndex / boardSize;
        int col = startIndex % boardSize;

        for (int step = 0; step < winLength; step++)
        {
            int r = row + dr * step;
            int c = col + dc * step;
            if (r < 0 || r >= boardSize || c < 0 || c >= boardSize) break;
            _cells[r * boardSize + c].SetHighlight(true);
        }
    }

    /// <summary>Locks all cells so no further input is accepted (game over / opponent's turn guard).</summary>
    public void SetInteractable(bool interactable)
    {
        foreach (CellView cell in _cells)
        {
            // Only re-enable empty cells; occupied cells stay non-interactable.
            if (interactable)
                cell.SetState(cell.GetComponent<UnityEngine.UI.Button>().interactable
                    ? CellState.Empty
                    : CellState.Empty); // refresh will re-evaluate properly
            else
                cell.GetComponent<UnityEngine.UI.Button>().interactable = false;
        }
    }

    private void HandleCellClicked(int index)
    {
        OnCellClicked?.Invoke(index);
    }
}
