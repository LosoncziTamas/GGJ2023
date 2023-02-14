using System.Collections.Generic;
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
                _tiles[rowIndex * ColumnCount + columnIndex] = tile;
            }
        }
    }

    public Tile GetTileByCoordinates(Vector2Int coords)
    {
        var outOfBounds = coords.y is < 0 or >= RowCount || coords.x is < 0 or >= ColumnCount;
        if (outOfBounds)
        {
            return null;
        }
        return _tiles[coords.y * ColumnCount + coords.x];
    }

    public List<Tile> GetTileByType(TileType type)
    {
        var result = new List<Tile>();
        foreach (var tile in _tiles)
        {
            if (type == tile.TileType)
            {
                result.Add(tile);
            }
        }
        return result;
    }

    public List<Tile> GetSurroundingTiles(Tile tile)
    {
        var tileCoord = tile.Coordinates;
        var tileBelow = GetTileByCoordinates(new Vector2Int(tileCoord.x, tileCoord.y - 1));
        var tileAbove = GetTileByCoordinates(new Vector2Int(tileCoord.x, tileCoord.y + 1));
        var tileLeft = GetTileByCoordinates(new Vector2Int(tileCoord.x - 1, tileCoord.y));
        var tileRight = GetTileByCoordinates(new Vector2Int(tileCoord.x + 1, tileCoord.y));
        return new List<Tile>() {tileAbove, tileBelow, tileLeft, tileRight};
    }

    private static bool IsWalkableTile(int rowIndex, int columnIndex)
    {
        return rowIndex == 0 || columnIndex == 0 || rowIndex == RowCount - 1 || columnIndex == ColumnCount - 1;
    }
}
