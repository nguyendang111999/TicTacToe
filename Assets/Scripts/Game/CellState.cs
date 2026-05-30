/// <summary>
/// Represents the occupancy of a single board cell.
/// Byte backing keeps the SyncVar byte[] array at 1 byte per cell (225 bytes total for 15x15).
/// Mirror serializes enum backing types natively — no custom reader/writer needed.
/// </summary>
public enum CellState : byte
{
    Empty = 0,
    X     = 1,
    O     = 2,
}
