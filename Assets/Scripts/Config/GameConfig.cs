using UnityEngine;

/// <summary>
/// Central configuration for the game. Create one instance via
/// Assets > Create > TicTacToe > GameConfig and assign it wherever needed.
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "TicTacToe/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Board")]
    [SerializeField] private int _boardSize = 15;
    [SerializeField] private int _winLength = 5;

    [Header("Players")]
    [SerializeField] private string _playerXSymbol = "X";
    [SerializeField] private string _playerOSymbol = "O";

    [Header("SFX")]
    [SerializeField] private AudioClip _placePieceClip;
    [SerializeField] private AudioClip _winClip;
    [SerializeField] private AudioClip _drawClip;
    [SerializeField] private AudioClip _loseClip;

    public int BoardSize => _boardSize;
    public int WinLength => _winLength;
    public int CellCount => _boardSize * _boardSize;
    public string PlayerXSymbol => _playerXSymbol;
    public string PlayerOSymbol => _playerOSymbol;
    public AudioClip PlacePieceClip => _placePieceClip;
    public AudioClip WinClip => _winClip;
    public AudioClip DrawClip => _drawClip;
    public AudioClip LoseClip => _loseClip;
}
