/// <summary>
/// Pure, stateless board logic. No Unity or Mirror dependencies.
/// All methods accept boardSize and winLength so they are driven by GameConfig.
/// Win rule: casual Gomoku — a run of >= winLength in any direction counts (overlines win).
/// </summary>
public static class BoardLogic
{
    // Direction vectors: right, down, diagonal-right-down, diagonal-right-up
    private static readonly (int dc, int dr)[] Directions =
    {
        ( 1,  0),
        ( 0,  1),
        ( 1,  1),
        ( 1, -1),
    };

    /// <summary>
    /// Returns the <see cref="CellState"/> of the winner, or <see cref="CellState.Empty"/> if
    /// there is no winner yet. Call after every move, passing the index of the cell just placed.
    /// Scans outward from <paramref name="lastMoveIndex"/> in all 4 directions — O(winLength) per call.
    /// </summary>
    public static CellState CheckWinner(CellState[] cells, int boardSize, int winLength, int lastMoveIndex)
    {
        CellState placed = cells[lastMoveIndex];
        if (placed == CellState.Empty) return CellState.Empty;

        int row = lastMoveIndex / boardSize;
        int col = lastMoveIndex % boardSize;

        foreach ((int dc, int dr) in Directions)
        {
            int count = 1
                + CountInDirection(cells, boardSize, row, col, dc, dr, placed)
                + CountInDirection(cells, boardSize, row, col, -dc, -dr, placed);

            if (count >= winLength)
                return placed;
        }

        return CellState.Empty;
    }

    /// <summary>
    /// Returns true when every cell is occupied (draw condition).
    /// Only call this after <see cref="CheckWinner"/> returns Empty.
    /// </summary>
    public static bool IsBoardFull(CellState[] cells)
    {
        foreach (CellState cell in cells)
            if (cell == CellState.Empty) return false;
        return true;
    }

    /// <summary>
    /// Returns the starting index and direction vector of the winning run, or (-1, 0, 0) if none.
    /// Intended for win-line highlight animation; call only after <see cref="CheckWinner"/> confirms a winner.
    /// </summary>
    public static (int startIndex, int dc, int dr) GetWinningLine(
        CellState[] cells, int boardSize, int winLength, int lastMoveIndex)
    {
        CellState placed = cells[lastMoveIndex];
        if (placed == CellState.Empty) return (-1, 0, 0);

        int row = lastMoveIndex / boardSize;
        int col = lastMoveIndex % boardSize;

        foreach ((int dc, int dr) in Directions)
        {
            int back  = CountInDirection(cells, boardSize, row, col, -dc, -dr, placed);
            int fwd   = CountInDirection(cells, boardSize, row, col,  dc,  dr, placed);
            int total = 1 + back + fwd;

            if (total >= winLength)
            {
                int startRow = row + (-dc) * back;   // walk back to line start
                // Note: dc/dr are (col-delta, row-delta) so swap correctly:
                startRow = row + (-dr) * back;
                int startCol = col + (-dc) * back;
                int startIndex = startRow * boardSize + startCol;
                return (startIndex, dc, dr);
            }
        }

        return (-1, 0, 0);
    }

    // --- helpers ---

    private static int CountInDirection(
        CellState[] cells, int boardSize,
        int row, int col,
        int dc, int dr,
        CellState target)
    {
        int count = 0;
        int r = row + dr;
        int c = col + dc;

        while (r >= 0 && r < boardSize && c >= 0 && c < boardSize)
        {
            if (cells[r * boardSize + c] != target) break;
            count++;
            r += dr;
            c += dc;
        }

        return count;
    }
}
