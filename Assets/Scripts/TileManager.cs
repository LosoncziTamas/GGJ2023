using UnityEngine;

public class TileManager : MonoBehaviour
{
    public const int ColumnCount = 18;
    public const int RowCount = 11;

    [SerializeField] private Tile _tilePrefab;

    private Tile[] _tiles;

    private void Awake()
    {
        if (_tiles == null || _tiles.Length == 0)
        {
            Build();
        }
    }

    private void Build()
    {
        _tiles = new Tile[ColumnCount * RowCount];
        var offsetX = 1.0f;
        var offsetY = 1.0f;
        var offsetZ = -0.4f;
        var startX = -9.0f;
        var startY = -5.5f;
        for (var rowIndex = 0; rowIndex < RowCount; rowIndex++)
        {
            for (var columnIndex = 0; columnIndex < ColumnCount; columnIndex++)
            {
                var tile = Instantiate(_tilePrefab, transform);
                var x = startX + columnIndex * offsetX;
                var y = startY + rowIndex * offsetY;
                tile.transform.position = new Vector3(x, y, offsetZ);
                var type = IsWalkableTile(rowIndex, columnIndex) ? TileType.Walkable : TileType.Slippery;
                var coordinates = new Vector2Int(columnIndex, rowIndex);
                tile.Init(type, coordinates);
            }
        }
    }

    private static bool IsWalkableTile(int rowIndex, int columnIndex)
    {
        return rowIndex == 0 || columnIndex == 0 || rowIndex == RowCount - 1 || columnIndex == ColumnCount - 1;
    }
}
