using UnityEngine;

public class Tile : MonoBehaviour
{
    private static readonly Vector3 CenterOffset = new(0.5f, 0.5f, 0f);
    
    [SerializeField] private Material _slipperyColor;
    [SerializeField] private Material _walkableColor;
    [SerializeField] private Material _playerHighlight;
    [SerializeField] private Material _enemyHighLight;
    [SerializeField] private Material _capturingMaterial;
    [SerializeField] private MeshRenderer _renderer;

    private TileType _originalType;
    private Transform _transform;

    public TileType TileType { get; private set; }

    public Vector2Int Coordinates { get; private set; }

    public Vector3 Center => _transform.position + CenterOffset;

    public void Init(TileType type, Vector2Int coordinates)
    {
        _originalType = TileType = type;
        Coordinates = coordinates;
        SetColors();
        gameObject.name = $"{type.ToString().Substring(0, 3)} Tile {Coordinates.x} {Coordinates.y}";
    }

    public void ResetToDefault()
    {
        TileType = _originalType;
        SetHighlightEnabled(false);
        SetColors();
    }

    private void Awake()
    {
        _transform = gameObject.transform;
    }

    private void Start()
    {
        SetColors();
    }

    private void SetBorderMaterial(Material material)
    {
        var materials = _renderer.materials;
        const int borderMaterialIndex = 0;
        materials[borderMaterialIndex] = material;
        _renderer.materials = materials;
    }

    private void SetColors()
    {
        if (TileType == TileType.Slippery)
        {
            SetBorderMaterial(_slipperyColor);
        }
        else if (TileType == TileType.Walkable)
        {
            SetBorderMaterial(_walkableColor);
        }
    }

    public void SetHighlightEnabled(bool highlightEnabled, bool enemy = false)
    {
        if (highlightEnabled)
        {
            SetBorderMaterial(enemy ? _enemyHighLight : _playerHighlight);
        }
        else
        {
            SetColors();
        }
    }

    public void MarkCapturing()
    {
        SetBorderMaterial(_capturingMaterial);
    }

    public void MarkCaptured()
    {
        TileType = TileType.Walkable;
        SetColors();
    }
}