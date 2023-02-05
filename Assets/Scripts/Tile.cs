using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private TileType _tileType;
    [SerializeField] private GameObject _highlightObject;
    [SerializeField] private Material _slipperyColor;
    [SerializeField] private Material _walkableColor;
    [SerializeField] private Material _playerHighlight;
    [SerializeField] private Material _enemyHighLight;
    [SerializeField] private Renderer _highlightObjectRenderer;
    
    private Renderer _renderer;
    private TileType _originalType;
    
    public TileType TileType => _tileType;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _originalType = _tileType;
    }

    public void ResetToDefault()
    {
        _tileType = _originalType;
        SetHighlightEnabled(false);
        SetColors();
    }

    private void Start()
    {
        SetColors();
    }

    private void SetColors()
    {
        if (_tileType == TileType.Slippery)
        {
            _renderer.material = _slipperyColor;
        }
        else if (_tileType == TileType.Walkable)
        {
            _renderer.material = _walkableColor;
        }
    }

    public void SetHighlightEnabled(bool highlightEnabled, bool enemy = false)
    {
        _highlightObject.gameObject.SetActive(highlightEnabled);
        if (highlightEnabled)
        {
            _highlightObjectRenderer.material = enemy ? _enemyHighLight : _playerHighlight;
        }
    }

    public void MarkResolved()
    {
        _tileType = TileType.Walkable;
        SetColors();
    }

    public bool OriginallyWalkable() => _originalType == TileType.Walkable;
    public bool OriginallySlippery() => _originalType == TileType.Slippery;
}