using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Visual representation of a single board cell.
/// Attach to the Cell prefab alongside a Button component.
/// The parent <see cref="BoardView"/> assigns <see cref="Index"/> and subscribes to the Button click.
/// </summary>
[RequireComponent(typeof(Button))]
public class CellView : MonoBehaviour
{
    [SerializeField] private TMP_Text _symbolLabel;
    [SerializeField] private Image _background;

    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _highlightColor = new Color(1f, 0.85f, 0f); // gold

    private Button _button;

    public int Index { get; private set; }

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    /// <summary>Called by <see cref="BoardView"/> after instantiation to wire up the index and click callback.</summary>
    public void Initialize(int index, System.Action<int> onClicked)
    {
        Index = index;
        _button.onClick.AddListener(() => onClicked(Index));
    }

    /// <summary>Updates the cell's symbol label to reflect the current <see cref="CellState"/>.</summary>
    public void SetState(CellState state)
    {
        _symbolLabel.text = state switch
        {
            CellState.X => "X",
            CellState.O => "O",
            _ => string.Empty,
        };

        _button.interactable = state == CellState.Empty;
    }

    /// <summary>Highlights the cell to indicate it is part of the winning line.</summary>
    public void SetHighlight(bool highlight)
    {
        if (_background != null)
            _background.color = highlight ? _highlightColor : _normalColor;
    }
}
